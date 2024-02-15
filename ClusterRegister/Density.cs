using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;

namespace ClusterRegister
{
    class Density : ISelector
    {
        public int ComputeClustering(Candidate[] points, RegistrationConfig config)
        {
            //throw new NotImplementedException();
            return 0;
        }

        DistanceTree dt;

        public DQuat FindDualQuatCandidate(Candidate[] candidate, RegistrationConfig config, int indexBestCandidate, out int clusterIndex)
        {
            int maxDensityIndex = -1;
                dt = new DistanceTree(candidate);
                double maxDensity = 0;
                maxDensityIndex = -1;
                double t = -config.radius * Math.Log(0.01) / config.spread;
                object[] teInput = new object[candidate.Length / 100];
                for (int i = 0; i < teInput.Length; i++)
                    teInput[i] = i * 100;
                ThreadedExecution<object, double[]> te = new ThreadedExecution<object, double[]>(teInput, new ThreadedExecution<object, double[]>.ExecBody(this.density), -1, new object[] { candidate, t, config });
                double[][] density = te.Execute();
                for (int i = 0; i < teInput.Length; i++)
                {
                    for (int j = 0; j < 100; j++)
                        if (density[i][j] > maxDensity)
                        {
                            maxDensity = density[i][j];
                            maxDensityIndex = i * 100 + j;
                        }
                }
            clusterIndex = 0;
            return candidate[maxDensityIndex].dq;
        }

        private double[] density(object input, int threadId, object[] parameters)
        {
            double[] result = new double[100];
            Candidate[] candidate = (Candidate[])parameters[0];            
            double t = (double)parameters[1];
            RegistrationConfig config = (RegistrationConfig)parameters[2];
            for (int i = 0; i < 100; i++)
            {
                int idx = (int)input + i;
                if (idx == candidate.Length)
                    break;
                var nbs = dt.findAllCloserThan(candidate[idx], t);
                double density = 0;
                foreach (int n in nbs)
                    density += Math.Exp(-config.spread * candidate[idx].SqrtDistanceTo(candidate[n]) / config.radius);
                result[i] = density;
            }
            return result;
        }

        public DQuat[] FindDualQuatCandidates(Candidate[] candidate, RegistrationConfig config, int indexBestCandidate, out int[] clusterIndexies)
        {
            //throw new NotImplementedException();
            clusterIndexies = null;
            return null;
        }

        public string GetInfo()
        {
            //throw new NotImplementedException();
            return "Density";
        }

        public void SetProperties(Dictionary<string, object> properties)
        {
            //throw new NotImplementedException();            
        }
    }
}
