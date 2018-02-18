using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using static FSTC.FSTCData;

namespace FSTC {

  /**
   * Extensions to make working with players and factions easier.
   */
  public static class FactionExtender {

    /**
     * Add a by-id lookup to the Player list.
     */
    public static IMyPlayer GetPlayerByID(this IMyPlayerCollection playersIn, long playerID) {
      List<IMyPlayer> players = new List<IMyPlayer>();
      playersIn.GetPlayers(players, x => x.IdentityId == playerID);
      if (players.Count > 0) {
        return players[0];
      }
      return null;
    }

    public static EmpireData GetEmpire(this IMyFaction faction) {
      return GlobalData.world.empires.Find(e => e.empireTag.Equals(faction.Tag));
    }

  };
} // namespace FSTC