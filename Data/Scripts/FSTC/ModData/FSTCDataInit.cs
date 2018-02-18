using VRageMath;
using static FSTC.FSTCData;

namespace FSTC {

  public static class FstcInitialData {
    public static FSTCData Get() {
      FSTCData ret = new FSTCData();
      // True Hostiles
      ret.empires.Add(EmpireSPRT());
      ret.empires.Add(EmpireSHVN());

      // Hostiles
      ret.empires.Add(EmpireSYND());

      // Neutrals
      ret.empires.Add(EmpireEIEF());
      ret.empires.Add(EmpireIFTA());
      ret.empires.Add(EmpireXGTC());

      // Police
      ret.empires.Add(EmpireUEFA());

      return ret;
    }

    private static FSTCData.EmpireData EmpireSPRT() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData {
        empireTag = "SPRT",
        empireType = (int)Diplomacy.EmpireType.TRUE_HOSTILE,
        ownedSectors = { new SectorId(0,1,0) }
      };
      return ret;
    }

    private static FSTCData.EmpireData EmpireSHVN() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData {
        empireTag = "SHVN",
        empireType = (int)Diplomacy.EmpireType.TRUE_HOSTILE,
        ownedSectors = { new SectorId(0,-1,0) }
      };
      return ret;
    }

    private static FSTCData.EmpireData EmpireSYND() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData {
        empireTag = "SYND",
        empireType = (int)Diplomacy.EmpireType.HOSTILE,
        ownedSectors = { new SectorId(0,0,1) }
      };
      return ret;
    }

    private static FSTCData.EmpireData EmpireEIEF() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData {
        empireTag = "EIEF",
        empireType = (int)Diplomacy.EmpireType.NEUTRAL,
        ownedSectors = { new SectorId(0,0,-1) }
      };
      return ret;
    }

    private static FSTCData.EmpireData EmpireIFTA() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData {
        empireTag = "IFTA",
        empireType = (int)Diplomacy.EmpireType.NEUTRAL,
        ownedSectors = { new SectorId(-1,0,0) }
      };
      return ret;
    }

    private static FSTCData.EmpireData EmpireXGTC() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData {
        empireTag = "XGTC",
        empireType = (int)Diplomacy.EmpireType.NEUTRAL,
        ownedSectors = { new SectorId(1,0,0) }
      };
      return ret;
    }

    private static FSTCData.EmpireData EmpireUEFA() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData {
        empireTag = "UEFA",
        empireType = (int)Diplomacy.EmpireType.POLICE,
        ownedSectors = { new SectorId(1,0,1) }
      };
      return ret;
    }
  };

}