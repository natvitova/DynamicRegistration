using System;
using System.Collections.Generic;

namespace Framework
{
    // KD tree for searching for nearest neighbour
    public class KDTree
    {
        Node root;

        Point3D[] pnts;

        Random rnd = new Random(0);

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
            public Point3D p;
            public double lim;            
            public List<int> result;
        }

        class SearchContextBoolean
        {
            public double smDist, smDistSq;
            public int nearest;
            public Point3D p;
            public double lim;
            public bool found = false;
        }

        public KDTree(Point3D[] points)
        {
            this.pnts = points;
            List<int> list = new List<int>();
            for (int i = 0; i < points.Length; i++)
                list.Add(i);            
            root = new Node();
            addChildrenX(root, list, 0);
        }

        public int findNearest(Point3D p, out double dist)
        {
            SearchContextBoolean sc = new SearchContextBoolean();
            sc.smDist = double.MaxValue;
            sc.smDistSq = double.MaxValue;
            sc.nearest = -1;
            sc.p = p;
            searchSubtreeX(root, sc);
            dist = sc.smDist;
            return (sc.nearest);
        }

        // finds a point that is closer to given point than lim. Ends if such point is found,or if it is certain that it does not exist. The resulting point is not necessarily the nearest point!
        public bool findNearestLim(Point3D p, double lim)
        {
            SearchContextBoolean sc = new SearchContextBoolean();
            sc.smDist = double.MaxValue;
            sc.smDistSq = double.MaxValue;
            sc.nearest = -1;
            sc.p = p;
            sc.lim = lim;
            searchSubtreeXLim(root, sc);
            return (sc.found);
        }

        public List<int> findAllCloserThan(Point3D p, double lim)
        {
            SearchContext sc = new SearchContext();
            sc.result = new List<int>();
            sc.p = p;
            sc.lim = lim;
            searchSubtreeXLim(root, sc);
            return (sc.result);
        }

        void searchSubtreeX(Node n, SearchContextBoolean sc)
        {
            double dif = sc.p.X - n.value;

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

        void searchSubtreeY(Node n, SearchContextBoolean sc)
        {
            double dif = sc.p.Y - n.value;
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
                    searchSubtreeZ(n.left, sc);
                }
                if (n.right != null)
                {
                    if ((-dif) < sc.smDist)
                        searchSubtreeZ(n.right, sc);
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
                    searchSubtreeZ(n.right, sc);
                }
                if (n.left != null)
                {
                    if (dif < sc.smDist)
                        searchSubtreeZ(n.left, sc);
                }
            }
        }

        void searchSubtreeZ(Node n, SearchContextBoolean sc)
        {
            double dif = sc.p.Z - n.value;
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
                    searchSubtreeX(n.right, sc);
                }
                if (n.left != null)
                {
                    if (dif < sc.smDist)
                        searchSubtreeX(n.left, sc);
                }
            }
        }

        void searchSubtreeXLim(Node n, SearchContextBoolean sc)
        {
            if (sc.found)
                return;

            double dif = sc.p.X - n.value;

            if (dif < 0)
            {
                if ((-dif) < sc.smDist)
                {
                    double dist = sc.p.SquareDistanceTo(pnts[n.point]);
                    if (dist < sc.smDistSq)
                    {
                        sc.smDistSq = dist;
                        sc.smDist = Math.Sqrt(dist);
                        sc.nearest = n.point;
                        if (sc.smDist < sc.lim)
                        {
                            sc.found = true;
                            return;
                        }
                    }
                }
                if (n.left != null)
                    searchSubtreeYLim(n.left, sc);
                if (n.right != null)
                {
                    if ((-dif) < sc.smDist)
                        if ((-dif)<sc.lim)
                            searchSubtreeYLim(n.right, sc);
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
                        if (sc.smDist < sc.lim)
                        {
                            sc.found = true;
                            return;
                        }
                    }
                }
                if (n.right != null)
                {
                    searchSubtreeYLim(n.right, sc);
                }
                if (n.left != null)
                {
                    if (dif < sc.smDist)
                        if ((dif) < sc.lim)
                            searchSubtreeYLim(n.left, sc);
                }
            }
        }

        void searchSubtreeYLim(Node n, SearchContextBoolean sc)
        {
            if (sc.found)
                return;
            double dif = sc.p.Y - n.value;
            if (dif < 0)
            {
                if ((-dif) < sc.smDist)
                {
                    double dist = sc.p.SquareDistanceTo(pnts[n.point]);
                    if (dist < sc.smDistSq)
                    {
                        sc.smDistSq = dist;
                        sc.smDist = Math.Sqrt(dist);
                        sc.nearest = n.point;
                        if (sc.smDist < sc.lim)
                        {
                            sc.found = true;
                            return;
                        }
                    }
                }
                if (n.left != null)
                {
                    searchSubtreeZLim(n.left, sc);
                }
                if (n.right != null)
                {
                    if ((-dif) < sc.smDist)
                        if ((-dif) < sc.lim)
                            searchSubtreeZLim(n.right, sc);
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
                        if (sc.smDist < sc.lim)
                        {
                            sc.found = true;
                            return;
                        }
                    }
                }
                if (n.right != null)
                {
                    searchSubtreeZLim(n.right, sc);
                }
                if (n.left != null)
                {
                    if (dif < sc.smDist)
                        if ((dif) < sc.lim)
                            searchSubtreeZLim(n.left, sc);
                }
            }
        }

        void searchSubtreeZLim(Node n, SearchContextBoolean sc)
        {
            if (sc.found)
                return;
            double dif = sc.p.Z - n.value;
            if (dif < 0)
            {
                if ((-dif) < sc.smDist)
                {
                    double dist = sc.p.SquareDistanceTo(pnts[n.point]);
                    if (dist < sc.smDistSq)
                    {
                        sc.smDistSq = dist;
                        sc.smDist = Math.Sqrt(dist);
                        sc.nearest = n.point;
                        if (sc.smDist < sc.lim)
                        {
                            sc.found = true;
                            return;
                        }
                    }
                }
                if (n.left != null)
                {
                    searchSubtreeXLim(n.left, sc);
                }
                if (n.right != null)
                {
                    if ((-dif) < sc.smDist)
                        if ((-dif) < sc.lim)
                            searchSubtreeXLim(n.right, sc);
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
                        if (sc.smDist < sc.lim)
                        {
                            sc.found = true;
                            return;
                        }
                    }
                }
                if (n.right != null)
                {
                    searchSubtreeXLim(n.right, sc);
                }
                if (n.left != null)
                {
                    if (dif < sc.smDist)
                        if ((dif) < sc.lim)
                            searchSubtreeXLim(n.left, sc);
                }
            }
        }

        void searchSubtreeXLim(Node n, SearchContext sc)
        {            
            double dif = sc.p.X - n.value;

            if (dif < 0)
            {
                if ((-dif) < sc.lim)
                {
                    double dist = sc.p.SquareDistanceTo(pnts[n.point]);
                    if (dist < sc.lim*sc.lim)
                    {
                        sc.result.Add(n.point);
                    }
                }
                if (n.left != null)
                    searchSubtreeYLim(n.left, sc);
                if (n.right != null)
                {                    
                    if ((-dif) < sc.lim)
                        searchSubtreeYLim(n.right, sc);
                }
            }
            else
            {
                if (dif < sc.lim)
                {
                    double dist = sc.p.SquareDistanceTo(pnts[n.point]);
                    if (dist < sc.lim*sc.lim)
                    {
                        sc.result.Add(n.point);
                    }
                }
                if (n.right != null)
                {
                    searchSubtreeYLim(n.right, sc);
                }
                if (n.left != null)
                {                    
                    if ((dif) < sc.lim)
                        searchSubtreeYLim(n.left, sc);
                }
            }
        }

        void searchSubtreeYLim(Node n, SearchContext sc)
        {            
            double dif = sc.p.Y - n.value;
            if (dif < 0)
            {
                if ((-dif) < sc.lim)
                {
                    double dist = sc.p.SquareDistanceTo(pnts[n.point]);
                    if (dist < sc.lim*sc.lim)
                    {
                        sc.result.Add(n.point);
                    }
                }
                if (n.left != null)
                {
                    searchSubtreeZLim(n.left, sc);
                }
                if (n.right != null)
                {
                    if ((-dif) < sc.lim)
                        searchSubtreeZLim(n.right, sc);
                }
            }
            else
            {
                if ((dif) < sc.lim)
                {
                    double dist = sc.p.SquareDistanceTo(pnts[n.point]);
                    if (dist < sc.lim*sc.lim)
                    {
                        sc.result.Add(n.point);
                    }
                }
                if (n.right != null)
                {
                    searchSubtreeZLim(n.right, sc);
                }
                if (n.left != null)
                {
                    if ((dif) < sc.lim)
                        searchSubtreeZLim(n.left, sc);
                }
            }
        }

        void searchSubtreeZLim(Node n, SearchContext sc)
        {
            double dif = sc.p.Z - n.value;
            if (dif < 0)
            {
                if ((-dif) < sc.lim)
                {
                    double dist = sc.p.SquareDistanceTo(pnts[n.point]);
                    if (dist < sc.lim*sc.lim)
                    {
                        sc.result.Add(n.point);
                    }
                }
                if (n.left != null)
                {
                    searchSubtreeXLim(n.left, sc);
                }
                if (n.right != null)
                {
                    if ((-dif) < sc.lim)
                        searchSubtreeXLim(n.right, sc);
                }
            }
            else
            {
                if (dif < sc.lim)
                {
                    double dist = sc.p.SquareDistanceTo(pnts[n.point]);
                    if (dist < sc.lim*sc.lim)
                    {
                        sc.result.Add(n.point);
                    }
                }
                if (n.right != null)
                {
                    searchSubtreeXLim(n.right, sc);
                }
                if (n.left != null)
                {
                    if ((dif) < sc.lim)
                        searchSubtreeXLim(n.left, sc);
                }
            }
        }

        private void addChildrenX(Node n, List<int> list, int depth)
        {
            n.axis = 0;

            if (list.Count == 1)
            {
                n.point = list[0];
                n.value = pnts[list[0]].X;
                return;
            }

            int med = median(list, 0);
            n.point = list[med];

            double medVal = pnts[list[med]].X;
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
                n.value = pnts[list[0]].Y;
                return;
            }

            int med = median(list, 1);
            n.point = list[med];

            double medVal = pnts[list[med]].Y;
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
                addChildrenZ(n.left, left, depth + 1);
            }
            if (right.Count > 0)
            {
                n.right = new Node();
                addChildrenZ(n.right, right, depth + 1);
            }
        }

        private void addChildrenZ(Node n, List<int> list, int depth)
        {
            n.axis = 2;

            if (list.Count == 1)
            {
                n.point = list[0];
                n.value = pnts[list[0]].Z;
                return;
            }

            int med = median(list, 2);
            n.point = list[med];

            double medVal = pnts[list[med]].Z;
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
            else if (axis == 1)
                return (findMedianY(points, 0, points.Count - 1, 0));
            else
                return (findMedianZ(points, 0, points.Count - 1, 0));
        }

        private int findMedianX(List<int> points, int min, int max, int depth)
        {
            if (depth > 35) // this usually happens if all the values are the same.
            {
                bool allSame = true;
                for (int i = min + 1; i <= max; i++)
                    if (pnts[points[i]].X != pnts[points[min]].X)
                        allSame = false;
                if (allSame)
                    return (min);
            }
            if (min == max)
                return (min);
            if (min == (max - 1))
            {
                if (pnts[points[min]].X > pnts[points[max]].X)
                {
                    int tmp = points[min];
                    points[min] = points[max];
                    points[max] = tmp;
                }
                return (points.Count / 2);
            }
            int pivot = min + rnd.Next(max - min + 1);
            double pivotVal = pnts[points[pivot]].X;
            int l = min;
            int r = max;
            while (l < r)
            {
                while (pnts[points[l]].X < pivotVal)
                    l++;
                while (pnts[points[r]].X >= pivotVal)
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
                    if (pnts[points[i]].Y != pnts[points[min]].Y)
                        allSame = false;
                if (allSame)
                    return (min);
            }
            if (min == max)
                return (min);
            if (min == (max - 1))
            {
                if (pnts[points[min]].Y > pnts[points[max]].Y)
                {
                    int tmp = points[min];
                    points[min] = points[max];
                    points[max] = tmp;
                }
                return (points.Count / 2);
            }
            int pivot = min + rnd.Next(max - min + 1);
            double pivotVal = pnts[points[pivot]].Y;
            int l = min;
            int r = max;
            while (l < r)
            {
                while (pnts[points[l]].Y < pivotVal)
                    l++;
                while (pnts[points[r]].Y >= pivotVal)
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

        private int findMedianZ(List<int> points, int min, int max, int depth)
        {
            if (depth > 35) // this usually happens if all the values are the same.
            {
                bool allSame = true;
                for (int i = min + 1; i <= max; i++)
                    if (pnts[points[i]].Z != pnts[points[min]].Z)
                        allSame = false;
                if (allSame)
                    return (min);
            }
            if (min == max)
                return (min);
            if (min == (max - 1))
            {
                if (pnts[points[min]].Z > pnts[points[max]].Z)
                {
                    int tmp = points[min];
                    points[min] = points[max];
                    points[max] = tmp;
                }
                return (points.Count / 2);
            }
            int pivot = min + rnd.Next(max - min + 1);
            double pivotVal = pnts[points[pivot]].Z;
            int l = min;
            int r = max;
            while (l < r)
            {
                while (pnts[points[l]].Z < pivotVal)
                    l++;
                while (pnts[points[r]].Z >= pivotVal)
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
                return (findMedianZ(points, l, max, depth + 1));
            else
                return (findMedianZ(points, min, r, depth + 1));
        }
    }    
}
