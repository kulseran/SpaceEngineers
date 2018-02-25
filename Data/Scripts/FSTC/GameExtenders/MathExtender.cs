using System;
using VRageMath;

namespace FSTC {

  /**
   * Helper functions for math primitives.
   */
  public static class MathExtender {

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

    /**
     * Generate a random point on a sphere.
     */
    public static Vector3D RandomPointOnSphere(Vector3D center, float radius) {
      double z = Util.rand.NextDouble();
      double t = Util.rand.NextDouble();
      double x = Math.Sqrt(1 - z*z) * Math.Cos(t);
      double y = Math.Sqrt(1 - z*z) * Math.Sin(t);
      return new Vector3D(x, y, z) * radius + center;
    }

    public static Vector3D RandomPerpendicularVector(Vector3D axis) {
      Vector3D tangent = Vector3D.CalculatePerpendicularVector(axis);
      Vector3D bitangent = Vector3D.Cross(axis, tangent);
      double angle = Util.rand.NextDouble() * 2 * MathHelper.Pi;
      return Math.Cos(angle) * tangent + Math.Sin(angle) * bitangent;
    }
  };

} // namespace FSTC