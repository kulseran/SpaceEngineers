using Sandbox.ModAPI;
using System.Collections.Generic;
using System.Linq;
using VRage.Game.ModAPI;

namespace FSTC {
  public static class Diplomacy {
    /**
     * Empire classification.
     * Determines how this empire reacts diplomatically to other empires.
     */
    public enum EmpireType {
      // True Neutrals will never go to war.
      TRUE_NEUTRAL = 0,
      // Neutrals will go to war if provoked, but can eventually be pursuaded to peace.
      NEUTRAL = 1,
      // Police are normally neutral, however will go to war for short periods of time to protect any Neutral class.
      POLICE = 2,
      // Hostiles start at war, however, can be pursuaded to peace for a time.
      HOSTILE = 3,
      // True Hostiles have no desire for peace.
      TRUE_HOSTILE = 4
    };

    public static void Initialize() {
    }
  }
}
