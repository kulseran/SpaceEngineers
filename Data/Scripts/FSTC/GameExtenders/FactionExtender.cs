using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.ModAPI;
using VRageMath;
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
    
    /**
     * Return the player closest to the target location.
     */
    public static IMyPlayer GetClosestPlayer(this IMyPlayerCollection playersIn, Vector3D target) {
      List<IMyPlayer> players = new List<IMyPlayer>();
      playersIn.GetPlayers(players);
      IMyPlayer closest = null;
      double dist = double.MaxValue;
      foreach(IMyPlayer player in players) {
        double curdist = player.GetPosition().DistanceTo(target);
        if (curdist < dist) {
          dist = curdist;
          closest = player;
        }
      }
      return closest;
    }

    public static EmpireData GetEmpire(this IMyFaction faction) {
      return GlobalData.world.empires.Find(e => e.empireTag.Equals(faction.Tag));
    }

  };
} // namespace FSTC