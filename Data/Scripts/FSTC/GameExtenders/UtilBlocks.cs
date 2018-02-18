using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage;
using VRage.Game;
using VRage.Game.ModAPI;
using VRage.ModAPI;
using VRage.ObjectBuilders;

namespace FSTC {

  /**
   * Helper functions for managing grid data.
   */
  public static class BlockManagement {

    private static readonly MyFixedPoint AMOUNT_ICE_TO_ADD = 2000;

    public static bool IsValidGrid(IMyCubeGrid targetGrid) {
      var gridEntity = targetGrid as IMyEntity;
      if (targetGrid == null || MyAPIGateway.Entities.Exist(gridEntity) == false) {
        return false;
      }
      return true;
    }

    /**
     * Locates all the Oxygen generator class blocks on the given grid,
     * then adds 2000 ice if there is room to do so.
     *
     * Only call on server.
     */
    public static void FillOxygenGenerators(IMyCubeGrid targetGrid) {
      List<IMySlimBlock> blockList = new List<IMySlimBlock>();
      targetGrid.GetBlocks(blockList, b => b.FatBlock is Sandbox.ModAPI.IMyGasGenerator);
      foreach (var block in blockList) {
        IMyGasGenerator gasGenerator = ((IMyGasGenerator)block.FatBlock);
        if (gasGenerator == null) {
          continue;
        }

        MyDefinitionId definitionId = new MyDefinitionId(typeof(MyObjectBuilder_Ore), "Ice");
        MyObjectBuilder_InventoryItem inventoryItem = new MyObjectBuilder_InventoryItem {
          Amount = AMOUNT_ICE_TO_ADD,
          Content = (MyObjectBuilder_PhysicalObject)MyObjectBuilderSerializer.CreateNewObject(definitionId)
        };

        IMyInventory gasGeneratorInv = gasGenerator.GetInventory(0);
        if (!gasGeneratorInv.CanItemsBeAdded(AMOUNT_ICE_TO_ADD, definitionId)) {
          continue;
        }
        gasGeneratorInv.AddItems(AMOUNT_ICE_TO_ADD, inventoryItem.Content);
      }
    }

    /**
     * Resets all the ownerships on all blocks ot the given owner (default nobody).
     *
     * Only call on server.
     */
    public static void UnifyGridOwnership(IMyCubeGrid originGrid, long newOwnerId = 0) {
      List<IMyCubeGrid> gridList = MyAPIGateway.GridGroups.GetGroup(originGrid, GridLinkTypeEnum.Mechanical);
      foreach (IMyCubeGrid grid in gridList) {
        grid.ChangeGridOwnership(newOwnerId, MyOwnershipShareModeEnum.Faction);
        MyCubeGrid gridImpl = grid as MyCubeGrid;
        foreach (long owner in grid.SmallOwners) {
          gridImpl.TransferBlocksBuiltByID(owner, 0);
        }
      }
    }

    /**
     * Checks if all the blocks on the grid are all under NPC ownership.
     * If a player has "hacked" one of the blocks, it will no longer have NPC ownership.
     */
    public static bool NPCOwnershipCheck(IMyCubeGrid cubeGrid) {
      List<IMySlimBlock> blockList = new List<IMySlimBlock>();
      cubeGrid.GetBlocks(blockList, b => b.FatBlock is Sandbox.ModAPI.Ingame.IMyTerminalBlock);
      foreach (var block in blockList) {
        long blockOwner = block.OwnerId;
        if (blockOwner == 0) {
          continue;
        }
        IMyFaction ownerFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(blockOwner);
        if (ownerFaction == null || ownerFaction.Tag.Length <= 3) {
          return false;
        }
      }
      return true;
    }

    /**
     * Deletes a ship from space
     */
    public static void DespawnShip(IMyCubeGrid cubeGrid) {
      List<IMyCubeGrid> gridList = MyAPIGateway.GridGroups.GetGroup(cubeGrid, GridLinkTypeEnum.Mechanical);
      foreach (IMyCubeGrid grid in gridList) {
        grid.Delete();
      }
    }

  };

} // namespace FSTC
