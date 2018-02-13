using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sandbox.Common;
using Sandbox.Common.ObjectBuilders;
using Sandbox.Definitions;
using Sandbox.Game;
using Sandbox.Game.Entities;
using Sandbox.Game.EntityComponents;
using Sandbox.ModAPI.Ingame;
using Sandbox.ModAPI.Interfaces;
using SpaceEngineers.Game.ModAPI;
using VRage.Game;
using VRage.Game.Components;
using VRage.Game.Entity;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;
using VRage.Utils;
using VRageMath;

namespace FSTC {

  public class SpawnManager {

		// Spawner Timing
		int spawnTickIntervalMin = 600; //Placeholder value. Real value is pulled from GlobalEvents.sbc
		int spawnTickIntervalMax = 6000; //Placeholder value. Real value is pulled from GlobalEvents.sbc

    // Spawner Prefab Data
    private List<MySpawnGroupDefinition> m_spawnGroups = new List<MySpawnGroupDefinition>();
    private int m_spawnerCost = 0;
    private string m_factionTag;
    private long m_factionFounder;

    // Player Ship Tracker

    /**
     * Init the spawner.
     * Searches for any spawn groups for the given faction.
     * If no faction is specified, attempts to become the Space Pirate spawner.
     * If no spawn groups exist in the SpawnGroups.sbc file, then this spawner
     * becomes premanently inactive.
     */
    public void Initialize(string factionTag) {
      if (factionTag == null) {
        m_factionTag = "SPRT";
      } else {
        m_factionTag = factionTag;
      }
      ReadSpawnConfig();
      GetAllSpawnGroups();
      GetFounder();
      if (m_spawnGroups.Count() > 0) {
        RegisterNextSpawn();
      }
    }

    /**
     * Enqueues an event to trigger another wave of spawning at a random interval into the future.
     */
    private void RegisterNextSpawn() {
      long targetTick = Util.currentTick + Util.rand.Next(spawnTickIntervalMin, spawnTickIntervalMax);
      EventManager.AddEvent(targetTick, Event_SpawnNPCShips);
    }

    /**
     * Event callback, spawns a round of NPC ships for the faction.
     * Registers the next spawn event.
     */
    public void Event_SpawnNPCShips() {
      SpawnForGroup(GetRandomSpawnGroup());
      RegisterNextSpawn();
    }

    /**
     * Select a random spawn group based on the spawner frequencies
     */
    private int GetRandomSpawnGroup() {
      int weight = Util.rand.Next(0, m_spawnerCost);
      for (int i = 0; i < m_spawnGroups.Count(); ++i) {
        MySpawnGroupDefinition def = m_spawnGroups[i];
        weight -= (Int32)Math.Ceiling(def.Frequency);
        if (weight <= 0) {
          return i;
        }
      }
      return 0;
    }

    /**
     * Spawn the ships in the given spawner group.
     */
    private void SpawnForGroup(int group) {
      IMyPlayer spawnTarget = GetRandomPlayer();
      if (spawnTarget == null) {
        return;
      }

      Vector3D spawnPos = new Vector3D(-100,0,0);
      Vector3D deSpawnPos = new Vector3D(0,0,0);
      Vector3D lookDir = new Vector3D(1,0,0);
      Vector3D upDir = new Vector3D(0,1,0);

      MySpawnGroupDefinition prefabGroup = m_spawnGroups[group];
      foreach (var prefab in prefabGroup.Prefabs) {
        // TODO: tranform sub-spawns correctly
        Vector3D prefabSpawnPos = spawnPos;
        Vector3D prefabDeSpawnPos = spawnPos + (lookDir * 1000.0);
        SpawnCargoShip(prefab.SubtypeId, prefabSpawnPos, prefabDeSpawnPos, prefab.Speed, prefab.BeaconText);
      }
    }

    /**
     * Get a random player to spawn off of.
     */
    private IMyPlayer GetRandomPlayer() {
      List<IMyPlayer> allPlayers = new List<IMyPlayer>();
      MyAPIGateway.Players.GetPlayers(allPlayers);
      if (allPlayers.Count() == 0) {
        return null;
      }
      List<IMyPlayer> playerList = allPlayers.FindAll(p => p.IsBot == false);
      return playerList[Util.rand.Next(playerList.Count())];
    }

    /**
     * Generate the prefab
     */
    private void SpawnCargoShip(string prefab, Vector3D spawnCoords, Vector3D despawnCoords, float prefabSpeed, string beaconName) {
      Util.Log("Spawning Prefab ::" + prefab + ":: for faction " + m_factionTag);
      Vector3D spawnFacing = Vector3D.Normalize(despawnCoords - spawnCoords);
      List<IMyCubeGrid> tempSpawningList = new List<IMyCubeGrid>();
      MyAPIGateway.PrefabManager.SpawnPrefab(
          resultList: tempSpawningList,
          prefabName: prefab,
          position: spawnCoords,
          forward: spawnFacing,
          up: new Vector3D(0,1,0),
          spawningOptions: SpawningOptions.SetNeutralOwner | SpawningOptions.RotateFirstCockpitTowardsDirection | SpawningOptions.SpawnRandomCargo,
          beaconName: beaconName,
          ownerId: m_factionFounder,
          updateSync: false);
    }

    /**
     * Read the GlobalEvents.sbc data pertaining to the spawn rate for the ships.
     * The "first" time field is ignored, and only the min/max are used.
     */
    private void ReadSpawnConfig() {
			var spawnShipGlobalEvent = MyDefinitionManager.Static.GetEventDefinition(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "FSSpawnerTick"));
			if(spawnShipGlobalEvent != null){
				// TimeSpan eventFirst = (TimeSpan)spawnShipGlobalEvent.FirstActivationTime;
				TimeSpan eventMin = (TimeSpan)spawnShipGlobalEvent.MinActivationTime;
				TimeSpan eventMax = (TimeSpan)spawnShipGlobalEvent.MaxActivationTime;
				spawnTickIntervalMin = (Int32)eventMin.TotalSeconds * 60;
				spawnTickIntervalMax = (Int32)eventMax.TotalSeconds * 60;
			}
    }

    /**
     * Init the spawn group list to contain all the spawner groups which contain the tag
     *     (FACTION)
     * in their name.
     */
    private void GetAllSpawnGroups() {
      var allSpawnGroups = MyDefinitionManager.Static.GetSpawnGroupDefinitions();
      string tag = "(" + m_factionTag + ")";
      foreach (var spawnGroup in allSpawnGroups) {
        if (spawnGroup.IsEncounter) {
          continue;
        }
        if (!spawnGroup.Id.SubtypeName.Contains(tag)) {
          continue;
        }
        m_spawnGroups.Add(spawnGroup);

        // Compute the total cost of all the spawners in order to be able to pick a random group.
        m_spawnerCost += (Int32)Math.Ceiling(spawnGroup.Frequency);
      }
      Util.Log("Found " + m_spawnGroups.Count() + " " + m_factionTag + " SpawnGroups.");
    }

    /**
     * Determine the founder entity for the faction we were initialized with.
     * This is used to set the correct ownership on the spawned ships.
     */
    private void GetFounder() {
      m_factionFounder = MyAPIGateway.Session.Factions.TryGetFactionByTag(m_factionTag).FounderId;
    }
  }

}  // namespace FSTC
