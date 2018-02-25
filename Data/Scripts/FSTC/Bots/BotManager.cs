
using Sandbox.ModAPI;
using System.Collections.Generic;
using VRage.Game.ModAPI;

namespace FSTC {

  public static class BotManager {

    public enum BotType {
      CargoShip,
      Fighter
    };

    private static List<BotBase> m_activeBots = new List<BotBase>();

    public static BotBase CreateBot(BotType botType, IMyRemoteControl remote) {
      BotBase bot = null;
      switch (botType) {
        case BotType.CargoShip:
          bot = new CargoBot(remote);
          break;
        case BotType.Fighter:
          break;
      }

      if (bot != null && bot.Active) {
        m_activeBots.Add(bot);
      }
      return bot;
    }

    public static void RemoveBot(BotBase bot) {
      m_activeBots.Remove(bot);
    }
  };


} // namespace FSTC