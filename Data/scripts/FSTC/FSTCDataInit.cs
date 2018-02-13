
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
      FSTCData.EmpireData ret = new FSTCData.EmpireData();
      ret.empireTag = "SPRT";
      ret.empireType = (int)Diplomacy.EmpireType.TRUE_HOSTILE;
      ret.credits = 0;
      ret.size = 0;
      ret.bounds = new BoundingBoxD(new Vector3D(-10000.0, -10000.0, -10000.0), new Vector3D(10000, 10000, 10000));
      return ret;
    }

    private static FSTCData.EmpireData EmpireSHVN() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData();
      ret.empireTag = "SHVN";
      ret.empireType = (int)Diplomacy.EmpireType.TRUE_HOSTILE;
      ret.credits = 1000;
      ret.size = 0;
      ret.bounds = new BoundingBoxD(new Vector3D(-10000.0, -10000.0, -10000.0), new Vector3D(10000, 10000, 10000));
      return ret;
    }

    private static FSTCData.EmpireData EmpireSYND() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData();
      ret.empireTag = "SYND";
      ret.empireType = (int)Diplomacy.EmpireType.HOSTILE;
      ret.credits = 1000;
      ret.size = 0;
      ret.bounds = new BoundingBoxD(new Vector3D(-10000.0, -10000.0, -10000.0), new Vector3D(10000, 10000, 10000));
      return ret;
    }

    private static FSTCData.EmpireData EmpireEIEF() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData();
      ret.empireTag = "EIEF";
      ret.empireType = (int)Diplomacy.EmpireType.NEUTRAL;
      ret.credits = 1000;
      ret.size = 0;
      ret.bounds = new BoundingBoxD(new Vector3D(-10000.0, -10000.0, -10000.0), new Vector3D(10000, 10000, 10000));
      return ret;
    }

    private static FSTCData.EmpireData EmpireIFTA() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData();
      ret.empireTag = "IFTA";
      ret.empireType = (int)Diplomacy.EmpireType.NEUTRAL;
      ret.credits = 1000;
      ret.size = 0;
      ret.bounds = new BoundingBoxD(new Vector3D(-10000.0, -10000.0, -10000.0), new Vector3D(10000, 10000, 10000));
      return ret;
    }

    private static FSTCData.EmpireData EmpireXGTC() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData();
      ret.empireTag = "XGTC";
      ret.empireType = (int)Diplomacy.EmpireType.NEUTRAL;
      ret.credits = 1000;
      ret.size = 0;
      ret.bounds = new BoundingBoxD(new Vector3D(-10000.0, -10000.0, -10000.0), new Vector3D(10000, 10000, 10000));
      return ret;
    }

    private static FSTCData.EmpireData EmpireUEFA() {
      FSTCData.EmpireData ret = new FSTCData.EmpireData();
      ret.empireTag = "UEFA";
      ret.empireType = (int)Diplomacy.EmpireType.POLICE;
      ret.credits = 1000;
      ret.size = 0;
      ret.bounds = new BoundingBoxD(new Vector3D(-10000.0, -10000.0, -10000.0), new Vector3D(10000, 10000, 10000));
      return ret;
    }
  };

}