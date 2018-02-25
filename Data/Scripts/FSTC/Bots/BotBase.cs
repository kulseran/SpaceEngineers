
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace FSTC {

  public class BotBase {

    IMyCubeGrid m_ownedGrid;
    IMyRemoteControl m_remote;

    public bool Active => m_remote != null;

    public BotBase(IMyRemoteControl remote) {
      m_ownedGrid = remote.CubeGrid;
      m_remote = remote;
      SetupRemote();
    }

    private void SetupRemote() {
      m_remote.OwnershipChanged += RemoteOwnershipChanged;
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