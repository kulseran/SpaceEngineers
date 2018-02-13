using System.Collections.Generic;
using System.IO;
using Sandbox.ModAPI;
using VRage.Game.Components;

namespace FSTC {

  [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 2000)]
  public class FSTotalConversion : MySessionComponentBase {

    private readonly string SAVEFILE_NAME = "FSTC.xml";

    private List<EmpireManager> m_empires = new List<EmpireManager>();
    FSTCData m_data = new FSTCData();

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
        Util.Log("Resuming save at tick: " + m_data.currentTick);
      } else {
        Util.Warning("No save data for FSTC mod initializing fresh campaign.");
        m_data = FstcInitialData.Get();
      }

      Util.Initialize(m_data.currentTick);
    }

    /**
     *
     */
    private bool LoadSaveFile() {
      if (MyAPIGateway.Utilities.FileExistsInWorldStorage(SAVEFILE_NAME, typeof(FSTCData))) {
        try {
          TextReader reader = MyAPIGateway.Utilities.ReadFileInWorldStorage(SAVEFILE_NAME, typeof(FSTCData));
          m_data = MyAPIGateway.Utilities.SerializeFromXML<FSTCData>(reader.ReadToEnd());
          reader.Close();
          return true;
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
      FSTCData data = new FSTCData();
      data.currentTick = Util.currentTick;
      foreach (EmpireManager empire in m_empires) {
        data.empires.Add(empire.GetSave());
      }

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
      foreach (FSTCData.EmpireData empireData in m_data.empires) {
        m_empires.Add(new EmpireManager(empireData));
      }
      Diplomacy.Initialize();
      foreach (EmpireManager empire in m_empires) {
        empire.Initialize();
      }

      Util.Log("Welcome to FSTC.");
    }
  }

}  // namespace FSTC
