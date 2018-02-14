using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Definitions;
using VRage.Game;
using VRage.Game.ModAPI;
using VRageMath;

namespace FSTC {

  public class SpawnManager {

    private static readonly string DATA_TAG_TYPE = "TYPE";
    private static readonly string DATA_TAG_COST = "COST";

		// Spawner Timing
		int spawnTickIntervalFirst = 600; //Placeholder value. Real value is pulled from GlobalEvents.sbc
		int spawnTickIntervalMin = 600; //Placeholder value. Real value is pulled from GlobalEvents.sbc
		int spawnTickIntervalMax = 6000; //Placeholder value. Real value is pulled from GlobalEvents.sbc

    /**
     *
     */
    public enum EncounterType {
      Static,
      Transient
    };

    /**
     * Generic classes of ship spawn
     */
    public enum SpawnerClass {
      ANY,

      STATIC_STRUCTURE,
      MILITARY,
      CIVILIAN
    };

    private static readonly List<string> SPAWNER_CLASS_NAMES = new List<string>() {
      "<any>",

      "SS",
      "M",
      "C"
    };

    /**
     * Specific ship classes
     */
    public enum SpawnerType {
      ANY,

      STATIC_STRUCTURE_HQ,
      STATIC_STRUCTURE_DEFENSE_PLATFORM,
      STATIC_STRUCTURE_MILITART_OUTPOST,
      STATIC_STRUCTURE_TRADE_OUTPOST,
      STATIC_STRUCTURE_CIVILIAN_OUTPOST,
      STATIC_STRUCTURE_GENERIC_BEACON,
      STATIC_STRUCTURE_NAV_BEACON,

      MILITARY_FIGHTER,
      MILITARY_CORVETTE,
      MILITARY_FRIGATE,
      MILITARY_CRUISER,
      MILITARY_BATTLESHIP,
      MILITARY_MOTHERSHIP,

      CIVILIAN_TRANSPORT,
      CIVILIAN_CARGO,
      CIVILIAN_EXPLORATION
    };

    /**
     * Tags to parse from spawner groups that match the SpawnerType enum above.
     */
    private static readonly List<string> SPAWNER_TYPE_NAMES = new List<string>() {
      "<any>",

      // Static Structures
      // Base of operations.
      "SS:HQ",
      // Defense Platform
      "SS:DFPT",
      // Military Outpost
      "SS:MOUT",
      // Trade Outpost
      "SS:TOUT",
      // Civilian Outpost
      "SS:COUT",
      // Generic Transponder Beacon
      "SS:BCON",
      // Nav Transponder Beacon
      "SS:NAV",

      // Military ships
      // Fighter
      "M:FTR",
      // Corvet
      "M:CVT",
      // Frigate
      "M:FRG",
      // Cruiser
      "M:CRU",
      // Battleship
      "M:BS",
      // Mothership
      "M:MS",

      // Civilian ships
      // Personel Transport
      "C:TRA",
      // Cargo Ship
      "C:CRGO",
      // Exploration Vessel
      "C:EXP",
    };

    /**
     * Information about a spawn group parsed from the name.
     */
    public class GroupInfo {
      public int m_cost;
      public SpawnerClass m_spawnClass;
      public SpawnerType m_spawnType;
      public MySpawnGroupDefinition m_spawnGroupDef;

      public GroupInfo() {
        m_cost = 0;
        m_spawnClass = SpawnerClass.ANY;
        m_spawnType = SpawnerType.ANY;
        m_spawnGroupDef = null;
      }

      public GroupInfo(int cost, SpawnerClass spawnClass, SpawnerType spawnType, MySpawnGroupDefinition spawnGroup) {
        m_cost = cost;
        m_spawnClass = spawnClass;
        m_spawnType = spawnType;
        m_spawnGroupDef = spawnGroup;
      }
    };

    private List<GroupInfo> m_spawnGroups = new List<GroupInfo>();
    private IMyFaction m_faction;

    private List<FSTCData.SpawnedStructure> m_savedStatics = new List<FSTCData.SpawnedStructure>();
    private List<FSTCData.SpawnedShip> m_savedDynamics = new List<FSTCData.SpawnedShip>();

    // Player Ship Tracker

    /**
     * Init the spawner.
     * Searches for any spawn groups for the given faction.
     * If no faction is specified, attempts to become the Space Pirate spawner.
     * If no spawn groups exist in the SpawnGroups.sbc file, then this spawner
     * becomes premanently inactive.
     */
    public SpawnManager(IMyFaction factionOwner, List<FSTCData.SpawnedStructure> savedStatics, List<FSTCData.SpawnedShip> savedDynamics) {
      m_faction = factionOwner;
      m_savedStatics = savedStatics;
      m_savedDynamics = savedDynamics;
      ReadSpawnConfig();
      GetAllSpawnGroups();
    }

    public void Save(out List<FSTCData.SpawnedStructure> savedStatics, out List<FSTCData.SpawnedShip> savedDynamics) {
      savedStatics = m_savedStatics;
      savedDynamics = m_savedDynamics;
    }

    public void GetRandomSpawnGroup(SpawnerClass spawnClass, int maxCost, out GroupInfo info) {
      List<GroupInfo> matchingGroups = m_spawnGroups.FindAll(g => (g.m_spawnClass == spawnClass) && (g.m_cost < maxCost));
      info = GetRandomSpawnGroup(matchingGroups);
    }

    public void GetRandomSpawnGroup(SpawnerType spawnType, int maxCost, out GroupInfo info) {
      List<GroupInfo> matchingGroups = m_spawnGroups.FindAll(g => (g.m_spawnType == spawnType) && (g.m_cost < maxCost));
      info = GetRandomSpawnGroup(matchingGroups);
    }

    /**
     * Spawn the group randomly within a region.
     */
    public void SpawnForRegion(GroupInfo group, EncounterType encounterType, BoundingBoxD region) {
      Vector3D regionExtent = (region.Max - region.Min);
      Vector3D randomSpawn = region.Min + (regionExtent * Util.rand.NextDouble());
      SpawnForGroup(group, encounterType, randomSpawn);
    }

    /**
     * Spawn at the edge of a 10Km sphere around the player
    public void SpawnForPlayer(GroupInfo group, IMyPlayer player) {

      SpawnForGroup(group, randomSpawn);
    }
     */

    /**
     * Spawn at the edge of a 200m sphere around the entity
    public void SpawnForEntity(GroupInfo group, IMyEntity entity) {

      SpawnForGroup(group, randomSpawn);
    }
     */

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
    public void Event_SpawnNPCShips(object unused) {
      // SpawnForGroup(GetRandomSpawnGroup());
      RegisterNextSpawn();
    }

    /**
     * Select a random spawn group based on the spawner frequencies
     */
    private GroupInfo GetRandomSpawnGroup(List<GroupInfo> infos) {
      int spawnerWeightTotal = infos.Sum(g => (int) Math.Ceiling(g.m_spawnGroupDef.Frequency));
      int weight = Util.rand.Next(0, spawnerWeightTotal);
      for (int i = 0; i < infos.Count(); ++i) {
        weight -= (Int32)Math.Ceiling(infos[i].m_spawnGroupDef.Frequency);
        if (weight <= 0) {
          return infos[i];
        }
      }
      return new GroupInfo();
    }

    /**
     * Spawn the ships in the given spawner group.
     */
    private void SpawnForGroup(GroupInfo group, EncounterType encounterType, Vector3D spawnCenter) {
      if (group.m_spawnGroupDef == null) {
        return;
      }

      Vector3D deSpawnPos = new Vector3D(0,0,0);
      Vector3D lookDir = new Vector3D(1,0,0);
      Vector3D upDir = new Vector3D(0,1,0);

      foreach (MySpawnGroupDefinition.SpawnGroupVoxel prefab in group.m_spawnGroupDef.Voxels) {

      }
      foreach (MySpawnGroupDefinition.SpawnGroupPrefab prefab in group.m_spawnGroupDef.Prefabs) {
        // TODO: tranform sub-spawns correctly
        Vector3D prefabSpawnPos = spawnCenter;
        Vector3D prefabDeSpawnPos = spawnCenter + (lookDir * 1000.0);
        SpawnCargoShip(prefab.SubtypeId, prefabSpawnPos, prefabDeSpawnPos, prefab.Speed, prefab.BeaconText, encounterType);
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
    private void SpawnCargoShip(string prefab, Vector3D spawnCoords, Vector3D despawnCoords, float prefabSpeed, string beaconName, EncounterType encounterType) {
      Util.Log("Spawning Prefab ::" + prefab + ":: for faction " + m_faction.Tag);
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
          ownerId: m_faction.FounderId,
          updateSync: false,
          callback: () => SpawnerCallback(encounterType, tempSpawningList));
    }

    public void SpawnerCallback(EncounterType encounterType, List<IMyCubeGrid> spawnList) {

    }

    /**
     * Read the GlobalEvents.sbc data pertaining to the spawn rate for the ships.
     * The "first" time field is ignored, and only the min/max are used.
     */
    private void ReadSpawnConfig() {
      MyGlobalEventDefinition spawnShipGlobalEvent;
			MyDefinitionManager.Static.TryGetDefinition(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "FSSpawnerTick[" + m_faction.Tag + "]"), out spawnShipGlobalEvent);
			if (spawnShipGlobalEvent == null) {
        MyDefinitionManager.Static.TryGetDefinition(new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "FSSpawnerTick"), out spawnShipGlobalEvent);
      }
			if(spawnShipGlobalEvent == null){
        return;
      }
      TimeSpan eventFirst = (TimeSpan)spawnShipGlobalEvent.FirstActivationTime;
      TimeSpan eventMin = (TimeSpan)spawnShipGlobalEvent.MinActivationTime;
      TimeSpan eventMax = (TimeSpan)spawnShipGlobalEvent.MaxActivationTime;
      spawnTickIntervalFirst = (Int32)eventFirst.TotalSeconds * 60;
      spawnTickIntervalMin = (Int32)eventMin.TotalSeconds * 60;
      spawnTickIntervalMax = (Int32)eventMax.TotalSeconds * 60;
    }

    /**
     * Init the spawn group list to contain all the spawner groups which contain the tag
     *     (FACTION)
     * in their name.
     */
    private void GetAllSpawnGroups() {
      var allSpawnGroups = MyDefinitionManager.Static.GetSpawnGroupDefinitions();
      string tag = "(" + m_faction.Tag + ")";
      foreach (var spawnGroup in allSpawnGroups) {
        if (spawnGroup.IsEncounter) {
          continue;
        }
        if (!spawnGroup.Id.SubtypeName.Contains(tag)) {
          continue;
        }

        int dataStart = spawnGroup.Id.SubtypeName.IndexOf('[');
        int dataEnd = spawnGroup.Id.SubtypeName.IndexOf(']');
        if (dataStart == -1 || dataEnd == -1) {
          Util.Warning("Skipping spawn group, due to bad data tag: " + spawnGroup.Id.SubtypeName);
          continue;
        }
        string[] dataTags = spawnGroup.Id.SubtypeName.Substring(dataStart + 1, Math.Max((dataEnd - 1) - (dataStart+1), 0)).ToUpper().Split(',');
        int cost = 0;
        SpawnerClass spawnClass = SpawnerClass.ANY;
        SpawnerType spawnType = SpawnerType.ANY;
        foreach(string data in dataTags) {
          try {
            if (data.StartsWith(DATA_TAG_TYPE)) {
              string tmp = data.Substring(DATA_TAG_TYPE.Length + 1);
              foreach(SpawnerClass c in Enum.GetValues(typeof(SpawnerClass))) {
                if (tmp.StartsWith(SPAWNER_CLASS_NAMES[(int) c])) {
                  spawnClass = c;
                  break;
                }
              }
              foreach(SpawnerType t in Enum.GetValues(typeof(SpawnerType))) {
                if (tmp.StartsWith(SPAWNER_TYPE_NAMES[(int) t])) {
                  spawnType = t;
                  break;
                }
              }
            } else if (data.StartsWith(DATA_TAG_COST)) {
              string tmp = data.Substring(DATA_TAG_TYPE.Length + 1);
              cost = Int32.Parse(tmp);
            } else {
              Util.Warning("Skipping bad data tag: " + data);
            }
          } catch (Exception e) {
            Util.Warning("Skipping bad data tag: " + data + " because " + e.ToString());
          }
        }

        m_spawnGroups.Add(new GroupInfo(cost, spawnClass, spawnType, spawnGroup));

      }
      Util.Log("Found " + m_spawnGroups.Count() + " " + m_faction.Tag + " SpawnGroups.");
    }
  }

}  // namespace FSTC
