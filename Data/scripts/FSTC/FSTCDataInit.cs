
using VRageMath;

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
        bounds = new BoundingBoxD(new Vector3D(-9000.0, -10000.0, -10000.0), new Vector3D(10000, 10000, 10000))
      };
      return ret;
    }

    private static FSTCData.EmpireData EmpireSHVN() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData {
        empireTag = "SHVN",
        empireType = (int)Diplomacy.EmpireType.TRUE_HOSTILE,
        bounds = new BoundingBoxD(new Vector3D(-10000.0, -9000.0, -10000.0), new Vector3D(10000, 10000, 10000))
      };
      return ret;
    }

    private static FSTCData.EmpireData EmpireSYND() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData {
        empireTag = "SYND",
        empireType = (int)Diplomacy.EmpireType.HOSTILE,
        bounds = new BoundingBoxD(new Vector3D(-10000.0, -10000.0, -10000.0), new Vector3D(9000, 10000, 10000))
      };
      return ret;
    }

    private static FSTCData.EmpireData EmpireEIEF() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData {
        empireTag = "EIEF",
        empireType = (int)Diplomacy.EmpireType.NEUTRAL,
        bounds = new BoundingBoxD(new Vector3D(-10000.0, -10000.0, -10000.0), new Vector3D(10000, 9000, 10000))
      };
      return ret;
    }

    private static FSTCData.EmpireData EmpireIFTA() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData {
        empireTag = "IFTA",
        empireType = (int)Diplomacy.EmpireType.NEUTRAL,
        bounds = new BoundingBoxD(new Vector3D(-10000.0, -10000.0, -10000.0), new Vector3D(10000, 9500, 9500))
      };
      return ret;
    }

    private static FSTCData.EmpireData EmpireXGTC() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData {
        empireTag = "XGTC",
        empireType = (int)Diplomacy.EmpireType.NEUTRAL,
        bounds = new BoundingBoxD(new Vector3D(-10000.0, -9000.0, -10000.0), new Vector3D(10000, 10000, 9000))
      };
      return ret;
    }

    private static FSTCData.EmpireData EmpireUEFA() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData {
        empireTag = "UEFA",
        empireType = (int)Diplomacy.EmpireType.POLICE,
        bounds = new BoundingBoxD(new Vector3D(-10000.0, -10000.0, -9500.0), new Vector3D(10000, 9500, 10000))
      };
      return ret;
    }
  };

}