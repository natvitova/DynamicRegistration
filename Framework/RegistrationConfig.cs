using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Framework
{
    public class RegistrationConfig
    {
        public double auto_delta_ratio = 0.01;
        public double acceptanceThreshold = 0.03;
        public int nbHoodWidth = 8;
        public int sample_size_Q = 400;
        public int sample_size_P = 2000;
        public int kMeansStepCount = 10;
        public int clusterCount = 300;
        public int populationSize = 15000;
        public int PSampleCount = 15000;
        public double spread = 10;
        public int threadCount = -1;
        public double curvatureRatio = 0.7;
        public int seed = 0;
        public int clusteringMethod = 0;
        public int verificationMethod = 0;

        // use random seed for clustering or use config.seed instead
        public bool deterministic;

        public bool useDensity = true;
        public double radius;

        // debug clustering
        public double randomChoice;
        public double facilityfacCostMult;

        // Gaussian - weights in curvature computation
        public const double radiusSearch = 100;
        public const double valueAtRadius = 0.001;


        public String toString()
        {
            String ret = "";

            foreach (var prop in this.GetType().GetProperties())
            {
                ret += prop.Name + " \t = " + prop.GetValue(this, null) +"\n" ;
            }
            return ret;
        }
    }
}
