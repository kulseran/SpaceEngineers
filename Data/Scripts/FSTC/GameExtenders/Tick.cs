
namespace FSTC {

  public static class Tick {
    /**
     * Convert seconds to ticks.
     */
    public static long Seconds(long v) {
      return v * 60;
    }

    /**
     * Convert minutes to ticks.
     */
    public static long Minutes(long v) {
      return Seconds(60) * v;
    }
  };


} // namespace FSTC