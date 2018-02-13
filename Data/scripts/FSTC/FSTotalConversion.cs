using System.Collections.Generic;
using System.IO;
using Sandbox.ModAPI;
using VRage.Game.Components;

namespace FSTC {

  [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 2000)]
  public class FSTotalConversion : MySessionComponentBase {

    private readonly string SAVEFILE_NAME = "FSTC.xml";

    private List<SpawnManager> m_spawnManagers = new List<SpawnManager>();

    public override void BeforeStart() {
      InitializeMod();
    }

    public override void LoadData() {
      FSTCData data = new FSTCData();

      if (MyAPIGateway.Utilities.FileExistsInWorldStorage(SAVEFILE_NAME, typeof(FSTCData))) {
        try {
          TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(SAVEFILE_NAME, typeof(FSTCData));
          data = MyAPIGateway.Utilities.SerializeFromXML<FSTCData>(reader.ReadToEnd());
          reader.Close();
        } catch {
          Util.Log("Corrupt save data.");
        }
      } else {
        Util.Log("No save data for FSTC mod.");
      }

      Util.Log("Loading tick: " + data.currentTick);
      Util.Initialize(data.currentTick);
    }

    public override void SaveData() {
      FSTCData data = new FSTCData();
      data.currentTick = Util.currentTick;

      TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(SAVEFILE_NAME, typeof(FSTCData));
      writer.Write(MyAPIGateway.Utilities.SerializeToXML<FSTCData>(data));
      writer.Flush();
      writer.Close();
    }

    /**
     * Main entrypoint from Space Engineers
     */
    public override void UpdateBeforeSimulation() {
      if (!MyAPIGateway.Multiplayer.IsServer) {
        return;
      }
      Util.NextTick();
      EventManager.TriggerEvents(Util.currentTick);
    }

    /**
     * Initialize the mod
     */
    private void InitializeMod() {
      if (!MyAPIGateway.Multiplayer.IsServer) {
        return;
      }
      Diplomacy.Initialize();

      foreach (string faction in Diplomacy.KNOWN_FACTION_TAGS) {
        SpawnManager manager = new SpawnManager();
        manager.Initialize(faction);
        m_spawnManagers.Add(manager);

        Diplomacy.Initialize();
      }
      Util.Log("Hello World");
    }
  }

}  // namespace FSTC
