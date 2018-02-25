using System;
using System.Collections.Generic;
using Sandbox.Game;
using VRage.Utils;

namespace FSTC {

  public static class Util {
    private const bool LOGGING_ENABLED = true;
    private const bool DEBUG_MODE = true;

    public static Random rand = new Random();

    public static void Swap<T>(ref T lhs, ref T rhs) {
      T temp = lhs;
      lhs = rhs;
      rhs = temp;
    }

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

    public static void Shuffle<T>(this IList<T> list) {
      for (var i = 0; i < list.Count; i++) {
        list.Swap(i, rand.Next(i, list.Count));
      }
    }

    public static void Swap<T>(this IList<T> list, int i, int j) {
      var temp = list[i];
      list[i] = list[j];
      list[j] = temp;
    }

    /**
     * Logging utlility
     */
    static string lastLog = null;
    public static void Log(string argument) {
      if (!LOGGING_ENABLED) {
        return;
      }
      if (lastLog != null && lastLog.Equals(argument)) {
        return;
      }
      lastLog = argument;
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
