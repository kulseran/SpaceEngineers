using Sandbox.ModAPI;
using VRageMath;
using static FSTC.FSTCData;

namespace FSTC {

  public class EmpireManager {

    private static readonly Vector3D MAX_STRUCTURE_DISPLACEMENT = new Vector3D(250.0, 250.0, 250.0);
    private static readonly Vector3D MAX_STRUCTURE_OWNERSHIP = new Vector3D(50000.0, 50000.0, 50000.0);

    private EmpireData m_data;
    private SpawnManager m_shipManager;

    public EmpireManager(EmpireData empire) {
      m_data = empire;
      m_data.m_faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(m_data.empireTag);
      if (m_data.m_faction == null) {
        Util.Error("Faction (" + m_data.empireTag + ") not found. Empire will stagnate.");
        m_data = null;
        return;
      }
      MyAPIGateway.Session.Factions.ChangeAutoAccept(m_data.m_faction.FactionId, m_data.m_faction.FounderId, false, false);

      m_shipManager = new SpawnManager(m_data);
    }

    public void Initialize() {
      if (m_data == null) {
        return;
      }
      if (m_data.encounters.Count == 0) {
        SpawnHQ();
      }
    }

    private void SpawnHQ() {
      SpawnManager.GroupInfo info;
      m_shipManager.GetRandomSpawnGroup(SpawnManager.SpawnerType.STATIC_STRUCTURE_HQ, 999999999, out info);
      Vector3D teritoryCenter = m_data.bounds.Min + (m_data.bounds.Max - m_data.bounds.Min) * 0.5;
      BoundingBoxD hqLocation = new BoundingBoxD(teritoryCenter - MAX_STRUCTURE_DISPLACEMENT, teritoryCenter + MAX_STRUCTURE_DISPLACEMENT);
      m_shipManager.SpawnForRegion(info, SpawnManager.EncounterType.Static, hqLocation);
    }

    private BoundingBoxD GetDefaultOwnershipBounds(Vector3D center) {
       return new BoundingBoxD(center - MAX_STRUCTURE_OWNERSHIP, center + MAX_STRUCTURE_OWNERSHIP);
    }
  };

}