using System.Collections.Generic;
using System.IO;
using Sandbox.ModAPI;
using VRage.Game.Components;
using static FSTC.FSTCData;

namespace FSTC {

  [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 2000)]
  public class FSTotalConversion : MySessionComponentBase {

    private readonly string SAVEFILE_NAME = "FSTC.xml";

    private List<EmpireManager> m_empireManagers = new List<EmpireManager>();

    /**
     *
     */
    public override void BeforeStart() {
      InitializeMod();
    }

    /**
     *
     */
    public override void LoadData() {
      if (LoadSaveFile()) {
        Util.Log("Resuming save at tick: " + GlobalData.world.currentTick);
      } else {
        Util.Warning("No save data for FSTC mod initializing fresh campaign.");
      }
    }

    /**
     *
     */
    private bool LoadSaveFile() {
      if (MyAPIGateway.Utilities.FileExistsInWorldStorage(SAVEFILE_NAME, typeof(FSTCData))) {
        try {
          TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(SAVEFILE_NAME, typeof(FSTCData));
          FSTCData data = MyAPIGateway.Utilities.SerializeFromXML<FSTCData>(reader.ReadToEnd());
          reader.Close();
          if (data != null) {
            GlobalData.world = data;
            return true;
          }
        } catch {
          Util.Error("Corrupt save data.");
        }
      }
      return false;
    }

    /**
     *
     */
    public override void SaveData() {
      TextWriter writer = MyAPIGateway.Utilities.WriteFileInWorldStorage(SAVEFILE_NAME, typeof(FSTCData));
      writer.Write(MyAPIGateway.Utilities.SerializeToXML(GlobalData.world));
      writer.Flush();
      writer.Close();
    }

    /**
     * Main entrypoint from Space Engineers
     */
    public override void UpdateBeforeSimulation() {
      GlobalData.NextTick();
      EventManager.TriggerEvents(GlobalData.world.currentTick);
    }

    /**
     * Initialize the mod
     */
    private void InitializeMod() {
      foreach (EmpireData empire in GlobalData.world.empires) {
        m_empireManagers.Add(new EmpireManager(empire));
      }
      foreach (EmpireManager empire in m_empireManagers) {
        empire.Initialize();
      }
      Diplomacy.Initialize();

      Util.Notify("Welcome to FSTC.");
    }
  }

}  // namespace FSTC
