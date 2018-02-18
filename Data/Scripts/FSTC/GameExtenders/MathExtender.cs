using VRageMath;

namespace FSTC {

  /**
   * Helper functions for math primitives.
   */
  public static class MathExtensions {

    /**
     * Convert a Vector3D into a GPS coordinate compatible with the clipboard.
     */
    public static string ToGPS(this Vector3D vec, string name) {
      return "GPS:" + name + ":" + vec.X.ToString() + ":" + vec.Y.ToString() + ":" + vec.Z.ToString() + ":";
    }

    /**
     * Adds distance measuring to a Vector3D object.
     */
    public static double DistanceTo(this Vector3D vecStart, Vector3D vecEnd) {
      return (vecEnd - vecStart).Length();
    }

  };

} // namespace FSTC