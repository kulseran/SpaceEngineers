using System;
using System.Collections.Generic;
using System.Linq;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using static FSTC.FSTCData;

namespace FSTC {

  public class SpawnManager {

    // Data tag used in spawn config to represent the type of spawn
    // eg. TYPE:SS::HQ
    private static readonly string DATA_TAG_TYPE = "TYPE";
    // Data tag used in spawn config to represent the credit cost of the spawn
    // eg. COST:100
    private static readonly string DATA_TAG_COST = "COST";

    // SpawnPrefab callback is broken, and doesn't return the ship grid.
    // In order to find the ships we just spawned, we have to search for them.
    // This is the radius around the designated spawn point to look.
    private static readonly double SPAWNER_SEARCH_RADIUS = 10.0;

    private static readonly float CARGOSHIP_SPEED = 30.0f;
    private static readonly float CARGOSHIP_SPAWN_DIST = 8000f;

    /**
     *
     */
    public enum EncounterType {
      Static,
      TransientEncounter,
      TransientCargoship,
      TransientAttackship,
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

    /**
     * Specific ship classes
     */
    public enum SpawnerType {
      ANY,

      STATIC_STRUCTURE_HQ,
      STATIC_STRUCTURE_DEFENSE_PLATFORM,
      STATIC_STRUCTURE_MILITARY_OUTPOST,
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
     * Container for finding the SpawnerClass and SpawnerType from the string tag used in the
     * config file.
     */
    private class SpawnTag {
      public string m_tagString;
      public SpawnerClass m_spawnClass;
      public SpawnerType m_spawnType;
      public SpawnTag(string tagString, SpawnerClass spawnClass, SpawnerType spawnType) {
        m_tagString = tagString;
        m_spawnClass = spawnClass;
        m_spawnType = spawnType;
      }
    };

    /**
     * Tags to parse from spawner groups that match the SpawnerType enum above.
     */
    private static readonly List<SpawnTag> SPAWNER_TAGS = new List<SpawnTag>() {
      // Static Structures
      // Base of operations.
      new SpawnTag("SS:HQ", SpawnerClass.STATIC_STRUCTURE, SpawnerType.STATIC_STRUCTURE_HQ),
      // Defense Platform
      new SpawnTag("SS:DFPT", SpawnerClass.STATIC_STRUCTURE, SpawnerType.STATIC_STRUCTURE_DEFENSE_PLATFORM),
      // Military Outpost
      new SpawnTag("SS:MOUT", SpawnerClass.STATIC_STRUCTURE, SpawnerType.STATIC_STRUCTURE_MILITARY_OUTPOST),
      // Trade Outpost
      new SpawnTag("SS:TOUT", SpawnerClass.STATIC_STRUCTURE, SpawnerType.STATIC_STRUCTURE_TRADE_OUTPOST),
      // Civilian Outpost
      new SpawnTag("SS:COUT", SpawnerClass.STATIC_STRUCTURE, SpawnerType.STATIC_STRUCTURE_CIVILIAN_OUTPOST),
      // Generic Transponder Beacon
      new SpawnTag("SS:BCON", SpawnerClass.STATIC_STRUCTURE, SpawnerType.STATIC_STRUCTURE_GENERIC_BEACON),
      // Nav Transponder Beacon
      new SpawnTag("SS:NAV", SpawnerClass.STATIC_STRUCTURE, SpawnerType.STATIC_STRUCTURE_NAV_BEACON),

      // Military ships
      // Fighter
      new SpawnTag("M:FTR", SpawnerClass.MILITARY, SpawnerType.MILITARY_FIGHTER),
      // Corvet
      new SpawnTag("M:CVT", SpawnerClass.MILITARY, SpawnerType.MILITARY_CORVETTE),
      // Frigate
      new SpawnTag("M:FRG", SpawnerClass.MILITARY, SpawnerType.MILITARY_FRIGATE),
      // Cruiser
      new SpawnTag("M:CRU", SpawnerClass.MILITARY, SpawnerType.MILITARY_CRUISER),
      // Battleship
      new SpawnTag("M:BS", SpawnerClass.MILITARY, SpawnerType.MILITARY_BATTLESHIP),
      // Mothership
      new SpawnTag("M:MS", SpawnerClass.MILITARY, SpawnerType.MILITARY_MOTHERSHIP),

      // Civilian ships
      // Personel Transport
      new SpawnTag("C:TRA", SpawnerClass.CIVILIAN, SpawnerType.CIVILIAN_TRANSPORT),
      // Cargo Ship
      new SpawnTag("C:CRGO", SpawnerClass.CIVILIAN, SpawnerType.CIVILIAN_CARGO),
      // Exploration Vessel
      new SpawnTag("C:EXP", SpawnerClass.CIVILIAN, SpawnerType.CIVILIAN_EXPLORATION)
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

      public GroupInfo(
          int cost,
          SpawnerClass spawnClass,
          SpawnerType spawnType,
          MySpawnGroupDefinition spawnGroup) {
        m_cost = cost;
        m_spawnClass = spawnClass;
        m_spawnType = spawnType;
        m_spawnGroupDef = spawnGroup;
      }
    };

    private List<GroupInfo> m_spawnGroups = new List<GroupInfo>();

    private EmpireData m_empireData;
    private int m_spawnTickIntervalFirst;
    private int m_spawnTickIntervalMin;
    private int m_spawnTickIntervalMax;

    /**
     * Init the spawner.
     */
    public SpawnManager(EmpireData empireData) {
      m_empireData = empireData;
      GetAllSpawnGroups();
      ReRegisterDespawns();
      RegisterCargoShip();
    }

    /**
     * Find a random spawn group with the correct class and no more than maxCost.
     */
    public GroupInfo GetRandomSpawnGroup(SpawnerClass spawnClass, int maxCost) {
      List<GroupInfo> matchingGroups =
          m_spawnGroups.FindAll(g => (g.m_spawnClass == spawnClass) && (g.m_cost < maxCost));
      return GetRandomSpawnGroup(matchingGroups);
    }

    /**
     * Find a random spawn group with the correct type and no more than maxCost.
     */
    public GroupInfo GetRandomSpawnGroup(SpawnerType spawnType, int maxCost) {
      List<GroupInfo> matchingGroups =
          m_spawnGroups.FindAll(g => (g.m_spawnType == spawnType) && (g.m_cost < maxCost));
      return GetRandomSpawnGroup(matchingGroups);
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
     * Select a random spawn group based on the spawner frequencies
     */
    private GroupInfo GetRandomSpawnGroup(List<GroupInfo> infos) {
      if (infos == null || infos.Count == 0) {
        return null;
      }
      int spawnerWeightTotal = infos.Sum(g => (int)Math.Ceiling(g.m_spawnGroupDef.Frequency));
      int weight = Util.rand.Next(0, spawnerWeightTotal);
      for (int i = 0; i < infos.Count(); ++i) {
        weight -= (Int32)Math.Ceiling(infos[i].m_spawnGroupDef.Frequency);
        if (weight <= 0) {
          return infos[i];
        }
      }
      return null;
    }

    /**
     * Spawn the ships in the given spawner group.
     */
    private void SpawnForGroup(GroupInfo group, EncounterType encounterType, Vector3D spawnCenter) {
      if (group == null || group.m_spawnGroupDef == null) {
        return;
      }

      Vector3D deSpawnPos = new Vector3D(0, 0, 0);
      Vector3D lookDir = new Vector3D(1, 0, 0);
      Vector3D upDir = new Vector3D(0, 1, 0);

      foreach (MySpawnGroupDefinition.SpawnGroupVoxel prefab in group.m_spawnGroupDef.Voxels) {
        // TODO(): Support spawning in Voxel grids?
      }
      foreach (MySpawnGroupDefinition.SpawnGroupPrefab prefab in group.m_spawnGroupDef.Prefabs) {
        MatrixD groupMat;
        // Vector3D prefabStartCoords = Vector3D.Transform((Vector3D)prefab.Position, cargoShipPathStartMatrix);
        // TODO: tranform sub-spawns correctly
        Vector3D prefabSpawnPos = spawnCenter;
        Vector3D prefabDeSpawnPos = spawnCenter + (lookDir * 1000.0);
        SpawnPrefab(prefab, prefabSpawnPos, prefabDeSpawnPos, encounterType);
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
    private void SpawnPrefab(MySpawnGroupDefinition.SpawnGroupPrefab prefab, Vector3D spawnCoords, Vector3D despawnCoords, EncounterType encounterType) {
      Util.Log("Spawning Prefab ::" + prefab.SubtypeId + ":: for faction " + m_empireData.m_faction.Tag);
      Vector3D spawnFacing = Vector3D.Normalize(despawnCoords - spawnCoords);
      List<IMyCubeGrid> tempSpawningList = new List<IMyCubeGrid>();

      MyAPIGateway.PrefabManager.SpawnPrefab(
          resultList: tempSpawningList,
          prefabName: prefab.SubtypeId,
          position: spawnCoords,
          forward: spawnFacing,
          up: new Vector3D(0, 1, 0),
          spawningOptions: SpawningOptions.SetNeutralOwner | SpawningOptions.RotateFirstCockpitTowardsDirection | SpawningOptions.SpawnRandomCargo,
          beaconName: prefab.BeaconText,
          ownerId: m_empireData.m_faction.FounderId,
          updateSync: false,
          callback: () => SpawnerCallback(encounterType, prefab, spawnCoords, m_empireData.m_faction));
    }

    public void SpawnerCallback(EncounterType encounterType, MySpawnGroupDefinition.SpawnGroupPrefab prefab, Vector3D spawnCoord, IMyFaction ownerFaction) {
      Util.Log("Searching for spawned Prefab...");
      IMyCubeGrid spawnedShip = null;
      double minDist = double.MaxValue;
      BoundingSphereD searchSphere = new BoundingSphereD(spawnCoord, SPAWNER_SEARCH_RADIUS);
      List<IMyEntity> entityList = MyAPIGateway.Entities.GetEntitiesInSphere(ref searchSphere);
      foreach (IMyEntity entity in entityList) {
        IMyCubeGrid targetGrid = entity as IMyCubeGrid;
        if (targetGrid == null) {
          continue;
        }
        //if (targetGrid.CustomName != prefab.SubtypeId) {
        //continue;
        //}
        if ((targetGrid.GetPosition() - spawnCoord).LengthSquared() < minDist) {
          spawnedShip = targetGrid;
        }
      }
      if (spawnedShip == null) {
        Util.Error("Error: Could not find ship " + prefab.SubtypeId + " after spawning.");
        return;
      }
      spawnedShip.ChangeGridOwnership(m_empireData.m_faction.FounderId, MyOwnershipShareModeEnum.None);
      IMyGridTerminalSystem gts = MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(spawnedShip);
      List<IMyRemoteControl> blocks = new List<IMyRemoteControl>();
      gts.GetBlocksOfType(blocks);
      IMyRemoteControl firstRemote = blocks.Find(b => b.IsFunctional);
      LaunchDrone(encounterType, firstRemote, spawnedShip);
    }

    void LaunchDrone(EncounterType encounterType, IMyRemoteControl remote, IMyEntity entity) {
      if (remote != null && encounterType == EncounterType.TransientCargoship) {
        remote.ClearWaypoints();
        remote.FlightMode = Sandbox.ModAPI.Ingame.FlightMode.OneWay;
        remote.AddWaypoint(new Vector3D(0, 0, 0), "DespawnTarget");
        remote.SetAutoPilotEnabled(true);
        remote.SpeedLimit = CARGOSHIP_SPEED;
      }

      SpawnedShip ship = new SpawnedShip();
      ship.entityId = entity.EntityId;
      m_empireData.encounters.Add(ship);
      //EventManager.AddEvent(
      //GlobalData.world.currentTick + Tick.Seconds(60),
      //() => DespawnDrone(entity.EntityId));
      Util.Log("Drone Prepped!");
    }

    public void DespawnDrone(long entityId) {
      IMyEntity entity = MyAPIGateway.Entities.GetEntityById(entityId);
      if (entity == null) {
        return;
      }
      BlockManagement.DespawnShip((IMyCubeGrid)entity);
    }

    /**
     * Read the GlobalEvents.sbc data pertaining to the spawn rate for the ships.
     * The "first" time field is ignored, and only the min/max are used.
     */
    private void ReadSpawnConfig() {
      MyGlobalEventDefinition spawnShipGlobalEvent;
      MyDefinitionManager.Static.TryGetDefinition(
          new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase),
              "FSSpawnerTick[" + m_empireData.m_faction.Tag + "]"),
          out spawnShipGlobalEvent);
      if (spawnShipGlobalEvent == null) {
        MyDefinitionManager.Static.TryGetDefinition(
            new MyDefinitionId(typeof(MyObjectBuilder_GlobalEventBase), "FSSpawnerTick"),
            out spawnShipGlobalEvent);
      }
      if (spawnShipGlobalEvent == null) {
        return;
      }
      TimeSpan eventFirst = (TimeSpan)spawnShipGlobalEvent.FirstActivationTime;
      TimeSpan eventMin = (TimeSpan)spawnShipGlobalEvent.MinActivationTime;
      TimeSpan eventMax = (TimeSpan)spawnShipGlobalEvent.MaxActivationTime;
      m_spawnTickIntervalFirst = (int)Tick.Seconds((int)eventFirst.TotalSeconds);
      m_spawnTickIntervalMin = (int)Tick.Seconds((int)eventMin.TotalSeconds);
      m_spawnTickIntervalMax = (int)Tick.Seconds((int)eventMax.TotalSeconds);
    }

    /**
     * Init the spawn group list to contain all the spawner groups which contain the tag
     * `(FACTION)` in their name. Parse out anything between the first `[]` looking for the 
     * required tag `TYPE:` and optional tag `COST:`
     */
    private void GetAllSpawnGroups() {
      var allSpawnGroups = MyDefinitionManager.Static.GetSpawnGroupDefinitions();
      string tag = "(" + m_empireData.m_faction.Tag + ")";
      foreach (var spawnGroup in allSpawnGroups) {
        if (!spawnGroup.Id.SubtypeName.Contains(tag)) {
          continue;
        }

        int dataStart = spawnGroup.Id.SubtypeName.IndexOf('[');
        int dataEnd = spawnGroup.Id.SubtypeName.IndexOf(']');
        if (dataStart == -1 || dataEnd == -1) {
          Util.Warning("Skipping spawn group, due to bad data tag: " + spawnGroup.Id.SubtypeName);
          continue;
        }
        string[] dataTags = spawnGroup.Id.SubtypeName.Substring(dataStart + 1, Math.Max((dataEnd - 1) - (dataStart + 1), 0)).ToUpper().Split(',');
        int cost = 0;
        SpawnTag spawnTag = null;
        foreach (string data in dataTags) {
          try {
            if (data.StartsWith(DATA_TAG_TYPE)) {
              string tmp = data.Substring(DATA_TAG_TYPE.Length + 1);
              spawnTag = SPAWNER_TAGS.Find(t => tmp.StartsWith(t.m_tagString));
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

        if (spawnTag == null) {
          Util.Warning("Skipping spawn group, due to missing data tags: " + spawnGroup.Id.SubtypeName);
          continue;
        }
        m_spawnGroups.Add(new GroupInfo(cost, spawnTag.m_spawnClass, spawnTag.m_spawnType, spawnGroup));
      }
      Util.Log("Found " + m_spawnGroups.Count() + " " + m_empireData.m_faction.Tag + " SpawnGroups.");
    }

    /**
     * Runs through all the ships spawned for the empire, and re-registers their despawn timer.
     * This should only be run at load time.
     */
    private void ReRegisterDespawns() {
      m_empireData.civilianFleet.RemoveAll(
          s => MyAPIGateway.Entities.GetEntityById(s.entityId) == null);
      m_empireData.militaryFleet.RemoveAll(
          s => MyAPIGateway.Entities.GetEntityById(s.entityId) == null);
      // m_empireData.encounters.RemoveAll(
      //    s => MyAPIGateway.Entities.GetEntityById(s.entityId) == null);

      foreach (SpawnedShip shipInfo in m_empireData.civilianFleet) {
        EventManager.AddEvent(shipInfo.despawnTick, () => DespawnDrone(shipInfo.entityId));
      }
      foreach (SpawnedShip shipInfo in m_empireData.militaryFleet) {
        EventManager.AddEvent(shipInfo.despawnTick, () => DespawnDrone(shipInfo.entityId));
      }
    }

    private void RegisterCargoShip() {
      long targetTick = Math.Max(
          m_spawnTickIntervalFirst,
          GlobalData.world.currentTick + Util.rand.Next(m_spawnTickIntervalMin, m_spawnTickIntervalMax));
      EventManager.AddEvent(targetTick, SpawnCargoShip);
    }

    private void SpawnCargoShip() {
      long targetTick = GlobalData.world.currentTick
          + Util.rand.Next(m_spawnTickIntervalMin, m_spawnTickIntervalMax);
      EventManager.AddEvent(targetTick, SpawnCargoShip);

      if (m_empireData.civilianFleet.Count >= 2) {
        return;
      }
      GroupInfo group = GetRandomSpawnGroup(SpawnerClass.CIVILIAN, m_empireData.credits);
      if (group == null) {
        return;
      }
      IMyPlayer randomPlayer = GetRandomPlayer();
      if (randomPlayer == null) {
        return;
      }
      Vector3D randLocation = MathExtender.RandomPointOnSphere(
          randomPlayer.GetPosition(),
          CARGOSHIP_SPAWN_DIST);
      SpawnForGroup(group, EncounterType.TransientCargoship, randLocation);
    }
  }

}  // namespace FSTC