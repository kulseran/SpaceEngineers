using Sandbox.ModAPI;
using VRage.Game.ModAPI;
using static FSTC.FSTCData;

namespace FSTC {

  public class CargoBot : BotBase {

    private static readonly long DESPAWN_RETRY_TICKS = Tick.Minutes(1);
    private static readonly long CALL_HELP_TICKS = Tick.Seconds(10);
    private static readonly float CARGOSHIP_BEACON_RADIUS = 15000.0f;
    private static readonly float CARGOSHIP_ANTENNA_RADIUS = 5000.0f;

    public CargoBot(SpawnedShip spawnedShip, IMyRemoteControl remote) : base(spawnedShip, remote) {
      EventManager.AddEvent(m_spawnedShip.despawnTick, UpdateDespawn);
      EventManager.AddEvent(GlobalData.world.currentTick + CALL_HELP_TICKS, UpdateCallHelp);
      SetupCallsign();
    }

    private void SetupCallsign() {
      if (m_mainBeacon == null) {
        return;
      }

      m_mainBeacon.Enabled = true;
      m_mainBeacon.Radius = CARGOSHIP_BEACON_RADIUS;
      m_mainBeacon.CustomName = "Freighter C" + Util.rand.Next() % 999;
    }

    private void UpdateCallHelp() {
      if (m_mainAntenna == null) {
        return;
      }
      // Check for enemies!
      bool foundEnemy = false;

      // If nothing, turn off the antenna!
      if (foundEnemy) {
        m_mainAntenna.EnableBroadcasting = true;
        m_mainAntenna.Radius = CARGOSHIP_ANTENNA_RADIUS;
        m_mainAntenna.CustomName = PirateAntennaManager.AntennaNames.GENERAL_DISTRESS;
      } else {
        m_mainAntenna.EnableBroadcasting = false;
      }

      EventManager.AddEvent(GlobalData.world.currentTick + CALL_HELP_TICKS, UpdateCallHelp);
    }

    private void UpdateDespawn() {
      if (!Active) {
        // Something disabled us, we'll never despawn now, and will hopefully be cleaned up.
        return;
      }

      EventManager.AddEvent(GlobalData.world.currentTick + DESPAWN_RETRY_TICKS, UpdateDespawn);
    }

  };

} // namespace FSTC