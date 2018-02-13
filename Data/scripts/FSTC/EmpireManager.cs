using VRage.Game.ModAPI;

namespace FSTC {

  public class EmpireManager {

    private FSTCData.EmpireData m_data;
    private IMyFaction m_myFaction = null;
    private SpawnManager m_shipManager;

    public EmpireManager(FSTCData.EmpireData empire) {
      m_data = empire;
      m_myFaction = MyAPIGateway.Session.Factions.TryGetFactionByTag(m_data.empireTag);
      if (m_myFaction == null) {
        Util.Error("Faction [" + m_data.empireTag + "] not found. Empire will stagnate.");
      }
      m_shipManager = new SpawnManager(m_myFaction, m_data.fleet);
    }

    public FSTCData.EmpireData GetSave() {
      return m_data;
    }

    public void Initialize() {
      if (m_data.presence.Count == 0) {
        SpawnHQ();
      }
    }


    private void SpawnHQ() {

    }
  };

}