using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using VRageMath;

namespace FSTC {
  [XmlType("FSTCData")]
  public class FSTCData {
    [XmlType("FSTCSpawnedShip")]
    public class SpawnedShip {
      public long entityId = 0;
      public long antennaEntityId = 0;
      public long despawnTick = 0;
      public bool isEncounter = false;
    }

    [XmlType("FSTCSpawnedEncounter")]
    public class EncounterMarker {
      public BoundingBoxD bounds;
      public int encounterId;
    }
  
    [XmlType("FSTCEmpireData")]
    public class EmpireData {
      
      [XmlType("FSTCEmpireStanding")]
      public class EmpireStanding {
         public string empireTag = "";
         public int standing = 0;
      }

      public string empireTag = "";
      public int empireType = 0;
      public int size = 0;
      public int credits = 0;
      public BoundingBoxD bounds;
      public List<EmpireStanding> standings = new List<EmpireStanding>();

      public List<SpawnedShip> fleet = new List<SpawnedShip>();
      public List<EncounterMarker> presence = new List<EncounterMarker>();
    }

    public long currentTick = 0;

    public List<EmpireData> empires = new List<EmpireData>();
  }
}