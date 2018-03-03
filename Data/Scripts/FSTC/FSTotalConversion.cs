using System;
using System.Collections.Generic;
using System.IO;
using Sandbox.Definitions;
using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using VRage.Game.Components;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRageMath;
using static FSTC.FSTCData;

namespace FSTC {

  [MySessionComponentDescriptor(MyUpdateOrder.BeforeSimulation, 2000)]
  public class FSTotalConversion : MySessionComponentBase {

    private readonly string SAVEFILE_NAME = "FSTC.xml";

    private List<EmpireManager> m_empireManagers = new List<EmpireManager>();

    public EmpireData EmpireData { get; private set; }

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
      MyDefinitionManager.Static.EnvironmentDefinition.SmallShipMaxSpeed = 200f;
      MyDefinitionManager.Static.EnvironmentDefinition.LargeShipMaxSpeed = 150f;

      if (!MyAPIGateway.Multiplayer.IsServer) {
        return;
      }
      
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
      if (!MyAPIGateway.Multiplayer.IsServer) {
        return;
      }

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

    public override void Draw() {
    }

    private void GenericDamageHandler(object target, ref MyDamageInformation info) {
      GenericDamageHandler(target, info);
    }

    public void GenericDamageHandler(object target, MyDamageInformation info) {
      if (target == null || !(target is IMySlimBlock)) {
        return;
      }
      IMySlimBlock block = target as IMySlimBlock;
      IMyCubeGrid grid = block.CubeGrid;
      long gridId = grid.GetTopMostParent().EntityId;
      IMyFaction damageeFaction = grid.GetOwningFaction();

      IMyPlayer damagerPlayer;
      IMyFaction damagerFaction;
      IMyCubeGrid damagerShip;
      if (damageeFaction != null
          && info.GetSource(out damagerPlayer, out damagerFaction, out damagerShip)) {
        if (damagerFaction == null) {
          return;
        }
        EmpireData damagerEmpire = damagerFaction.GetEmpire();
        foreach (EmpireManager empire in m_empireManagers) {
          if (empire.GetData().empireTag == damageeFaction.Tag) {
            empire.TakeStandingsHit(damagerEmpire);
            break;
          }
        }
      }
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

      MyAPIGateway.Session.DamageSystem.RegisterBeforeDamageHandler(0, GenericDamageHandler);
      MyAPIGateway.Session.DamageSystem.RegisterDestroyHandler(0, GenericDamageHandler);

      Util.Notify("Welcome to FSTC.");
    }

  }

}  // namespace FSTC
