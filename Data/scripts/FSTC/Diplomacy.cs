using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;

namespace FSTC {
  public static class Diplomacy {
    public static readonly List<string> KNOWN_FACTION_TAGS = new List<string>() {
          "SPRT",
          "FTRA",
          "IEFA",
          "IEEF",
          "XNOS",
          "SHVN"
        };

    public static readonly List<string> LAWFUL_FACTION_TAGS = new List<string>() {
          "FTRA",
          "IEFA",
          "IEEF",
          "XNOS",
        };

    public static readonly List<string> HOSTILE_FACTION_TAGS = new List<string>() {
          "SPRT",
          "SHVN"
        };

    public static readonly string POLICE_FACTION_TAG = "IEFA";

    public static IMyFaction Police { get; private set; }
    public static List<IMyFaction> LawfulFactions { get; private set; }
    public static List<IMyFaction> HostileFactions { get; private set; }

    public static void Initialize() {
      Police = MyAPIGateway.Session.Factions.TryGetFactionByTag(POLICE_FACTION_TAG);
      LawfulFactions = MyAPIGateway.Session.Factions.Factions.Values.Where(
          x => LAWFUL_FACTION_TAGS.Contains(x.Tag)).ToList();
      HostileFactions = MyAPIGateway.Session.Factions.Factions.Values.Where(
          x => HOSTILE_FACTION_TAGS.Contains(x.Tag)).ToList();
    }
  }
}
