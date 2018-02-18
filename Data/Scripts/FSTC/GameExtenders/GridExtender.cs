using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace FSTC {

  /**
   * Helper functions for pulling ship information
   */
  public static class GridEndender {

    /**
     * Get the terminal helper for a given grid.
     */
    public static IMyGridTerminalSystem GetTerminalSystem(this IMyCubeGrid grid) {
      return MyAPIGateway.TerminalActionsHelper.GetTerminalSystemForGrid(grid);
    }

    /**
     * Get a block's builder
     */
    public static long GetBuiltBy(this IMyCubeBlock block) {
      return (block as MyCubeBlock).BuiltBy;
    }

    /**
     * Convert GetBlocksOfType to sane usage :|
     */
    public static List<T> GetBlocksOfType<T>(
        this IMyGridTerminalSystem Term,
        Func<T, bool> collect = null)
        where T : class, Sandbox.ModAPI.Ingame.IMyTerminalBlock {
      List<T> TermBlocks = new List<T>();
      Term.GetBlocksOfType<T>(TermBlocks, collect);
      return TermBlocks;
    }

    /**
     * Get the player currently flying a ship.
     */
    public static IMyPlayer GetControllingPlayer(this IMyCubeGrid grid) {
      IMyGridTerminalSystem term = grid.GetTerminalSystem();
      List<IMyShipController> shipControllers =
          term.GetBlocksOfType<IMyShipController>(collect: x => x.IsUnderControl);

      if (shipControllers.Count > 0) {
        return MyAPIGateway.Players.GetPlayerByID(
            shipControllers[0].ControllerInfo.ControllingIdentityId);
      }

      shipControllers = term.GetBlocksOfType<IMyShipController>(x => x.GetBuiltBy() != 0);
      if (shipControllers.Count > 0) {
        IMyShipController mainController = shipControllers.Find(x => x.IsMainCockpit);
        if (mainController == null) {
          mainController = shipControllers[0];
        }
        return MyAPIGateway.Players.GetPlayerByID(mainController.GetBuiltBy());
      }
      return null;
    }

    /**
     * Return the major owner of the given grid.
     */
    public static IMyPlayer GetOwningPlayer(this IMyCubeGrid grid) {
      if (grid.BigOwners.Count == 0) {
        return null;
      }
      foreach (long owner in grid.BigOwners) {
        IMyPlayer player = MyAPIGateway.Players.GetPlayerByID(owner);
        if (player != null) {
          return player;
        }
      }
      return null;
    }

    /**
     * Return the major owner of the given grid.
     */
    public static IMyFaction GetOwningFaction(this IMyCubeGrid grid) {
      if (grid.BigOwners.Count == 0) {
        return null;
      }
      foreach (long owner in grid.BigOwners) {
        IMyFaction ownedFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(owner);
        if (ownedFaction != null) {
          return ownedFaction;
        }
        foreach (IMyFaction faction in  MyAPIGateway.Session.Factions.Factions.Values) {
          if (faction.IsMember(owner)) {
            return faction;
          }
        }
      }

      return null;
    }

    //********************************************************************************************
    // Specific block type helpers.
    //********************************************************************************************
  };


} // namespace FSTC