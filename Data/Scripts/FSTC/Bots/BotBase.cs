
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using static FSTC.FSTCData;

namespace FSTC {

  public class BotBase {

    protected IMyCubeGrid m_ownedGrid;
    protected IMyRemoteControl m_remote;
    protected IMyRadioAntenna m_mainAntenna;
    protected IMyBeacon m_mainBeacon;
    protected SpawnedShip m_spawnedShip;

    public bool Active => m_remote != null;

    public BotBase(SpawnedShip ship, IMyRemoteControl remote) {
      m_spawnedShip = ship;
      m_ownedGrid = remote.CubeGrid;
      m_remote = remote;
      SetupRemote();
      SetupComms();
    }

    private void SetupRemote() {
      m_remote.OwnershipChanged += RemoteOwnershipChanged;
    }

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