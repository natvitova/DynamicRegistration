using System;
using System.Collections.Generic;
using System.Text;

namespace Framework
{
    public struct Triangle
    {
        public int V1;
        public int V2;
        public int V3;

        /// <summary>
        /// Triangle vertex indexer. Try to avoid the indexer due to performance hit.
        /// </summary>
        /// <param name="i">Triangle vertex</param>
        /// <returns>Index of vertex or -1</returns>        
        public int this[int i]
        {
            get
            {
                switch (i)
                {
                    case 0: return V1;
                    case 1: return V2;
                    case 2: return V3;
                    default: break;
                }
                return -1;
            }
        }

        public Triangle(int v1, int v2, int v3)
        {
            V1 = v1;
            V2 = v2;
            V3 = v3;
        }

        public override int GetHashCode()
        {
            return V1 + V2 + V3;
        }

        public override bool Equals(object obj)
        {
            Triangle t = (Triangle)obj;
            if ((this.V1 != t.V1) && (this.V1 != t.V2) && (this.V1 != t.V3))
                return false;
            if ((this.V2 != t.V1) && (this.V2 != t.V2) && (this.V2 != t.V3))
                return false;
            if ((this.V3 != t.V1) && (this.V3 != t.V2) && (this.V3 != t.V3))
                return false;
            return true;
        }

        public bool containsVertex(int v)
        {
            if (V1 == v)
                return true;
            if (V2 == v)
                return true;
            if (V3 == v)
                return true;
            return false;
        }
    }    
}
