using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Framework;

namespace ClusterRegister
{
    // KD tree for searching for nearest neighbour
    public class KDTree2D
    {
        class Node
        {
            public Node left, right;
            public int point;
            public int axis;
            public double value;

            public bool Contains(int p)
            {
                if (point == p)
                    return (true);
                if (left != null)
                    if (left.Contains(p))
                        return (true);
                if (right != null)
                    if (right.Contains(p))
                        return (true);
                return (false);
            }
        }

        // search context, allows multiple threads to perform the search, using the same data structure.
        class SearchContext
        {
            public double smDist, smDistSq;
            public int nearest;
            public PointRecord p;                                            
        }

        Node root;

        PointRecord[] pnts;

        Random rnd = new Random(0);

        public KDTree2D(PointRecord[] points)
        {
            this.pnts = points;
            List<int> list = new List<int>();
            for (int i = 0; i < points.Length; i++)
                list.Add(i);            
            root = new Node();
            addChildrenX(root, list, 0);
        }

        public int findNearest(PointRecord p, out double dist)
        {
            SearchContext sc = new SearchContext();
            sc.smDist = double.MaxValue;
            sc.smDistSq = double.MaxValue;
            sc.nearest = -1;
            sc.p = p;
            searchSubtreeX(root, sc);
            dist = sc.smDist;
            return (sc.nearest);
        }

        void searchSubtreeX(Node n, SearchContext sc)
        {
            double dif = sc.p.k1 - n.value;

            if (dif<0)
            {
                if ((-dif) < sc.smDist)
                {
                    double dist = sc.p.SquareDistanceTo(pnts[n.point]);
                    if (dist < sc.smDistSq)
                    {
                        sc.smDistSq = dist;
                        sc.smDist = Math.Sqrt(dist);
                        sc.nearest = n.point;
                    }
                }
                if (n.left != null)
                {
                    searchSubtreeY(n.left, sc);
                }
                if (n.right != null)
                {
                    if ((-dif) < sc.smDist)
                        searchSubtreeY(n.right, sc);
                }
            }
            else
            {
                if (dif < sc.smDist)
                {
                    double dist = sc.p.SquareDistanceTo(pnts[n.point]);
                    if (dist < sc.smDistSq)
                    {
                        sc.smDist = Math.Sqrt(dist);
                        sc.smDistSq = dist;
                        sc.nearest = n.point;
                    }
                }
                if (n.right != null)
                {
                    searchSubtreeY(n.right, sc);
                }
                if (n.left != null)
                {
                    if (dif < sc.smDist)
                        searchSubtreeY(n.left, sc);
                }
            }
        }

        void searchSubtreeY(Node n, SearchContext sc)
        {
            double dif = sc.p.k2 - n.value;
            if (dif<0)
            {
                if ((-dif) < sc.smDist)
                {
                    double dist = sc.p.SquareDistanceTo(pnts[n.point]);
                    if (dist < sc.smDistSq)
                    {
                        sc.smDistSq = dist;
                        sc.smDist = Math.Sqrt(dist);
                        sc.nearest = n.point;
                    }
                }
                if (n.left != null)
                {
                    searchSubtreeX(n.left, sc);
                }
                if (n.right != null)
                {
                    if ((-dif) < sc.smDist)
                        searchSubtreeX(n.right, sc);
                }
            }
            else
            {
                if ((dif) < sc.smDist)
                {
                    double dist = sc.p.SquareDistanceTo(pnts[n.point]);
                    if (dist < sc.smDistSq)
                    {
                        sc.smDist = Math.Sqrt(dist);
                        sc.smDistSq = dist;
                        sc.nearest = n.point;
                    }
                }
                if (n.right != null)
                {
                    searchSubtreeX(n.right, sc);
                }
                if (n.left != null)
                {
                    if (dif < sc.smDist)
                        searchSubtreeX(n.left, sc);
                }
            }
        }

        private void addChildrenX(Node n, List<int> list, int depth)
        {
            n.axis = 0;

            if (list.Count == 1)
            {
                n.point = list[0];
                n.value = pnts[list[0]].k1;
                return;
            }

            int med = median(list, 0);
            n.point = list[med];

            double medVal = pnts[list[med]].k1;
            n.value = medVal;

            List<int> left = new List<int>();
            List<int> right = new List<int>();

            for (int i = 0; i < list.Count / 2; i++)
                if (i != med)
                    left.Add(list[i]);

            for (int i = list.Count / 2; i < list.Count; i++)
                if (i != med)
                    right.Add(list[i]);


            if (left.Count > 0)
            {
                n.left = new Node();
                addChildrenY(n.left, left, depth + 1);
            }
            if (right.Count > 0)
            {
                n.right = new Node();
                addChildrenY(n.right, right, depth + 1);
            }
        }

        private void addChildrenY(Node n, List<int> list, int depth)
        {
            n.axis = 1;

            if (list.Count == 1)
            {
                n.point = list[0];
                n.value = pnts[list[0]].k2;
                return;
            }

            int med = median(list, 1);
            n.point = list[med];

            double medVal = pnts[list[med]].k2;
            n.value = medVal;

            List<int> left = new List<int>();
            List<int> right = new List<int>();

            for (int i = 0; i < list.Count / 2; i++)
                if (i != med)
                    left.Add(list[i]);

            for (int i = list.Count / 2; i < list.Count; i++)
                if (i != med)
                    right.Add(list[i]);


            if (left.Count > 0)
            {
                n.left = new Node();
                addChildrenX(n.left, left, depth + 1);
            }
            if (right.Count > 0)
            {
                n.right = new Node();
                addChildrenX(n.right, right, depth + 1);
            }
        }

        public int median(List<int> points, int axis)
        {
            if (axis == 0)
                return (findMedianX(points, 0, points.Count - 1, 0));
            else
                return (findMedianY(points, 0, points.Count - 1, 0));
        }



        private int findMedianX(List<int> points, int min, int max, int depth)
        {
            if (depth > 35) // this usually happens if all the values are the same.
            {
                bool allSame = true;
                for (int i = min + 1; i <= max; i++)
                    if (pnts[points[i]].k1 != pnts[points[min]].k1)
                        allSame = false;
                if (allSame)
                    return (min);
            }
            if (min == max)
                return (min);
            if (min == (max - 1))
            {
                if (pnts[points[min]].k1 > pnts[points[max]].k1)
                {
                    int tmp = points[min];
                    points[min] = points[max];
                    points[max] = tmp;
                }
                return (points.Count / 2);
            }
            int pivot = min + rnd.Next(max - min + 1);
            double pivotVal = pnts[points[pivot]].k1;
            int l = min;
            int r = max;
            while (l < r)
            {
                while (pnts[points[l]].k1 < pivotVal)
                    l++;
                while (pnts[points[r]].k1 >= pivotVal)
                {
                    r--;
                    if (r < 0)
                        break;
                }
                if (l < r)
                {
                    int tmp = points[l];
                    points[l] = points[r];
                    points[r] = tmp;
                }
            }
            if (r < (points.Count / 2))
                return (findMedianX(points, l, max, depth + 1));
            else
                return (findMedianX(points, min, r, depth + 1));
        }

        private int findMedianY(List<int> points, int min, int max, int depth)
        {
            if (depth > 35) // this usually happens if all the values are the same.
            {
                bool allSame = true;
                for (int i = min + 1; i <= max; i++)
                    if (pnts[points[i]].k2 != pnts[points[min]].k2)
                        allSame = false;
                if (allSame)
                    return (min);
            }
            if (min == max)
                return (min);
            if (min == (max - 1))
            {
                if (pnts[points[min]].k2 > pnts[points[max]].k2)
                {
                    int tmp = points[min];
                    points[min] = points[max];
                    points[max] = tmp;
                }
                return (points.Count / 2);
            }
            int pivot = min + rnd.Next(max - min + 1);
            double pivotVal = pnts[points[pivot]].k2;
            int l = min;
            int r = max;
            while (l < r)
            {
                while (pnts[points[l]].k2 < pivotVal)
                    l++;
                while (pnts[points[r]].k2 >= pivotVal)
                {
                    r--;
                    if (r < 0)
                        break;
                }
                if (l < r)
                {
                    int tmp = points[l];
                    points[l] = points[r];
                    points[r] = tmp;
                }
            }
            if (r < (points.Count / 2))
                return (findMedianY(points, l, max, depth + 1));
            else
                return (findMedianY(points, min, r, depth + 1));
        }
    }    
}
