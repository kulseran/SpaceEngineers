using System;
using Sandbox.Game;
using VRage.Utils;

namespace FSTC {

  public static class Util {
    private const bool LOGGING_ENABLED = true;
    private const bool DEBUG_MODE = true;

    public static Random rand = new Random();

    /**
     * Logging utlility
     */
    public static void Log(string argument) {
      if (!LOGGING_ENABLED) {
        return;
      }
      MyLog.Default.WriteLineAndConsole("FSTC: " + argument);
      if (DEBUG_MODE) {
        MyVisualScriptLogicProvider.ShowNotificationToAll("FSTC: " + argument, 10000, "White");
      }
    }

    /**
     * Logging utlility
     */
    public static void Warning(string argument) {
      if (!LOGGING_ENABLED) {
        return;
      }
      MyLog.Default.WriteLineAndConsole("FSTC: (warn) " + argument);
      if (DEBUG_MODE) {
        MyVisualScriptLogicProvider.ShowNotificationToAll("FSTC: " + argument, 10000, "Yellow");
      }
    }

    /**
     * Logging utlility
     */
    public static void Error(string argument) {
      if (!LOGGING_ENABLED) {
        return;
      }
      MyLog.Default.WriteLineAndConsole("FSTC: (error) " + argument);
      if (DEBUG_MODE) {
        MyVisualScriptLogicProvider.ShowNotificationToAll("FSTC: " + argument, 10000, "Red");
      }
    }
  }
}  // namespace FSTC
