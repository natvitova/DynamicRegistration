using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;

namespace ClusterRegister
{
    class DTNode
    {
        int node;
        DTNode close;
        DTNode far;
        double threshold;
        List<int> items;
        bool leaf;
        public static Candidate[] cands;


        public DTNode(int n, List<int> children, int depth)
        {
            if (children.Count < 5)
            {
                this.leaf = true;
                this.items = new List<int>();
                items.AddRange(children);
                items.Add(n);
            }
            else
            {
                this.node = n;
                this.leaf = false;
                this.threshold = findMedian(node, children);
                List<int> c = new List<int>();
                List<int> f = new List<int>();
                foreach (int i in children)
                {
                    if (cands[node].SqrtDistanceTo(cands[i]) >= threshold)
                        f.Add(i);
                    else
                        c.Add(i);
                }
                if (c.Count != 0)
                {
                    int cn = c[c.Count - 1];
                    c.RemoveAt(c.Count - 1);
                    this.close = new DTNode(cn, c, depth + 1);
                }
                int fn = f[f.Count - 1];
                f.RemoveAt(f.Count - 1);
                this.far = new DTNode(fn, f, depth + 1);
            }
        }

        public bool contains(int i)
        {
            if (leaf)
            {
                return items.Contains(i);
            }
            else
            {
                if (node == i)
                    return true;
                if (close != null)
                    if (close.contains(i))
                        return true;
                if (far.contains(i))
                    return true;
                return false;
            }
        }

        private double findMedian(int node, List<int> children)
        {
            double[] distances = new double[children.Count];
            for (int i = 0; i < distances.Length; i++)
                distances[i] = cands[node].SqrtDistanceTo(cands[children[i]]);
            quickSelect(distances, 0, distances.Length - 1, distances.Length / 2);
            return distances[distances.Length / 2];
        }

        private void quickSelect(double[] d, int l, int r, int s)
        {
            int t = split(d, l, r);
            if (t == s)
                return;
            if (t > s)
                quickSelect(d, l, t - 1, s);
            else
                quickSelect(d, t + 1, r, s);
        }

        private int split(double[] d, int l, int r)
        {
            double pivot = d[r];
            while (true)
            {
                while ((d[l] < pivot) && (l < r))
                    l++;
                if (l < r)
                {
                    d[r] = d[l];
                    r--;
                }
                else break;
                while ((d[r] >= pivot) && (l < r))
                    r--;
                if (l < r)
                {
                    d[l] = d[r];
                    l++;
                }
                else break;
            }
            d[l] = pivot;
            return l;
        }

        internal void findAllCloserThan(Candidate query, double dist, List<int> result)
        {
            if (leaf)
            {
                foreach (int i in items)
                {
                    double d = query.SqrtDistanceTo(cands[i]);
                    if (d < dist)
                        result.Add(i);
                }
            }
            else
            {
                double d = query.SqrtDistanceTo(cands[node]);
                if (d < dist)
                    result.Add(node);
                if (d <= (threshold + dist))
                    if (close != null)
                        close.findAllCloserThan(query, dist, result);
                if (d >= (threshold - dist))
                    far.findAllCloserThan(query, dist, result);
            }
        }
    }
   
    class DistanceTree
    {
        DTNode root;
        Candidate[] cands;
        public DistanceTree(Candidate[] candidates)
        {
            cands = candidates;
            DTNode.cands = candidates;
            List<int> children = new List<int>();
            for (int i = 1; i < candidates.Length; i++)
                children.Add(i);
            root = new DTNode(0, children, 0);
        }

        public List<int> findAllCloserThan(Candidate query, double dist)
        {
            List<int> result = new List<int>();
            root.findAllCloserThan(query, dist, result);
            return result;
        }

    }
}
