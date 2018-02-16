using System;
using Sandbox.Game;
using VRage.Utils;

namespace FSTC {

  public static class Util {
    private const bool LOGGING_ENABLED = true;
    private const bool DEBUG_MODE = true;

    public static Random rand = new Random();

    /**
     * Display player-visible notification.
     */
    public static void Notify(string argument) {
      if (!LOGGING_ENABLED) {
        return;
      }
      if (!DEBUG_MODE) {
        return;
      }
      MyVisualScriptLogicProvider.ShowNotificationToAll("FSTC: " + argument, 10000, "White");
    }

    /**
     * Convert seconds to ticks.
     */
    public static long TickSeconds(long v) {
      return v * 60;
    }

    /**
     * Convert minutes to ticks.
     */
    public static long TickMinutes(long v) {
      return TickSeconds(60) * v;
    }

    /**
     * Logging utlility
     */
    public static void Log(string argument) {
      if (!LOGGING_ENABLED) {
        return;
      }
      MyLog.Default.WriteLine("FSTC: " + argument);
    }

    /**
     * Logging utlility
     */
    public static void Warning(string argument) {
      if (!LOGGING_ENABLED) {
        return;
      }
      MyLog.Default.WriteLineAndConsole("FSTC: (warn) " + argument);
    }

    /**
     * Logging utlility
     */
    public static void Error(string argument) {
      if (!LOGGING_ENABLED) {
        return;
      }
      MyLog.Default.WriteLineAndConsole("FSTC: (error) " + argument);
    }
  }
}  // namespace FSTC
