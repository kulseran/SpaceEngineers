using Sandbox.ModAPI;
using System.Collections.Generic;
using VRageMath;
using static FSTC.FSTCData;

namespace FSTC {

  public class EmpireManager {

    private static readonly Vector3D MAX_STRUCTURE_DISPLACEMENT =
        new Vector3D(250.0, 250.0, 250.0);
    private static readonly long STANDINGS_DEBOUCE_SEC = 1;

    private EmpireData m_data;
    private SpawnManager m_shipManager;
    private long m_nextStandingsChange = 0;

    public EmpireManager(EmpireData empire) {
      m_data = empire;
      m_data.m_faction = MyAPIGateway.Session.Factions.TryGetFactionByTag(m_data.empireTag);
      if (m_data.m_faction == null) {
        Util.Error("Faction (" + m_data.empireTag + ") not found. Empire will stagnate.");
        m_data = null;
        return;
      }
      MyAPIGateway.Session.Factions.ChangeAutoAccept(
          m_data.m_faction.FactionId,
          m_data.m_faction.FounderId,
          false,
          false);

      m_shipManager = new SpawnManager(m_data);
    }

    public EmpireData GetData() {
      return m_data;
    }

    public void Initialize() {
      if (m_data == null) {
        return;
      }
      InitSectorOwnership();
      if (m_data.encounters.Count == 0) {
        SpawnHQ();
      }
    }

    private void InitSectorOwnership() {
      List<SectorId> capturedSectors = new List<SectorId>();
      foreach (SectorId sector in m_data.ownedSectors) {
        if (!SectorManager.OwnFreeSector(sector, m_data)) {
          capturedSectors.Add(sector);
        }
      }
      m_data.ownedSectors.RemoveAll(s => capturedSectors.Find(t => t == s) != null);
      if (m_data.ownedSectors.Count == 0) {
        Util.Error("Empire owns no sectors: " + m_data.empireTag);
      }
    }

    private void SpawnHQ() {
      if (m_data.ownedSectors.Count == 0) {
        Util.Error("Empire owns no sectors: " + m_data.empireTag);
        return;
      }
      SpawnManager.GroupInfo info = m_shipManager.GetRandomSpawnGroup(
          SpawnManager.SpawnerType.STATIC_STRUCTURE_HQ,
          999999999);
      Vector3D teritoryCenter = SectorManager.CenterFromSector(
          m_data.ownedSectors[Util.rand.Next(m_data.ownedSectors.Count)]);
      BoundingBoxD hqLocation = new BoundingBoxD(
          teritoryCenter - MAX_STRUCTURE_DISPLACEMENT,
          teritoryCenter + MAX_STRUCTURE_DISPLACEMENT);
      m_shipManager.SpawnForRegion(info, SpawnManager.EncounterType.Static, hqLocation);
    }

    public void TakeStandingsHit(EmpireData aggressorEmpire) {
      if (m_nextStandingsChange > GlobalData.world.currentTick) {
        return;
      }
      m_nextStandingsChange = GlobalData.world.currentTick + Tick.Seconds(STANDINGS_DEBOUCE_SEC);

      EmpireData.EmpireStanding standings = Diplomacy.FindStandings(m_data, aggressorEmpire);
      if (standings == null) {
        return;
      }
      standings.reputation--;
      if (standings.atWar) {
        return;
      }
      Diplomacy.CallPolice(m_data, aggressorEmpire);
      if (standings.reputation < 0) {
        Diplomacy.DeclareWar(m_data, aggressorEmpire);
      }
    }

  };

}