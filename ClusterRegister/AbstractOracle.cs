using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Framework;

namespace ClusterRegister
{
    public abstract class AbstractOracle
    {
        public bool SavePairs { get; set; } = true;

        public bool Mute { get; set; }
        public ElapsedTime Time { get; internal set; }

        public abstract Candidate[] findCandidates();
    }
}
