using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework
{
    public class CornerTable
    {
        TriangleMesh mesh;

        // these two fields represent the Corner table data structure
        public int[] opposite;
        private int[] incidentCorner;

        // edge data structure
        struct Edge
        {
            // start and end vertex
            public int v1, v2;

            // hash code - needed for the dictionary
            public override int GetHashCode()
            {
                return (v1 + 10000 * v2);
            }

            //constructor - simplicity incarnate.
            public Edge(int v1, int v2)
            {
                this.v1 = v1;
                this.v2 = v2;
            }

            // equality operator. Edges are equal, when start and end point coiincide. 
            // Edge cannot be equal to anyhing than an edge
            public override bool Equals(object obj)
            {
                if (obj is Edge)
                {
                    Edge other = (Edge)obj;
                    if ((this.v1 == other.v1) && (this.v2 == other.v2))
                        return (true);
                    else
                        return (false);
                }
                else
                    return (false);
            }
        }

        public CornerTable(TriangleMesh mesh)
        {
            this.mesh = mesh;
            initCornerTable();
        }

        // returns next corner
        public int n(int c)
        {
            if (c % 3 == 2)
                return (c - 2);
            else
                return (c + 1);
        }

        // returns previous corner
        public int p(int c)
        {
            if (c % 3 == 0)
                return (c + 2);
            else
                return (c - 1);
        }

        // returns vertex incident with a corner
        public int v(int c)
        {
            int t = c / 3;
            if ((c % 3) == 0)
                return (mesh.Triangles[t].V3);
            if ((c % 3) == 1)
                return (mesh.Triangles[t].V1);
            return (mesh.Triangles[t].V2);
        }

        public int t(int c)
        {
            return c / 3;
        }

        // returns vertices incident with a vertex
        public int[] VertexNeighbours(int vertex)
        {
            List<int> result = new List<int>();
            int initCorner = incidentCorner[vertex];

            result.Add(v(p(initCorner)));

            int o = opposite[n(initCorner)];
            while (o != -1)
            {
                if (n(o) == initCorner)
                    break;
                result.Add(v(o));
                o = opposite[p(o)];
            }

            if (o == -1)
            {
                // this seems to be a border vertex
                // lets assume it is not complex, i.e. that there are exactly two border edges
                // and lets move around the vertex in the opposite direction
                result.Add(v(n(initCorner)));

                o = opposite[p(initCorner)];
                while (o != -1)
                {
                    if (p(o) == initCorner)
                        break;
                    result.Add(v(o));
                    o = opposite[n(o)];
                }
            }

            return (result.ToArray());
        }

        // returns faces incident with a vertex
        public int[] VF(int vertex)
        {
            List<int> result = new List<int>();
            int initCorner = incidentCorner[vertex];

            result.Add(initCorner / 3);

            int o = opposite[n(initCorner)];
            while (o != -1)
            {
                if (n(o) == initCorner)
                    break;
                result.Add(o / 3);
                o = opposite[p(o)];
            }

            if (o == -1)
            {
                // this seems to be a border vertex
                // lets assume it is not complex, i.e. that there are exactly two border edges
                // and lets move around the vertex in the opposite direction                

                o = opposite[p(initCorner)];
                while (o != -1)
                {
                    if (p(o) == initCorner)
                        break;
                    result.Add(o / 3);
                    o = opposite[n(o)];
                }
            }

            return (result.ToArray());
        }

        // determines whether a vertex lies on the border
        public bool isOnBorder(int vertex)
        {
            // walk around the vertex and look for -1 in the corner table
            int initCorner = incidentCorner[vertex];
            int o = opposite[n(initCorner)];
            while (o != -1)
            {
                if (n(o) == initCorner)
                    break;
                o = opposite[p(o)];
            }

            return (o == -1);
        }

        // determines whether a vertex lies on the border
        public int nextVertexOnBorder(int vertex)
        {
            // walk around the vertex and look for -1 in the corner table
            int initCorner = incidentCorner[vertex];
            int o = opposite[n(initCorner)];

            if (o == -1)
                return (v(p(initCorner)));
            while (o != -1)
            {
                if (n(o) == initCorner)
                    break;
                int no = opposite[p(o)];
                if (no == -1)
                    return (v(o));
                o = no;
            }



            // this is not a border vertex!
            return (-1);
        }

        // initializes the cornertable data structure
        // fills the incidentCorner and oposite 
        public void initCornerTable()
        {
            Dictionary<Edge, int> dictionary = new Dictionary<Edge, int>();
            this.opposite = new int[mesh.Triangles.Length * 3];
            for (int i = 0; i < opposite.Length; i++)
            {
                opposite[i] = -1;
            }
            this.incidentCorner = new int[mesh.Points.Length];

            for (int i = 0; i < mesh.Triangles.Length; i++)
            {
                Triangle t = mesh.Triangles[i];

                Edge e = new Edge(t.V1, t.V2);
                int cornerIndex = 3 * i;
                processEdge(dictionary, e, cornerIndex);
                incidentCorner[t.V3] = cornerIndex;

                e = new Edge(t.V2, t.V3);
                cornerIndex = 3 * i + 1;
                processEdge(dictionary, e, cornerIndex);
                incidentCorner[t.V1] = cornerIndex;

                e = new Edge(t.V3, t.V1);
                cornerIndex = 3 * i + 2;
                processEdge(dictionary, e, cornerIndex);
                incidentCorner[t.V2] = cornerIndex;
            }
        }

        // this is an auxiliary function for the creation of the corner table
        // sees whether the edge is already in the dictionary. If it isn't, then it is added.
        private void processEdge(Dictionary<Edge, int> dictionary, Edge e, int cornerIndex)
        {
            if (dictionary.ContainsKey(e))
            {
                int oppositeCornerIndex = dictionary[e];
                opposite[cornerIndex] = oppositeCornerIndex;
                opposite[oppositeCornerIndex] = cornerIndex;
            }
            else
            {
                Edge reversed = new Edge(e.v2, e.v1);
                if (dictionary.ContainsKey(reversed))
                    throw new Exception("Non-manifold mesh at the input.");
                dictionary.Add(reversed, cornerIndex);
            }
        }

        public HashSet<int> VertexNeighboursK2(int v, int k, out int nb)
        {
            Dictionary<int, int> distances = new Dictionary<int, int>();
            nb = 0;
            HashSet<int> visitedTriangles = new HashSet<int>();
            HashSet<int> result = new HashSet<int>();
            result.Add(v);
            int start = incidentCorner[v];
            if (start < 0)
                return (null);
            Stack<int> workQueue = new Stack<int>();
            int vertex = this.v(n(start));
            result.Add(vertex);
            distances[vertex] = 1;
            nb = vertex;
            visitedTriangles.Add(t(start));
            workQueue.Push(start);
            int current = -1;
            int op = opposite[p(start)];
            if (op>0)
                current = p(op);
            while ((current != start) && (current >= 0))
            {
                vertex = this.v(n(current));
                result.Add(vertex);
                distances[vertex] = 1;
                visitedTriangles.Add(t(current));
                workQueue.Push(current);
                op = opposite[p(current)];
                if (op < 0)
                    current = -1;
                else
                    current = p(op);
            }

            // treatment of border. Only two border edges per vertex allowed!
            if (current < 0)
            {
                vertex = this.v(p(start));
                result.Add(vertex);
                distances[vertex] = 1;
                op = opposite[n(start)];
                if (op >= 0)
                    current = n(op);
                while (current >=0)
                {
                    vertex = this.v(p(current));
                    result.Add(vertex);
                    distances[vertex] = 1;
                    visitedTriangles.Add(t(current));
                    workQueue.Push(current);
                    op = opposite[n(current)];
                    if (op < 0)
                        current = -1;
                    else
                        current = n(op);
                }
            }            

            while (workQueue.Count > 0)
            {
                int c = workQueue.Pop();
                if (opposite[c] < 0)
                    continue;

                if (visitedTriangles.Contains(t(opposite[c])))
                    continue;

                int v1 = distances[this.v(p(c))];
                int v2 = distances[this.v(n(c))];

                if ((v1 == k) && (v2 == k))
                    continue;

                vertex = this.v(opposite[c]);
                if (!distances.ContainsKey(vertex))
                {
                    distances[vertex] = Math.Min(v1 + 1, v2 + 1);
                    result.Add(vertex);
                }
                visitedTriangles.Add(t(opposite[c]));

                workQueue.Push(n(opposite[c]));
                workQueue.Push(p(opposite[c]));
            }
            return (result);
        }
    }
}
