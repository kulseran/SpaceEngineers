using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using VRage.Game.ModAPI;
using VRageMath;

namespace FSTC {
  [XmlType("FSTCData")]
  public class FSTCData {
    [XmlType("FSTCSpawnedShip")]
    public class SpawnedShip {
      public string prefabId = null;
      public long entityId = 0;
      public long antennaEntityId = 0;
      public long despawnTick = 0;
      public int encounterType = 0;
    }

    [XmlType("FSTCSectorId")]
    public class SectorId {
      public long x;
      public long y;
      public long z;

      public SectorId(long x, long y, long z) {
        this.x = x;
        this.y = y;
        this.z = z;
      }
    };

    [XmlType("FSTCEmpireData")]
    public class EmpireData {
      
      [XmlType("FSTCEmpireStanding")]
      public class EmpireStanding {
         public string empireTag = "";
         public int reputation = 0;
         public bool atWar = false;
         public long inStateTill = 0;
      }

      public string empireTag = "";
      public int empireType = 0;
      public int size = 0;
      public int credits = 0;
      public int nextTick = 0;
      public List<EmpireStanding> standings = new List<EmpireStanding>();
      public List<SpawnedShip> militaryFleet = new List<SpawnedShip>();
      public List<SpawnedShip> civilianFleet = new List<SpawnedShip>();
      public List<SpawnedShip> encounters = new List<SpawnedShip>();
      public List<SectorId> ownedSectors = new List<SectorId>();

      [XmlIgnore]
      public IMyFaction m_faction;
    }

    public long currentTick = 0;

    public List<EmpireData> empires = new List<EmpireData>();
  }
}