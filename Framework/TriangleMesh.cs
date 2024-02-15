using System;
using System.Collections.Generic;

namespace Framework
{
    public class TriangleMesh
    {
        Point3D[] points;
        Triangle[] triangles;
        Point3D[] normals;
        private KDTree tree;

        public double radius;
        public double maxArea;
        public double avgEL = 0;

        public Point3D[] Normals
        {
            get { return normals; }
            set { normals = value; }
        }

        public Point3D[] Points
        {
            get { return points; }
            set { points = value; }
        }

        public Triangle[] Triangles
        {
            get { return triangles; }
            set { triangles = value; }
        }

        public String[] AdditionalVertexData { get; set; }

        public TriangleMesh()
        {
        }

        public KDTree Tree
        {
            get
            {
                if (tree == null)
                    tree = new KDTree(this.points);
                return tree;

            }
        }

        public void ComputeNormals()
        {
            if (triangles.Length != 0)
                ComputeNormalsTriangleMesh();
            else
                ComputeNormalsPointCloud();
        }

        private void ComputeNormalsTriangleMesh()
        {
            Point3D[] normals = new Point3D[points.Length];
            for (int i = 0; i < triangles.Length; i++)
            {
                Triangle t = triangles[i];
                Point3D p1 = points[t.V1];
                Point3D p2 = points[t.V2];
                Point3D p3 = points[t.V3];
                Point3D v1 = p2 - p1;
                Point3D v2 = p3 - p1;
                Point3D n = v1.CrossProductRef(ref v2);
                if (n.Abs() == 0)
                    continue;
                n *= 1 / n.Abs();
                normals[t.V1].AddRef(ref n);
                normals[t.V2].AddRef(ref n);
                normals[t.V3].AddRef(ref n);
            }
            for (int i = 0; i < normals.Length; i++)
            {
                double l = normals[i].Abs();
                if (l < 0.00001)
                    normals[i] = new Point3D(1, 0, 0);
                else
                    normals[i] *= 1.0 / l;
            }

            this.normals = normals;
        }

        object computeSomeNormals(object input, int threadId, object[] dat)
        {
            DateTime start = DateTime.Now;
            double lim = 8;
            int j = (int)input;
            for (int i = j * 1000; (i < points.Length) && (i < ((j + 1) * 1000)); i++)
            {
                var nbs = Tree.findAllCloserThan(points[i], lim);
                while (nbs.Count > 30)
                {
                    lim *= 0.8;
                    nbs = Tree.findAllCloserThan(points[i], lim);
                }
                while (nbs.Count < 15 && lim < 300)
                {
                    lim *= 1.2;
                    nbs = Tree.findAllCloserThan(points[i], lim);
                }
                if (nbs.Count < 3)
                    normals[i] = new Point3D(0, 0, 1);
                else
                    normals[i] = fitPlane(nbs);
                //normalsCounter++;
                // orient normals
                if (normals[i].DotProduct(points[i]) > 0) 
                    normals[i] *= -1;
            }
            return null;
        }
        private void ComputeNormalsPointCloud()
        {
            Point3D z = new Point3D(0, 0, 1);
            this.normals = new Point3D[points.Length];
            // compute normals
            object[] input = new object[points.Length / 1000 + 1];
            for (int i = 0; i < input.Length; i++)
                input[i] = i;
            tree = this.Tree;
            ThreadedExecution<object, object> te = new ThreadedExecution<object, object>(input, new ThreadedExecution<object, object>.ExecBody(this.computeSomeNormals));
            te.Execute();
        }

        private Point3D fitPlane(List<int> nbs)
        {
            Point3D centroid = new Point3D();
            for (int i = 0; i < nbs.Count; i++)
                centroid += points[nbs[i]];
            centroid *= 1.0 / nbs.Count;
            double a = 0;
            double b = 0;
            double c = 0;
            double d = 0;
            double e = 0;
            double f = 0;
            // a d f
            // d b e
            // f e c
            for (int i = 0; i < nbs.Count; i++)
            {
                Point3D point = points[nbs[i]] - centroid;
                a += point.X * point.X;
                b += point.Y * point.Y;
                c += point.Z * point.Z;
                d += point.X * point.Y;
                e += point.Y * point.Z;
                f += point.X * point.Z;
            }
            double p1 = d * d + e * e + f * f;
            double q = (a + b + c) / 3; // trace(A) is the sum of all diagonal values
            double p2 = (a - q) * (a - q) + (b - q) * (b - q) + (c - q) * (c - q) + 2 * p1;
            double p = Math.Sqrt(p2 / 6);
            double ba = a / p - q / p;
            double bb = b / p - q / p;
            double bc = c / p - q / p;
            double bd = d / p;
            double be = e / p;
            double bf = f / p;
            // B = (1 / p) * (A - q * I) % I is the identity matrix
            var detB = ba * bb * bc +
                bd * be * bf * 2 -
                bb * bf * bf -
                bd * bd * bc -
                ba * be * be;
            var r = detB / 2;
            // In exact arithmetic for a symmetric matrix - 1 <= r <= 1
            // but computation error can leave it slightly outside this range.
            double phi = 0;
            if (r <= -1)
                phi = Math.PI / 3;
            else if (r <= 1)
                phi = Math.Acos(r) / 3;
            // the eigenvalues satisfy eig3 <= eig2 <= eig1
            var eig1 = q + 2 * p * Math.Cos(phi);
            var eig3 = q + 2 * p * Math.Cos(phi + (2 * Math.PI / 3));
            var eig2 = 3 * q - eig1 - eig3; // since trace(A) = eig1 + eig2 + eig3
            var smEig = eig1;
            if (eig2 < eig1)
                smEig = eig2;
            if (eig3 < smEig)
                smEig = eig3;
            Point3D v1 = new Point3D(a - smEig, d, f);
            Point3D v2 = new Point3D(d, b - smEig, e);
            var result = v1.CrossProduct(v2);
            result *= 1 / result.Abs();
            return result;
        }//fitPlane

        public double Radius()
        {
            Point3D centroid = new Point3D();
            foreach (Point3D p in points)
                centroid += p;
            centroid *= 1.0 / points.Length;
            double sum = 0;
            foreach (Point3D p in points)
                sum += p.DistanceTo(centroid);
            return (sum / points.Length);
        }

        public int uniformRandomVertex(Random r)
        {
            return r.Next(this.points.Length); ;
        }
    }//TriangleMesh
}
