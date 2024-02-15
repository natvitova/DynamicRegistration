using System;

namespace Framework
{
    public class Candidate
    {
        // properties of Q        
        static double[] tpSum; // sum of tensor products, diagonal only
        static double dpsum; // sum of dot products
        static int n; // number of points
        static double invn; // inverse number of points

        public static double bodyDiagonalLength;

        static Point3D[] points; // points for which the distances between candidates will be evaluated        



        double[] rot; // rotation matrix
        double[] t; // translation

        public DQuat dq; // dual quaternion representation
        public double score = 0; // score of the candidate, higher is better

        public Point3D correspondence;

        // Clustering Algo
        public double WeightedDistToFac;

        public int source, target;
        public bool swap;

        public override bool Equals(object obj)
        {
            if (!(obj is Candidate))
                return false;
            Candidate o = (Candidate)obj;
            if (this.source != o.source)
                return false;
            if (this.target != o.target)
                return false;
            if (this.swap != o.swap)
                return false;
            return true;
        }

        public override int GetHashCode()
        {
            return source * target * (swap ? 1 : 2);
        }

        /// <summary>
        /// initializes rotation and translation from dual quaternion representation
        /// </summary>
        public void initRT()
        {
            this.rot = new double[9];
            this.t = new double[3];
            this.dq.fillRT(rot, t);
        }

        /// <summary>
        /// initializes the properties of Q
        /// </summary>
        /// <param name="points">Points of Q</param>
        public static void initSums(Point3D[] points)
        {
            dpsum = 0;
            tpSum = new double[3];
            
            Point3D minPoint = new Point3D(double.MaxValue, double.MaxValue, double.MaxValue);
            Point3D maxPoint = new Point3D(double.MinValue, double.MinValue, double.MinValue);
            for (int i = 0; i < points.Length; i++)
            {
                Point3D p = points[i];

                tpSum[0] += p.X * p.X;
                tpSum[1] += p.Y * p.Y;
                tpSum[2] += p.Z * p.Z;

                dpsum += p.AbsSq();

                minPoint.X = Math.Min(p.X, minPoint.X);
                minPoint.Y = Math.Min(p.Y, minPoint.Y);
                minPoint.Z = Math.Min(p.Z, minPoint.Z);

                maxPoint.X = Math.Max(p.X, maxPoint.X);
                maxPoint.Y = Math.Max(p.Y, maxPoint.Y);
                maxPoint.Z = Math.Max(p.Z, maxPoint.Z);
            }

            n = points.Length;
            invn = 1.0 / n;

            Candidate.points = points;

            bodyDiagonalLength = minPoint.DistanceTo(maxPoint);
        }

        /// <summary>
        /// creates a candidate transformation that maps s to t, including normal and principal curvature directions.
        /// </summary>
        /// <param name="s">Source pointrecord</param>
        /// <param name="t">Target pointrecord</param>
        /// <param name="swap">determines whether or not the orientation of the map should be swapped</param>
        public Candidate(PointRecord s, PointRecord t, bool swap, int source, int target)
        {
            this.source = source;
            this.target = target;
            this.swap = swap;
            DQuat trans = DQuat.Identity();
            correspondence = s.p;
            trans.AddTranslation(-s.p.X + t.p.X, -s.p.Y + t.p.Y, -s.p.Z + t.p.Z); // translation maps the source point to the target point

            // rotate to match normal directions
            Point3D axis = (Point3D)s.n.CrossProductRef(ref t.n);
            DQuat r2 = DQuat.Identity();
            if (axis.Abs() > 0.0000001)
            {
                double angle = Math.Acos(s.n.DotProductRef(ref t.n));
                r2 = DQuat.generalRotation(t.p, t.p + axis, angle);
            }

            // eigenvector 1
            Point3D ev1 = s.p + s.ev1;
            Point3D ev1t = (r2 * trans).Transform(ev1); // after rotation
            double acos = (ev1t - t.p).DotProductRef(ref t.ev1);
            if (acos > 1)
                acos = 1;
            if (acos < -1)
                acos = -1;
            double angle2 = Math.Acos(acos);
            axis = (t.ev1).CrossProduct(ev1t - t.p);
            axis *= 1.0 / axis.Abs();
            DQuat r3 = DQuat.generalRotation(t.p, t.p + axis, -angle2 + (swap ? Math.PI : 0)); // rotate to match the eigenvector. If swap, then PI is added 

            this.dq = r3 * r2 * trans;
        }

        public Candidate(Transform3D2 tr)
        {
            rot = new double[9];
            t = new double[3];
            rot[0] = tr[0, 0];
            rot[1] = tr[0, 1];
            rot[2] = tr[0, 2];
            rot[3] = tr[1, 0];
            rot[4] = tr[1, 1];
            rot[5] = tr[1, 2];
            rot[6] = tr[2, 0];
            rot[7] = tr[2, 1];
            rot[8] = tr[2, 2];
            t[0] = tr[0, 3];
            t[1] = tr[1, 3];
            t[2] = tr[2, 3];
        }

        public Candidate(DQuat dQuat)
        {
            this.dq = DQuat.Identity();
            this.dq.real.q0 = dQuat.real.q0;
            this.dq.real.q1 = dQuat.real.q1;
            this.dq.real.q2 = dQuat.real.q2;
            this.dq.real.q3 = dQuat.real.q3;
            this.dq.dual.q0 = dQuat.dual.q0;
            this.dq.dual.q1 = dQuat.dual.q1;
            this.dq.dual.q2 = dQuat.dual.q2;
            this.dq.dual.q3 = dQuat.dual.q3;
        }

        public double SqrtDistanceTo(Candidate target)
        {
            double d = DistanceTo(target);
            if (d < 0)
                d = 0;
            return Math.Sqrt(d);
        }

        /// <summary>
        /// brute force version of the transformation distance function
        /// </summary>
        /// <param name="c"></param>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public double bfDistanceTo(Candidate c, TriangleMesh mesh)
        {
            double sum = 0;
            //for(int i = 0;i<mesh.Points.Length;i++)
            for (int i = 0; i < Candidate.points.Length; i++)
            {
                //Point3D p = mesh.Points[i];
                Point3D p = Candidate.points[i];
                double x1 = rot[0] * p.X + rot[1] * p.Y + rot[2] * p.Z + t[0];
                double y1 = rot[3] * p.X + rot[4] * p.Y + rot[5] * p.Z + t[1];
                double z1 = rot[6] * p.X + rot[7] * p.Y + rot[8] * p.Z + t[2];

                double x2 = c.rot[0] * p.X + c.rot[1] * p.Y + c.rot[2] * p.Z + c.t[0];
                double y2 = c.rot[3] * p.X + c.rot[4] * p.Y + c.rot[5] * p.Z + c.t[1];
                double z2 = c.rot[6] * p.X + c.rot[7] * p.Y + c.rot[8] * p.Z + c.t[2];

                double d = (x1 - x2) * (x1 - x2) + (y1 - y2) * (y1 - y2) + (z1 - z2) * (z1 - z2);
                sum += d;
            }
            return (Math.Sqrt(sum * invn));
        }

        /// <summary>
        /// Caution! Only works if the mesh is centered and rotated correctly, otherwise use bfDistanceTo(...)!
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        public double DistanceTo(Candidate target)
        {
            //return DistanceToMitra(target);
            double[] r1 = this.rot;
            double[] r2 = target.rot;
            double[] t1 = this.t;
            double[] t2 = target.t;


            double r1tr20 = r1[0] * r2[0] + r1[3] * r2[3] + r1[6] * r2[6];
            double r1tr21 = r1[1] * r2[1] + r1[4] * r2[4] + r1[7] * r2[7];
            double r1tr22 = r1[2] * r2[2] + r1[5] * r2[5] + r1[8] * r2[8];

            double t1t2 = t1[0] * t2[0] + t1[1] * t2[1] + t1[2] * t2[2];
            double t2t2 = t2[0] * t2[0] + t2[1] * t2[1] + t2[2] * t2[2];
            double t1t1 = t1[0] * t1[0] + t1[1] * t1[1] + t1[2] * t1[2];

            double result = 2 * (dpsum) + n * (t2t2 + t1t1 - 2 * t1t2);

            result -= 2 * (r1tr20 * tpSum[0] + r1tr21 * tpSum[1] + r1tr22 * tpSum[2]);

            result *= invn;

            return (result);
        }
    }
}
