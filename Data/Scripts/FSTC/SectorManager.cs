using System;
using System.Collections.Generic;
using VRageMath;
using static FSTC.FSTCData;

namespace FSTC {

  public static class SectorManager {

    private static readonly double SECTOR_SIZE = 5000.0;
    private static readonly double SECTOR_HALF_SIZE = SECTOR_SIZE * 0.5;
    private static readonly Vector3D SECTOR_OFFSET =
        new Vector3D(SECTOR_HALF_SIZE, SECTOR_HALF_SIZE, SECTOR_HALF_SIZE);

    public class Sector {
      public EmpireData m_owner;

      public Sector(EmpireData owner) {
        m_owner = owner;
      }
    };

    private static Dictionary<SectorId, Sector> m_ocupiedSectors = new Dictionary<SectorId, Sector>();

    public static SectorId FindFreeSectorNear(SectorId origin, HashSet<SectorId> checkedSectors = null) {
      if (!m_ocupiedSectors.ContainsKey(origin)) {
        if (!(origin.x == 0 && origin.y == 0 && origin.z == 0)) {
          return origin;
        }
      }
      List<SectorId> checkSectors = new List<SectorId>();
      List<SectorId> freeSectors = new List<SectorId>();
      List<SectorId> occupiedSectors = new List<SectorId>();
      checkSectors.Add(new SectorId(origin.x + 1, origin.y + 1, origin.z + 1));
      checkSectors.Add(new SectorId(origin.x + 0, origin.y + 1, origin.z + 1));
      checkSectors.Add(new SectorId(origin.x - 1, origin.y + 1, origin.z + 1));
      checkSectors.Add(new SectorId(origin.x + 1, origin.y + 0, origin.z + 1));
      checkSectors.Add(new SectorId(origin.x + 0, origin.y + 0, origin.z + 1));
      checkSectors.Add(new SectorId(origin.x - 1, origin.y + 0, origin.z + 1));
      checkSectors.Add(new SectorId(origin.x + 1, origin.y - 1, origin.z + 1));
      checkSectors.Add(new SectorId(origin.x + 0, origin.y - 1, origin.z + 1));
      checkSectors.Add(new SectorId(origin.x - 1, origin.y - 1, origin.z + 1));

      checkSectors.Add(new SectorId(origin.x + 1, origin.y + 1, origin.z + 0));
      checkSectors.Add(new SectorId(origin.x + 0, origin.y + 1, origin.z + 0));
      checkSectors.Add(new SectorId(origin.x - 1, origin.y + 1, origin.z + 0));
      checkSectors.Add(new SectorId(origin.x + 1, origin.y + 0, origin.z + 0));

      checkSectors.Add(new SectorId(origin.x - 1, origin.y + 0, origin.z + 0));
      checkSectors.Add(new SectorId(origin.x + 1, origin.y - 1, origin.z + 0));
      checkSectors.Add(new SectorId(origin.x + 0, origin.y - 1, origin.z + 0));
      checkSectors.Add(new SectorId(origin.x - 1, origin.y - 1, origin.z + 0));

      checkSectors.Add(new SectorId(origin.x + 1, origin.y + 1, origin.z - 1));
      checkSectors.Add(new SectorId(origin.x + 0, origin.y + 1, origin.z - 1));
      checkSectors.Add(new SectorId(origin.x - 1, origin.y + 1, origin.z - 1));
      checkSectors.Add(new SectorId(origin.x + 1, origin.y + 0, origin.z - 1));
      checkSectors.Add(new SectorId(origin.x + 0, origin.y + 0, origin.z - 1));
      checkSectors.Add(new SectorId(origin.x - 1, origin.y + 0, origin.z - 1));
      checkSectors.Add(new SectorId(origin.x + 1, origin.y - 1, origin.z - 1));
      checkSectors.Add(new SectorId(origin.x + 0, origin.y - 1, origin.z - 1));
      checkSectors.Add(new SectorId(origin.x - 1, origin.y - 1, origin.z - 1));

      foreach (SectorId id in checkSectors) {
        if (m_ocupiedSectors.ContainsKey(id) && (checkedSectors == null || !checkedSectors.Contains(id))) {
          occupiedSectors.Add(id);
        } else {
          freeSectors.Add(id);
        }
      }

      if (freeSectors.Count > 0) {
        return freeSectors[Util.rand.Next(freeSectors.Count)];
      }

      if (checkedSectors == null) {
        checkedSectors = new HashSet<SectorId>();
      }
      checkedSectors.Add(origin);
      foreach (SectorId o in occupiedSectors) {
        checkedSectors.Add(o);
      }

      Util.Shuffle(occupiedSectors);
      foreach (SectorId id in occupiedSectors) {
        SectorId found = FindFreeSectorNear(id, checkedSectors);
        if (found != null) {
          return found;
        }
      }
      return null;
    }

    public static List<SectorId> FindSectorsByOwner(EmpireData empire) {
      List<SectorId> sectors = new List<SectorId>();

      return sectors;
    }

    public static Sector GetSector(SectorId id) {
      Sector sector = null;
      if (m_ocupiedSectors.TryGetValue(id, out sector)) {
        return sector;
      }
      return null;
    }

    public static bool OwnFreeSector(SectorId id, EmpireData capturingEmpire) {
      Sector sector = GetSector(id);
      if (sector == null) {
        m_ocupiedSectors.Add(id, new Sector(capturingEmpire));
        return true;
      }
      return false;
    }

    /**
     * Convert from a world position to the relevant sector.
     */
    public static SectorId SectorFromPosition(Vector3D pos) {
      Vector3D sectorFloat = (pos + SECTOR_OFFSET) / SECTOR_SIZE;
      return new SectorId(
          (long) Math.Floor(sectorFloat.X),
          (long) Math.Floor(sectorFloat.Y),
          (long) Math.Floor(sectorFloat.Z));
    }

    /**
     * Convert a sector to it's center
     */
    public static Vector3D CenterFromSector(SectorId pos) {
      Vector3D sectorFloat = new Vector3D(pos.x, pos.y, pos.z);
      sectorFloat *= SECTOR_SIZE;
      return sectorFloat;
    }
  }

}