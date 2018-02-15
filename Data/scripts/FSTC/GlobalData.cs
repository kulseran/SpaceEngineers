

namespace FSTC {

  /**
   * Global Data for the Campaign
   */
  public static class GlobalData {

    public static FSTCData world = FstcInitialData.Get();

    /**
     * Move to the next game tick
     */
    public static void NextTick() {
      world.currentTick++;
    }
  }

}