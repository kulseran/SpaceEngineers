
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using static FSTC.FSTCData;

namespace FSTC {

  public class BotBase {

    protected IMyCubeGrid m_ownedGrid;
    protected IMyRemoteControl m_remote;
    protected IMyRadioAntenna m_mainAntenna;
    protected IMyBeacon m_mainBeacon;
    protected SpawnedShip m_spawnedShip;
    protected SpawnManager m_spawnManager;

    public bool Active => m_remote != null;

    public BotBase(SpawnManager manager, SpawnedShip ship, IMyRemoteControl remote) {
      m_spawnManager = manager;
      m_spawnedShip = ship;
      m_ownedGrid = remote.CubeGrid;
      m_remote = remote;

      SetupRemote();
      SetupComms();
    }

    protected IMyEntity GetClosestEnemy(float maxSearchRadius) {
      if (!Active) {
        return null;
      }
      IMyFaction selfFaction = m_remote.CubeGrid.GetOwningFaction();

      BoundingSphereD bounds = new BoundingSphereD(m_remote.GetPosition(), maxSearchRadius);
      List<IMyEntity> possibleEnemies = MyAPIGateway.Entities.GetEntitiesInSphere(ref bounds);
      double minDist = double.MaxValue;
      IMyEntity closest = null;
      foreach (IMyEntity entity in possibleEnemies) {
        if(entity == m_remote.CubeGrid) {
          continue;
        }
        Vector3D position = entity.GetPosition();
        IMyFaction objectFaction = null;
        if (entity is IMyCubeGrid) {
          IMyCubeGrid grid = entity as IMyCubeGrid;
          objectFaction = grid.GetOwningFaction();
        } else if (entity is IMyPlayer) {
          IMyPlayer player = entity as IMyPlayer;
          objectFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(player.IdentityId);
        }

        if (objectFaction == null) {
          continue;
        }
        MyRelationsBetweenFactions relation = MyAPIGateway.Session.Factions.GetRelationBetweenFactions(selfFaction.FactionId, objectFaction.FactionId);
        if (relation.Equals(MyRelationsBetweenFactions.Enemies)) {
          double dist = m_remote.GetPosition().DistanceTo(position);
          if(dist < minDist) {
            minDist = dist;
            closest = entity;
          }
        }
      }
      return closest;
    }

    /**
     * Initialize the remote control setup.
     */
    private void SetupRemote() {
      m_remote.OwnershipChanged += RemoteOwnershipChanged;
    }

    /**
     * Locate the primary communication antenna and beacon. These may be used by the specific
     * bot implementation to indicate the status of the ship.
     */
    private void SetupComms() {
      IMyGridTerminalSystem terminal = m_ownedGrid.GetTerminalSystem();
      List<IMyRadioAntenna> antennas = terminal.GetBlocksOfType<IMyRadioAntenna>();
      if (antennas.Count > 0) {
        m_mainAntenna = antennas[0];
      }
      foreach(IMyRadioAntenna antenna in antennas) {
        antenna.EnableBroadcasting = false;
      }

      List<IMyBeacon> beacons = terminal.GetBlocksOfType<IMyBeacon>();
      if (beacons.Count > 0) {
        m_mainBeacon = beacons[0];
      }
      foreach(IMyBeacon beacon in beacons) {
        beacon.Enabled = false;
      }
    }

    /**
     * If the remote control is destroyed, then this bot is now dead.
     */
    private void RemoteOwnershipChanged(IMyTerminalBlock obj) {
      Util.Notify("Block Ownership Changed!");
      m_remote = null;
      UnregisterBot();
    }

    private void UnregisterBot() {
      BotManager.RemoveBot(this);
    }
  }


} // namespace FSTC