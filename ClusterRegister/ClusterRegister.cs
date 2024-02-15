using System.Collections.Generic;
using System;
using System.IO;
using System.Globalization;
using Framework;

using System.Diagnostics;


namespace ClusterRegister
{
    public class ClusterRegistration
    {
        // The delta for the LCP.        
        private double delta = 0.04;

        private double qRadius = 0;

        // set delta automatically as a fraction of body diagonal 
        private bool auto_delta = true;

        // determines whether the set P will be sampled or not
        private bool sample_P = false;

        RegistrationConfig config;

        Candidate[] centers;
        Candidate[] candidates;

        /// <summary>
        /// used for evaluation purposes only
        /// </summary>
        public Transform3D2 correctTransform { get; set; }

        public ClusterRegistration()
        {
            correctTransform = new Transform3D2();
            correctTransform.SetIdentity();
        }

        public Transform3D2 Execute(TriangleMesh[] mesh, TriangleMesh[] mesh2, out int clusterIndex, ElapsedTime elapsedTime)
        {
            RegistrationConfig config = readFromFile("config.txt");
            return Execute(mesh, mesh2, config, out clusterIndex, elapsedTime);
        }
        public Transform3D2 Execute(TriangleMesh[] meshP, TriangleMesh[] meshQ, RegistrationConfig conf, out int clusterIndex, ElapsedTime elapsedTime)
        {
            Stopwatch init = Stopwatch.StartNew();
            this.config = conf;

            Point3D toCenter = new Point3D();
            Transform3D2 rot = null;
            DateTime s1 = DateTime.Now;

            Point3D[][] Qn = new Point3D[meshP.Length][];
            List<Candidate> allCandidates = new List<Candidate>(); 

            for (int i = 0; i < meshP.Length; i++)
            {
                Qn[i] = (Point3D[])meshQ[i].Normals.Clone();
            }
            
            List<Point3D> sampled_Q = null;
            List<Point3D> sampled_P = null;

            CreateSequenceTransform(meshP, meshQ, Qn, out toCenter, out rot, out sampled_P, out sampled_Q);
            Candidate.initSums(sampled_Q.ToArray());


            for (int i = 0; i < meshP.Length; i++)
            {
                Random[] generators = CreateGenerators(config);

                Point3D[] P = meshP[i].Points;
                Point3D[] Q = meshQ[i].Points;

                // rotate/translate Q so that pointsum is zero and tensor product sum is diagonal
                Q = new Point3D[Q.Length];
                for (int j = 0; j < Q.Length; j++)
                    Q[j] = rot.Transform(meshQ[i].Points[j] + toCenter);

                init.Stop();
                elapsedTime.LogTime(ElapsedTime.INITIALIZATION, init.ElapsedMilliseconds);

                Point3D[] Pn = (Point3D[])meshP[i].Normals.Clone();

                AbstractOracle oracle = new CurvatureOracle(config, meshP[i], meshQ[i], P, Q, generators, Pn, Qn[i]);
                oracle.Time = elapsedTime;

                candidates = oracle.findCandidates();

                allCandidates.AddRange(candidates);
            }

            candidates = allCandidates.ToArray();
            
            Logger.Log("Computing density...");
            Stopwatch s4 = Stopwatch.StartNew();

            // initialize clustering method

            // prepare points
            foreach (Candidate c in candidates)
            {
                c.initRT();
            }

            config.radius = meshQ[0].radius; 

            ISelector methodClustering = new Density();

            candidates = uniqueCandidates(candidates);
            DQuat dt = methodClustering.FindDualQuatCandidate(candidates, config, 0, out clusterIndex);

            Transform3D2 t = dt.getTransform2();
            candidates[0] = new Candidate(dt);

            s4.Stop();
            Logger.Log("Density computed in {0} ms.", s4.ElapsedMilliseconds);
            elapsedTime.LogTime(ElapsedTime.CLUSTERING, s4.ElapsedMilliseconds);

            Transform3D2 translateToCenter = new Transform3D2();
            translateToCenter.SetIdentity();
            translateToCenter.AddTranslation(toCenter.X, toCenter.Y, toCenter.Z);

            t = t * rot * translateToCenter;// *ro;            

            Logger.Log("Total time: {0} ms", (DateTime.Now - s1).TotalMilliseconds);

            return (t);
        }//Execute



        // reads config from config file
        public static RegistrationConfig readFromFile(string p)
        {
            RegistrationConfig config = new RegistrationConfig();
            NumberFormatInfo nfi = new CultureInfo("en-US", false).NumberFormat;
            StreamReader sr = new StreamReader(p);
            string line = sr.ReadLine();
            while (line != null)
            {
                if (line.StartsWith("%"))
                {
                    line = sr.ReadLine();
                    continue;
                }

                switch (line.Split(' ')[0])
                {
                    case "acceptance_threshold":
                        config.acceptanceThreshold = double.Parse(line.Split(' ')[1], nfi);
                        break;
                    case "sample_size_P":
                        config.populationSize = int.Parse(line.Split(' ')[1], nfi);
                        break;
                    case "nbhood_width":
                        config.nbHoodWidth = int.Parse(line.Split(' ')[1], nfi);
                        break;
                    case "clustering_iterations":
                        config.kMeansStepCount = int.Parse(line.Split(' ')[1], nfi);
                        break;
                    case "cluster_count":
                        config.clusterCount = int.Parse(line.Split(' ')[1], nfi);
                        break;
                    case "delta_ratio":
                        config.auto_delta_ratio = double.Parse(line.Split(' ')[1], nfi);
                        break;
                    case "psample_count":
                        config.PSampleCount = int.Parse(line.Split(' ')[1], nfi);
                        break;
                    case "spread":
                        config.spread = int.Parse(line.Split(' ')[1], nfi);
                        break;
                    case "clustering_method":
                        config.clusteringMethod = int.Parse(line.Split(' ')[1], nfi);
                        break;
                    case "verification_method":
                        config.verificationMethod = int.Parse(line.Split(' ')[1], nfi);
                        break;
                    case "deterministic":
                        config.deterministic = line.Split(' ')[1].Equals("true", StringComparison.OrdinalIgnoreCase);
                        break;
                    default:
                        Console.WriteLine("Unknown config line: " + line);
                        break;
                }
                line = sr.ReadLine();
            }
            sr.Close();
            return config;
        }

        private void CreateSequenceTransform(TriangleMesh[] P, TriangleMesh[] Q, Point3D[][] Qn, out Point3D toCenter, out Transform3D2 rot, out List<Point3D> sampled_P, out List<Point3D> sampled_Q)
        {
            toCenter = InitializeArrays(P, Q, out sampled_P, out sampled_Q);
            rot = findRotationArrays(sampled_Q, Qn);
        }

        // Initializes the data structures and needed values before the match
        Point3D InitializeArrays(TriangleMesh[] P, TriangleMesh[] Q, out List<Point3D> sampled_P, out List<Point3D> sampled_Q)
        {

            // fill [][]s with vetices
            Point3D[][] pointsQ = new Point3D[Q.Length][];
            Point3D[][] pointsP = new Point3D[P.Length][];
            int numberOfPointsP = 0;
            for (int i = 0; i < Q.Length; i++)
            {
                pointsQ[i] = Q[i].Points;
                pointsP[i] = P[i].Points;
                numberOfPointsP += pointsP[i].Length;
            }

            Q[0].radius = Q[0].Radius();
            qRadius = Q[0].radius;

            // logging
            Logger.Log("Mesh radius: {0}", qRadius);
            if (auto_delta)
                delta = qRadius * config.auto_delta_ratio;
            Logger.Log("Delta: {0}", delta);

            Random rand;
            if (config.deterministic)
                rand = new Random(config.seed);
            else
                rand = new Random();


            sampled_P = new List<Point3D>();
            sampled_Q = new List<Point3D>();


            // uniform sampling
            List<int[]> uniform_P = DistUniformSampling(pointsP, numberOfPointsP, delta / 3);
            List<int[]> uniform_Q = new List<int[]>();

            for (int j = 0; j < pointsQ.Length; j++)
            {
                for (int i = 0; i < pointsQ[j].Length; i++)
                {
                    uniform_Q.Add(new int[] { j, i });
                }
            }

            int sample_fraction_P = 1;
            if (sample_P)
                sample_fraction_P = Math.Max(1, (int)(uniform_P.Count / config.sample_size_P));

            // Sample the sets P and Q uniformly.
            for (int i = 0; i < uniform_P.Count; ++i)
            {
                if (rand.Next(sample_fraction_P) == 0)
                {
                    sampled_P.Add(pointsP[uniform_P[i][0]][uniform_P[i][1]]);
                }
            }

            Point3D toCenter = new Point3D(0, 0, 0);

            int sample_fraction_Q = Math.Max(1, (int)(uniform_Q.Count / config.sample_size_Q));
            for (int i = 0; i < uniform_Q.Count; ++i)
            {
                if (rand.Next(sample_fraction_Q) == 0)
                {
                    sampled_Q.Add(pointsQ[uniform_Q[i][0]][uniform_Q[i][1]]);
                    toCenter += pointsQ[uniform_Q[i][0]][uniform_Q[i][1]];
                }
            }

            toCenter *= 1.0 / sampled_Q.Count;

            for (int i = 0; i < sampled_Q.Count; i++)
                sampled_Q[i] -= toCenter;

            return (-toCenter);
        } //InitializeArrays
        List<int[]> DistUniformSampling(Point3D[][] set, int num_input, double delta)
        {
            List<int[]> sample = new List<int[]>();
            Framework.HashTable hash = new Framework.HashTable(num_input, delta);

            int index = 0;
            for (int j = 0; j < set.Length; j++)
            {
                for (int i = 0; i < set[j].Length; i++)
                {
                    int ind = hash[set[j][i]];
                    if (ind < 0)
                    {
                        sample.Add(new int[] { j, i });
                        hash.set(sample.Count);
                    }
                    index++;
                }
            }

            return (sample);
        }

        private Transform3D2 findRotationArrays(List<Point3D> q, Point3D[][] Qn)
        {
            MathNet.Numerics.LinearAlgebra.Matrix<double> m = MathNet.Numerics.LinearAlgebra.Matrix<double>.Build.Dense(3, 3);
            for (int i = 0; i < q.Count; i++)
            {
                m[0, 0] += q[i].X * q[i].X;
                m[0, 1] += q[i].X * q[i].Y;
                m[0, 2] += q[i].X * q[i].Z;
                m[1, 1] += q[i].Y * q[i].Y;
                m[1, 2] += q[i].Y * q[i].Z;
                m[2, 2] += q[i].Z * q[i].Z;
            }
            m[1, 0] = m[0, 1];
            m[2, 0] = m[0, 2];
            m[2, 1] = m[1, 2];

            MathNet.Numerics.LinearAlgebra.Factorization.Evd<double> evd = m.Evd();
            Transform3D2 result = new Transform3D2();
            result.SetIdentity();

            Point3D rX = new Point3D(evd.EigenVectors[0, 0], evd.EigenVectors[0, 1], evd.EigenVectors[0, 2]);
            Point3D rY = new Point3D(evd.EigenVectors[1, 0], evd.EigenVectors[1, 1], evd.EigenVectors[1, 2]);
            Point3D rZ = rX.CrossProduct(rY);

            result.matrix[0, 0] = rX.X;
            result.matrix[1, 0] = rX.Y;
            result.matrix[2, 0] = rX.Z;

            result.matrix[0, 1] = rY.X;
            result.matrix[1, 1] = rY.Y;
            result.matrix[2, 1] = rY.Z;

            result.matrix[0, 2] = rZ.X;
            result.matrix[1, 2] = rZ.Y;
            result.matrix[2, 2] = rZ.Z;

            List<Point3D> trPoints = new List<Point3D>();
            for (int i = 0; i < q.Count; i++)
            {
                Point3D p = q[i];
                trPoints.Add(result.TransformNoW(ref p));
            }
            q.Clear();
            q.AddRange(trPoints);

            for (int j = 0; j < Qn.Length; j++)
            {
                for (int i = 0; i < Qn[j].Length; i++)
                {
                    Point3D oqn = Qn[j][i];
                    Point3D tqn = result.TransformNoW(ref oqn);
                    Qn[j][i] = tqn;
                }
            }

            return result;
        }//findRotationArrays


        private Random[] CreateGenerators(RegistrationConfig config)
        {
            Random[] generators = null;

            if (config.deterministic)
            {
                generators = new Random[config.threadCount < 0 ? Environment.ProcessorCount : config.threadCount];

                for (int i = 0; i < generators.Length; i++)
                {
                    generators[i] = new Random(config.seed);
                }
            }
            else
            {
                Random r = new Random();

                generators = new Random[config.threadCount < 0 ? Environment.ProcessorCount : config.threadCount];
                for (int i = 0; i < generators.Length; i++)
                    generators[i] = new Random(r.Next());
            }

            return generators;
        }

        private Candidate[] uniqueCandidates(Candidate[] candidates)
        {
            HashSet<Candidate> unique = new HashSet<Candidate>();
            foreach (Candidate c in candidates)
                unique.Add(c);
            Candidate[] result = new Candidate[unique.Count];
            int i = 0;
            foreach (Candidate c in unique)
            {
                result[i] = c;
                i++;
            }
            return result;
        }
    }
}
