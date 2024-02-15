
namespace Framework
{
    // stores information about a particular point
    public struct PointRecord
    {
        // principal curvatures
        public double k1, k2;

        // principal curvature direction, normal, coordinates
        public Point3D ev1, n, p;

        /// <summary>
        /// difference of points based on curvature
        /// </summary>
        /// <param name="pointRecord">target</param>
        /// <returns></returns>
        public double SquareDistanceTo(PointRecord pointRecord)
        {
            double dk1 = this.k1 - pointRecord.k1;
            double dk2 = this.k2 - pointRecord.k2;
            return (dk1 * dk1 + dk2 * dk2);
        }
    }
}
