using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;
using VRageMath;
using static FSTC.FSTCData;
using static FSTC.FSTCData.EmpireData;

namespace FSTC {
  public static class Diplomacy {

    private static readonly long FACTION_REFRESH_TIMER = Util.TickSeconds(1); // 1 sec
    private static readonly long REPUTATION_REFRESH_TIMER = Util.TickMinutes(1); // 1 sec

    /**
     * Empire classification.
     * Determines how this empire reacts diplomatically to other empires.
     */
    public enum EmpireType {
      // True Neutrals will never go to war.
      TRUE_NEUTRAL = 0,
      // Neutrals will go to war if provoked, but can eventually be pursuaded to peace.
      NEUTRAL = 1,
      // Police are normally neutral, however will go to war for short periods of time to protect any Neutral class.
      POLICE = 2,
      // Hostiles start at war, however, can be pursuaded to peace for a time.
      HOSTILE = 3,
      // True Hostiles have no desire for peace.
      TRUE_HOSTILE = 4,
      // Player owned
      PLAYER = 5
    };

    /**
     * Inject refresh events for diplomacy.
     */
    public static void Initialize() {
      EventManager.AddEvent(GlobalData.world.currentTick + FACTION_REFRESH_TIMER, RefreshPlayerFactions);
      EventManager.AddEvent(GlobalData.world.currentTick + REPUTATION_REFRESH_TIMER, RefreshReputation);
    }

    /**
     * Refreshes the empire list to reflect the player factions.
     */
    public static void RefreshPlayerFactions(object unused) {
      List<EmpireData> newEmpires = new List<EmpireData>();
      foreach (IMyFaction faction in MyAPIGateway.Session.Factions.Factions.Values) {
        if (faction.Tag.Length > 3 || GlobalData.world.empires.Find(e => e.empireTag == faction.Tag) != null) {
          continue;
        }
        EmpireData playerEmpire = new EmpireData {
          empireTag = faction.Tag,
          empireType = (int)Diplomacy.EmpireType.PLAYER,
          bounds = new BoundingBoxD(new Vector3D(-0.0, -0.0, -0.0), new Vector3D(0, 0, 0)),
          m_faction = faction
        };
        newEmpires.Add(playerEmpire);
        Util.Log("Adding Diplomacy for player faction: " + faction.Tag);
      }
      if (newEmpires.Count > 0) {
        GlobalData.world.empires.AddRange(newEmpires);
        RefreshReputation(null);
      }
      EventManager.AddEvent(GlobalData.world.currentTick + FACTION_REFRESH_TIMER, RefreshPlayerFactions);
    }

    /**
     * Refresh reputation
     */
    public static void RefreshReputation(object unused) {
      foreach (EmpireData empireA in GlobalData.world.empires) {
        if ((EmpireType) empireA.empireType == EmpireType.PLAYER) {
          continue;
        }
        foreach (EmpireData empireB in GlobalData.world.empires) {
          if (empireA == empireB) {
            continue;
          }
          EmpireStanding standing = FindStandings(empireA, empireB);
          if (standing == null) {
            standing = GetDefaultStanding(empireA, empireB);
            if (standing == null) {
              Util.Warning("Could not repute " + empireA.empireTag + " and " + empireB.empireTag);
              continue;
            }
            empireA.standings.Add(standing);
          }

          // Reputation tends towards 0
          int standingsChange = 0;
          if (standing.reputation > 0) {
            standingsChange = -1;
          } else if (standing.reputation < 0) {
            standingsChange = 1;
          }
          // But war holds reputation static for the duration of the war.
          if (!standing.atWar) {
            standing.reputation += standingsChange;
          }

          // If we're normally peaceful, try to stay peaceful.
          if (standing.atWar && PeaceIsDefault(empireA) && GlobalData.world.currentTick >= standing.inStateTill) {
            standing.atWar = false;
            standing.inStateTill = long.MaxValue;
          }

          // If we're normally war-like, try to go to war.
          if (!standing.atWar && WarIsDefault(empireA) && GlobalData.world.currentTick >= standing.inStateTill) {
            standing.atWar = true;
            standing.inStateTill = long.MaxValue;
          }

          // If we're not at war, accept any peace offers.
          if (!standing.atWar && MyAPIGateway.Session.Factions.IsPeaceRequestStatePending(empireA.m_faction.FactionId, empireB.m_faction.FactionId)) {
            MyAPIGateway.Session.Factions.AcceptPeace(empireA.m_faction.FactionId, empireB.m_faction.FactionId);
          }

          if (!standing.atWar) {
            TryDeclarePeace(empireA, empireB);
          } else {
            TryDeclareWar(empireA, empireB);
          }
        }
      }

      EventManager.AddEvent(GlobalData.world.currentTick + REPUTATION_REFRESH_TIMER, RefreshReputation);
    }

    /**
     * Locate the standing information of what empire A thinks of empire B,
     * or null if no standings are available.
     */
    public static EmpireStanding FindStandings(EmpireData empireA, EmpireData empireB) {
      foreach (EmpireStanding standing in empireA.standings) {
        if (standing.empireTag == empireB.empireTag) {
          return standing;
        }
      }
      return null;
    }

    /**
     * Determine based on empire A's type, how it default feels about empire B.
     */
    private static EmpireStanding GetDefaultStanding(EmpireData empireA, EmpireData empireB) {
      Util.Log("Generating default standing for " + empireA.empireTag + " and " + empireB.empireTag);
      EmpireStanding standing = new EmpireStanding();
      standing.inStateTill = long.MaxValue;
      standing.reputation = 0;
      standing.empireTag = empireB.empireTag;

      switch ((EmpireType)empireA.empireType) {
        case EmpireType.TRUE_HOSTILE:
        case EmpireType.HOSTILE:
          standing.atWar = true;
          break;
        case EmpireType.NEUTRAL:
        case EmpireType.POLICE:
        case EmpireType.TRUE_NEUTRAL:
        case EmpireType.PLAYER:
          standing.atWar = false;
          break;
      }

      return standing;
    }

    private static void TryDeclareWar(EmpireData empireA, EmpireData empireB) {
      if (empireA.m_faction == null || empireB.m_faction == null) {
        return;
      }
      if (MyAPIGateway.Session.Factions.IsPeaceRequestStateSent(empireA.m_faction.FactionId, empireB.m_faction.FactionId)) {
        MyAPIGateway.Session.Factions.CancelPeaceRequest(empireA.m_faction.FactionId, empireB.m_faction.FactionId);
      }
      if (MyAPIGateway.Session.Factions.AreFactionsEnemies(empireA.m_faction.FactionId, empireB.m_faction.FactionId)) {
        return;
      }
      Util.Log("Empires go to war: " + empireA.empireTag + " and " + empireB.empireTag);
      MyAPIGateway.Session.Factions.DeclareWar(empireA.m_faction.FactionId, empireB.m_faction.FactionId);
    }

    private static void TryDeclarePeace(EmpireData empireA, EmpireData empireB) {
      if (empireA.m_faction == null || empireB.m_faction == null) {
        return;
      }
      if (!MyAPIGateway.Session.Factions.AreFactionsEnemies(empireA.m_faction.FactionId, empireB.m_faction.FactionId)) {
        return;
      }
      if (MyAPIGateway.Session.Factions.IsPeaceRequestStateSent(empireA.m_faction.FactionId, empireB.m_faction.FactionId)) {
        Util.Log("Empires beg for peace: " + empireA.empireTag + " and " + empireB.empireTag);
        return;
      }
      Util.Log("Empires try to restore peace: " + empireA.empireTag + " and " + empireB.empireTag);
      MyAPIGateway.Session.Factions.SendPeaceRequest(empireA.m_faction.FactionId, empireB.m_faction.FactionId);
    }

    private static bool PeaceIsDefault(EmpireData empire) {
      return !WarIsDefault(empire);
    }

    private static bool WarIsDefault(EmpireData empire) {
      return (EmpireType)empire.empireType == EmpireType.TRUE_HOSTILE
          || (EmpireType)empire.empireType == EmpireType.HOSTILE;
    }

    private static bool WillAcceptPeace(EmpireData empire) {
      return (EmpireType)empire.empireType != EmpireType.TRUE_HOSTILE;
    }

    private static bool WarIsTransient(EmpireData empire) {
      return (EmpireType)empire.empireType == EmpireType.NEUTRAL ||
             (EmpireType)empire.empireType == EmpireType.POLICE;
    }

    private static bool PeaceIsTransient(EmpireData empire) {
      return (EmpireType)empire.empireType == EmpireType.HOSTILE;
    }
  }
}
