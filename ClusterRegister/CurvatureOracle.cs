using System;
using System.Collections.Generic;
using Framework;
using System.Diagnostics;

namespace ClusterRegister
{
    class CurvatureOracle : AbstractOracle
    {
        public CurvatureOracle(RegistrationConfig config, TriangleMesh mesh, TriangleMesh mesh2, 
            Point3D[] P, Point3D[] Q, Random[] generators, 
            Point3D[] Pn, Point3D[] Qn)
        {
            this.config = config;
            this.mesh = mesh;
            this.mesh2 = mesh2;
            this.P = P;
            this.Q = Q;
            this.generators = generators;
            this.Pn = Pn;
            this.Qn = Qn;       
        }

        RegistrationConfig config;
        PointRecord[] samples;
        int[] sampleIDs;        
        KDTree2D sampleTree;
        Candidate[] candidates;
        bool[][] added;
        TriangleMesh mesh, mesh2;
        Point3D[] P, Q;
        Point3D[] Pn, Qn;        
        Random[] generators;

        private const double radiusSearch = 100;
        private const double valueAtRadius = 0.001;

        public override Candidate[] findCandidates()
        {
            HashSet<int> sampleIndices = new HashSet<int>();
            samples = new PointRecord[config.PSampleCount];
            sampleIDs = new int[config.PSampleCount];

            CornerTable ctP = new CornerTable(mesh);

            NeighboursCache PCache = new NeighboursCache(ctP, P.Length);

            object[] input = new object[config.PSampleCount / 100 + 1];
            for (int i = 0; i < input.Length; i++)
                input[i] = (object)(i * 100);

            int pc = config.threadCount;
            if (config.deterministic)
                pc = 1;

            if (config.deterministic)
            {
                added = new bool[1][];
                added[0] = new bool[mesh.Points.Length];
            }
            else
            {
                added = new bool[config.threadCount < 0 ? Environment.ProcessorCount : config.threadCount][];
                for (int i = 0; i < generators.Length; i++)
                {
                    added[i] = new bool[mesh.Points.Length];
                }
            }

            ThreadedExecution<object, object> te = new ThreadedExecution<object, object>(input,
                new ThreadedExecution<object, object>.ExecBody(this.initIndividuals),
                pc,
                null,
                new object[] { P, sampleIndices, PCache, Pn, mesh });

            Stopwatch s2 = Stopwatch.StartNew();
            te.Execute();
            s2.Stop();

            Time.LogTime(ElapsedTime.SAMPLING, s2.ElapsedMilliseconds);

            Stopwatch s3 = Stopwatch.StartNew();

            candidates = new Candidate[config.populationSize];

            sampleTree = new KDTree2D(samples);

            CornerTable ctQ = new CornerTable(mesh2);

            NeighboursCache QCache = new NeighboursCache(ctQ, Q.Length);

            input = new object[config.populationSize / 100 + 1];
            for (int i = 0; i < input.Length; i++)
                input[i] = (object)(i * 100);

            if (config.deterministic)
            {
                added = new bool[1][];
                added[0] = new bool[mesh2.Points.Length];
            }
            else
            {
                added = new bool[config.threadCount < 0 ? Environment.ProcessorCount : config.threadCount][];
                for (int i = 0; i < generators.Length; i++)
                    added[i] = new bool[mesh2.Points.Length];

            }

            ThreadedExecution<object, object> te2 = new ThreadedExecution<object, object>(input, 
                new ThreadedExecution<object, object>.ExecBody(this.FindMatches),
                pc, null, new object[] { Q, QCache, Qn, mesh2 });
            te2.Execute();
            s3.Stop();
            Time.LogTime(ElapsedTime.CANDIDATES, s3.ElapsedMilliseconds);

            return candidates;
        }//findCandidates

        private object initIndividuals(object input, int threadId, object[] dat)
        {
            object[] data = (object[])dat[1];
            Point3D[] mesh = (Point3D[])data[0];
            HashSet<int> sampleIndices = (HashSet<int>)data[1];
            NeighboursCache ctP = (NeighboursCache)data[2];
            Point3D[] Pn = (Point3D[])data[3];
            TriangleMesh tm = (TriangleMesh)data[4];
            int start = (int)input;
            Random r = generators[threadId];
            bool[] add = added[threadId];
            for (int i = start; (i < start + 100) && (i < config.PSampleCount); i++)
            {
                while (true)
                {
                    int index = tm.uniformRandomVertex(r);
                    lock (this)
                    {
                        while (sampleIndices.Contains(index))
                            index = tm.uniformRandomVertex(r);
                    }
                    samples[i] = curvatures(index, mesh, ctP, Pn, tm, add, config.nbHoodWidth);
                    sampleIDs[i] = index;

                    if ((!double.IsNaN(samples[i].k1)) && (curvatureEqualityCondition(samples[i])))
                        break;
                }
            }
            return null;
        }

        private object FindMatches(object input, int threadId, params object[] paramet)
        {
            object[] parameters = (object[])paramet[1];
            int start = (int)input;
            Random r = generators[threadId];
            Point3D[] pts = (Point3D[])parameters[0];
            NeighboursCache ctQ = (NeighboursCache)parameters[1];
            Point3D[] Qn = (Point3D[])parameters[2];
            TriangleMesh tm = (TriangleMesh)parameters[3];

            PointRecord Qsample;
            for (int i = start; (i < (start + 100)) && (i < candidates.Length); i++)
            {
                PointRecord match;
                int ps = 0, qs;
                while (true)
                {
                    qs = tm.uniformRandomVertex(r);
                    Qsample = curvatures(qs, pts, ctQ, Qn, tm, added[threadId], config.nbHoodWidth);
                    while (double.IsNaN(Qsample.k1) || (!curvatureEqualityCondition(Qsample)))
                    {
                        qs = tm.uniformRandomVertex(r);
                        Qsample = curvatures(qs, pts, ctQ, Qn, tm, added[threadId], config.nbHoodWidth);
                    }

                    double d = 0;
                    int nearest = sampleTree.findNearest(Qsample, out d);
                    ps = sampleIDs[nearest];
                    match = samples[nearest];
                    break;
                }

                Candidate p = new Candidate(Qsample, match, false, qs, ps);

                p.initRT();
                candidates[i] = p;
                if ((i + 1) < (start + 100))
                {
                    Candidate p2 = new Candidate(Qsample, match, true, qs, ps);

                    p2.initRT();
                    candidates[i + 1] = p2;
                    i++;
                }

            }
            return (null);
        }//FindMatches

        bool curvatureEqualityCondition(PointRecord r)
        {
            double cr = r.k1 / r.k2;
            if (cr < config.curvatureRatio)
                return true;
            if (cr > (1 / config.curvatureRatio))
                return true;
            return false;
        }

        /// <summary>
        /// computes curvatures of a vertex in a mesh, returns pointrecord
        /// </summary>
        /// <param name="index">Vertex index</param>
        /// <param name="pts">Array of points</param>
        /// <param name="cache">Cache of neighbour queries</param>
        /// <param name="normals">Array of point normals</param>
        /// <param name="mesh">Input triangle mesh. Only connectivity is used.</param>
        /// <param name="add">Bool array used by the queries. Passed in order to avoid allocating it in each query.</param>
        /// <returns>PointRecord instance encapuslating the curvatures and curvature directions.</returns>
        public static PointRecord curvatures(int index, Point3D[] pts, NeighboursCache cache, Point3D[] normals, TriangleMesh mesh, bool[] add, int nbhoodWidth)
        {
            List<int> neighbours;
            neighbours = getNeighboursEuclidPC(index, mesh, radiusSearch);

            if (neighbours == null)
            {
                PointRecord fail = new PointRecord();
                fail.k1 = double.NaN;
                return (fail);
            }

            double[] weights = GetWeights(neighbours, pts[index], pts);
            Point3D pointNormal = normals[index];

            // build local basis
            Point3D v3 = pointNormal;
            Point3D v1 = pts[neighbours[1]].GetSubtract(ref pts[index]);
            v1 -= v3 * v1.DotProductRef(ref v3);
            double l = v1.Abs();
            if (l < 0.000001)
            {
                PointRecord fail = new PointRecord();
                fail.k1 = double.NaN; // not really, the curvature can be determined, but the eigenvectors cannot, and thus we report failure
                return (fail);
            }

            v1 *= 1 / l;
            Point3D v2 = v1.CrossProductRef(ref v3);
            Point3D o = pts[index];

            // matrix of rotation, we only need first two rows
            double[] rot = new double[6];
            rot[0] = v1.X;
            rot[3] = v2.X;
            rot[1] = v1.Y;
            rot[4] = v2.Y;
            rot[2] = v1.Z;
            rot[5] = v2.Z;

            // rotated translation
            double[] rtr = new double[]{
                -rot[0]*o.X -rot[1]*o.Y -rot[2]*o.Z,
                -rot[3]*o.X -rot[4]*o.Y -rot[5]*o.Z
            };

            double[] m = new double[5];
            double[] r = new double[3];
            for (int i = 0; i < neighbours.Count; i++)
            {
                double w = weights[i];
                Point3D p = pts[neighbours[i]];
                Point3D normal = normals[neighbours[i]];

                double lpx = rot[0] * p.X + rot[1] * p.Y + rot[2] * p.Z + rtr[0];
                double lpy = rot[3] * p.X + rot[4] * p.Y + rot[5] * p.Z + rtr[1];
                double lnx = rot[0] * normal.X + rot[1] * normal.Y + rot[2] * normal.Z;
                double lny = rot[3] * normal.X + rot[4] * normal.Y + rot[5] * normal.Z;

                double wlpx = w * lpx;
                double wlpy = w * lpy;
                double wlnx = w * lnx;
                double wlny = w * lny;

                double wlpxlpx = wlpx * wlpx;
                double wlpxlpy = wlpx * wlpy;
                double wlpylpy = wlpy * wlpy;

                m[0] += wlpxlpx;
                m[1] += wlpxlpy;
                r[0] += wlpx * wlnx;
                r[1] += wlpy * wlnx;
                m[4] += wlpylpy;
                r[1] += wlpx * wlny;
                r[2] += wlpy * wlny;
            }
            m[2] += m[0] + m[4];
            m[3] += m[1];

            if (singular3x3(m))
            {
                PointRecord fail = new PointRecord();
                fail.k1 = double.NaN; // not really, the curvature can be determined, but the eigenvectors cannot, and thus we report failure
                return (fail);
            }

            double[] s = solve3x3SymStripe(m[0], m[1], m[2], m[3], m[4], r);

            double e1, e2, egv1, egv2;
            evd2x2(out e1, out e2, out egv1, out egv2, s[0], s[1], s[2]);

            v1.Multiply(egv1);
            v2.Multiply(egv2);
            v1.AddRef(ref v2);
            v1.Normalize();

            PointRecord res = new PointRecord();
            res.n = v3;
            res.ev1 = v1;
            res.k1 = e1;
            res.k2 = e2;
            res.p = o;

            return (res);
        }//curvatures
        
        //__________________________________Methods for curvatures()____________________________________________________________________
        private static List<int> getNeighboursEuclidPC(int index, TriangleMesh tm, double dist)
        {
            var result = tm.Tree.findAllCloserThan(tm.Points[index], dist);

            if (result == null || result.Count < 5)
                return null;
            return result;
        }
        private static double[] GetWeights(List<int> neighbours, Point3D centre, Point3D[] points)
        {
            double d;
            double[] weights = new double[neighbours.Count];
            for (int i = 0; i < neighbours.Count; i++)
            {
                d = centre.DistanceTo(points[neighbours[i]]);
                weights[i] = gaussian(radiusSearch, valueAtRadius, d);
            }
            return weights;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="d">Maximum distance.</param>
        /// <param name="i">Value the function returns for d.</param>
        /// <param name="x">The distance for which the gaussian is calculated.</param>
        /// <returns></returns>
        private static double gaussian(double d, double i, double x)
        {
            double c = Math.Abs(d * d / Math.Log(i));

            return Math.Exp(-x * x / c);
        }
        private static bool singular3x3(double[] m)
        {
            double D = m[0] * m[2] - m[1] * m[1];
            if (Math.Abs(D) < 0.0000000001)
                return true;
            double D2 = m[0] * m[3];
            double y = D2 / D;
            if (Math.Abs(m[3] * y - m[4]) < 0.000001)
                return true;
            return false;
        }

        /// <summary>
        /// solve symmetric stripe matrix in the form
        /// a b 0
        /// b c d
        /// 0 d e
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        /// <param name="d"></param>
        /// <param name="e"></param>
        /// <param name="r"></param>
        /// <returns></returns>
        private static double[] solve3x3SymStripe(double a, double b, double c, double d, double e, double[] r)
        {
            double det = a * c * e - d * d * a - e * b * b;
            double det1 = r[0] * c * e + b * d * r[2] - d * d * r[0] - e * r[1] * b;
            double det2 = a * r[1] * e - r[2] * d * a - e * b * r[0];
            double det3 = a * c * r[2] + r[0] * b * d - d * r[1] * a - r[2] * b * b;
            return (new double[] { det1 / det, det2 / det, det3 / det });
        }

        /// <summary>
        /// eigenvalue decomposition of a 2x2 matrix, uses characteristic polynomial
        /// </summary>
        /// <param name="e1"></param>
        /// <param name="e2"></param>
        /// <param name="v1"></param>
        /// <param name="v2"></param>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="c"></param>
        private static void evd2x2(out double e1, out double e2, out double v1, out double v2, double a, double b, double c)
        {
            double d = Math.Sqrt((a - c) * (a - c) + 4 * b * b);
            e1 = (a + c + d) / 2;
            e2 = (a + c - d) / 2;

            if (Math.Abs(e2) < Math.Abs(e1))
            {
                double tmp = e2;
                e2 = e1;
                e1 = tmp;
            }

            v1 = 1;
            v2 = 1;
            if (Math.Abs(b) < Math.Abs(a - e1))
            {
                v1 = -b / (a - e1);
            }
            else
            {
                v2 = (e1 - a) / b;
            }
        }
    }
}
