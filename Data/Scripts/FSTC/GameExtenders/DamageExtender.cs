using Sandbox.Game.Entities;
using Sandbox.ModAPI;
using Sandbox.ModAPI.Weapons;
using VRage.Game.ModAPI;
using VRage.ModAPI;

namespace FSTC {

  /**
   * Utility functions that extend {@link MyDamageInformation} to
   * provide additional details about the source of the damage.
   */
  public static class DamageExtender {

    /**
     * Determine the source Player and Ship associated with a damage event.
     * Returns false if the event was environmental, or no player/ship can be determined.
     */
    public static bool GetSource(
        this MyDamageInformation info,
        out IMyPlayer damagePlayer,
        out IMyFaction damageFaction,
        out IMyCubeGrid damageShip) {
      damagePlayer = null;
      damageFaction = null;
      damageShip = null;

      IMyEntity entity = MyAPIGateway.Entities.GetEntityById(info.AttackerId);
      if (entity == null) {
        return false;
      }
      if (entity is IMyMeteor) {
        return false;
      }
      if (entity is IMyThrust) {
        return false;
      }
      if (entity is IMyWarhead) {
        return GetPlayerByWarhead(
            entity as IMyWarhead,
            out damagePlayer,
            out damageFaction);
      }

      entity = entity.GetTopMostParent();
      if (entity == null) {
        return false;
      }

      if (entity is IMyCubeGrid) {
        damageShip = entity as IMyCubeGrid;
        damagePlayer = damageShip.GetControllingPlayer();
        if (damagePlayer == null) {
          damagePlayer = damageShip.GetOwningPlayer();
        }
        damageFaction = damageShip.GetOwningFaction();
      } else {
        if (entity is IMyEngineerToolBase) {
          return GetPlayerByTool(
              entity as IMyEngineerToolBase,
              out damagePlayer,
              out damageFaction);
        }

        if (entity is IMyGunBaseUser) {
          return GetPlayerByWeapon(
              entity as IMyGunBaseUser,
              out damagePlayer,
              out damageFaction);
        }
      }

      return damageShip != null || damageFaction != null || damagePlayer != null;
    }

    /**
     * Get the owner of a Warhead.
     */
    private static bool GetPlayerByWarhead(
        IMyWarhead warhead,
        out IMyPlayer owner,
        out IMyFaction owningFaction) {
      if (warhead.OwnerId == 0) {
        long playerId = (warhead as MyCubeBlock).BuiltBy;
        owner = MyAPIGateway.Players.GetPlayerByID(playerId);
        owningFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId);
      } else {
        long playerId = warhead.OwnerId;
        owner = MyAPIGateway.Players.GetPlayerByID(playerId);
        owningFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(playerId);
      }
      return owner != null || owningFaction != null;
    }

    /**
     * Get the owner of a Tool
     */
    private static bool GetPlayerByTool(
        IMyEngineerToolBase tool,
        out IMyPlayer owner,
        out IMyFaction owningFaction) {
      owner = MyAPIGateway.Players.GetPlayerByID(tool.OwnerIdentityId);
      owningFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(tool.OwnerIdentityId);
      return owner != null || owningFaction != null;
    }

    /**
     * Get the owner of a player Weapon
     */
    private static bool GetPlayerByWeapon(
        IMyGunBaseUser gun,
        out IMyPlayer owner,
        out IMyFaction owningFaction) {
      owner = MyAPIGateway.Players.GetPlayerByID(gun.OwnerId);
      owningFaction = MyAPIGateway.Session.Factions.TryGetPlayerFaction(gun.OwnerId);
      return owner != null || owningFaction != null;
    }
  };

} // namespace FSTC