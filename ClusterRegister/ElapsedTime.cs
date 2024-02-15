using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ClusterRegister
{
    public class ElapsedTime
    {
        public static string TOTAL_TIME = "TotalTime";
        public static string CLUSTERING = "ClusteringTime";
        public static string CANDIDATES = "Candidates";
        public static string SAMPLING = "Sampling";
        public static string INITIALIZATION = "Initialization";

        public Dictionary <string, double> elapsedTime { get; set; }

        public ElapsedTime()
        {
            elapsedTime = new Dictionary<string, double>();
        }

        public void LogTime(string log, double time)
        {
            elapsedTime[log] = time;
        }

        public Dictionary<string, double> ElapseTime
        {
            get { return elapsedTime; }
        }
    }
}
