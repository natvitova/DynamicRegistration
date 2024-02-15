using System;
using System.Globalization;
using System.IO;

namespace Framework
{
    /// <summary>
    /// General tranformation of a 3D object.
    /// </summary>
    public class Transform3D2
    {
        /// <summary>
        /// double matrix 4 x 4 representing 3D-transformation.
        /// </summary>
        public double[,] matrix;

        /// <summary>
        /// The toleration for equality operator.
        /// </summary>
        public static double Epsilon = double.Epsilon;

        /// <summary>
        /// Set identity transformation.
        /// </summary>
        public void SetIdentity()
        {
            matrix = new double[,] { { 1, 0, 0, 0 }, { 0, 1, 0, 0 }, { 0, 0, 1, 0 }, { 0, 0, 0, 1 } };
        }


        public Transform3D2()
        {
            SetIdentity();
        }

        /// <summary>
        /// Add scaling to current transformation.
        /// </summary>
        /// <param name="dx">Scaling in x-axis. 1 means no scale.</param>
        /// <param name="dy">Scaling in y-axis. 1 means no scale.</param>
        /// <param name="dz">Scaling in z-axis. 1 means no scale.</param>
        public void AddScale(double dx, double dy, double dz)
        {
            Transform3D2 t = new Transform3D2();
            t.SetScale(dx, dy, dz);
            this.matrix = (this * t).matrix;
        }

        /// <summary>
        /// Set curent matrix to scaling matrix of given values.
        /// </summary>
        /// <param name="dx">Scaling in x-axis. 1 means no scale.</param>
        /// <param name="dy">Scaling in y-axis. 1 means no scale.</param>
        /// <param name="dz">Scaling in z-axis. 1 means no scale.</param>
        public void SetScale(double dx, double dy, double dz)
        {
            SetIdentity();
            matrix[0, 0] = dx;
            matrix[1, 1] = dy;
            matrix[2, 2] = dz;
        }

        /// <summary>
        /// Noncumulative rotation about axis X measured in radians.
        /// </summary>
        /// <param name="angle">Rotation angle masured in radians.</param>
        public void SetRotaionX(double angle)
        {
            SetIdentity();
            matrix[1, 1] = matrix[2, 2] = (double)Math.Cos(angle);
            matrix[2, 1] = (double)Math.Sin(angle);
            matrix[1, 2] = -matrix[2, 1];
        }

        /// <summary>
        /// Cumulative rotation about axis X.
        /// </summary>
        /// <param name="angle">Rotation angle masured in radians.</param>
        public void AddRotationX(double angle)
        {
            Transform3D2 rot = new Transform3D2();
            rot.SetRotaionX(angle);
            this.matrix = (rot * this).matrix;
        }

        /// <summary>
        /// Noncumulative rotation about axis Y measured in radians.
        /// </summary>
        /// <param name="angle">Rotation angle masured in radians.</param>
        public void SetRotaionY(double angle)
        {
            SetIdentity();
            matrix[0, 0] = matrix[2, 2] = (double)Math.Cos(angle);
            matrix[0, 2] = (double)Math.Sin(angle);
            matrix[2, 0] = -matrix[0, 2];
        }

        /// <summary>
        /// Cumulative rotation about axis Y.
        /// </summary>
        /// <param name="angle">Rotation angle masured in radians.</param>
        public void AddRotationY(double angle)
        {
            Transform3D2 rot = new Transform3D2();
            rot.SetRotaionY(angle);
            this.matrix = (rot * this).matrix;
        }

        /// <summary>
        /// Noncumulative rotation about axis Z measured in radians.
        /// </summary>
        /// <param name="angle">Rotation angle masured in radians.</param>
        public void SetRotaionZ(double angle)
        {
            SetIdentity();
            matrix[0, 0] = matrix[1, 1] = (double)Math.Cos(angle);
            matrix[1, 0] = (double)Math.Sin(angle);
            matrix[0, 1] = -matrix[1, 0];
        }

        /// <summary>
        /// Cumulative rotation about axis Z.
        /// </summary>
        /// <param name="angle">Rotation angle masured in radians.</param>
        public void AddRotationZ(double angle)
        {
            Transform3D2 rot = new Transform3D2();
            rot.SetRotaionZ(angle);
            this.matrix = (rot * this).matrix;
        }

        /// <summary>
        /// Noncumlative translation.
        /// </summary>
        /// <param name="x">Translation in x-axis.</param>
        /// <param name="y">Translation in y-axis.</param>
        /// <param name="z">Translation in z-axis.</param>
        public void SetTranslation(double x, double y, double z)
        {
            SetIdentity();
            matrix[0, 3] = x;
            matrix[1, 3] = y;
            matrix[2, 3] = z;
        }

        /// <summary>
        /// Cumulative translation
        /// </summary>
        /// <param name="x">Translation in x-axis.</param>
        /// <param name="y">Translation in y-axis.</param>
        /// <param name="z">Translation in z-axis.</param>
        public void AddTranslation(double x, double y, double z)
        {
            matrix[0, 3] += x;
            matrix[1, 3] += y;
            matrix[2, 3] += z;
        }

        /// <summary>
        /// Transform the point.
        /// </summary>
        /// <param name="op">Point to be transform. This parameter won't be changed.</param>
        /// <returns>New instance of transformed point.</returns>
        public Point3D Transform(Point3D op)
        {
            Point3D tp = new Point3D();

            tp.X = matrix[0, 0] * op.X + matrix[0, 1] * op.Y + matrix[0, 2] * op.Z + matrix[0, 3];
            tp.Y = matrix[1, 0] * op.X + matrix[1, 1] * op.Y + matrix[1, 2] * op.Z + matrix[1, 3];
            tp.Z = matrix[2, 0] * op.X + matrix[2, 1] * op.Y + matrix[2, 2] * op.Z + matrix[2, 3];

            double w = matrix[3, 0] * op.X + matrix[3, 1] * op.Y + matrix[3, 2] * op.Z + matrix[3, 3];

            tp.X /= w;
            tp.Y /= w;
            tp.Z /= w;

            return tp;
        }

        /// <summary>
        /// Transform the point.
        /// </summary>
        /// <param name="op">Point to be transform. This parameter won't be changed.</param>
        /// <returns>New instance of transformed point.</returns>
        public Point3D TransformNoW(ref Point3D op)
        {
            Point3D tp = new Point3D();

            tp.X = matrix[0, 0] * op.X + matrix[0, 1] * op.Y + matrix[0, 2] * op.Z + matrix[0, 3];
            tp.Y = matrix[1, 0] * op.X + matrix[1, 1] * op.Y + matrix[1, 2] * op.Z + matrix[1, 3];
            tp.Z = matrix[2, 0] * op.X + matrix[2, 1] * op.Y + matrix[2, 2] * op.Z + matrix[2, 3];            

            return tp;
        }

        /// <summary>
        /// Transform the point.
        /// </summary>
        /// <param name="op">Point to be transform. This parameter won't be changed.</param>
        /// <returns>New instance of transformed point.</returns>
        public Point3D TransformNormal(Point3D op)
        {
            Point3D tp = new Point3D();

            tp.X = matrix[0, 0] * op.X + matrix[0, 1] * op.Y + matrix[0, 2] * op.Z;
            tp.Y = matrix[1, 0] * op.X + matrix[1, 1] * op.Y + matrix[1, 2] * op.Z;
            tp.Z = matrix[2, 0] * op.X + matrix[2, 1] * op.Y + matrix[2, 2] * op.Z;

            return tp;
        }

        public void TransformNoWBack(ref Point3D op)
        {
            double x = matrix[0, 0] * op.X + matrix[0, 1] * op.Y + matrix[0, 2] * op.Z + matrix[0, 3];
            double y = matrix[1, 0] * op.X + matrix[1, 1] * op.Y + matrix[1, 2] * op.Z + matrix[1, 3];
            op.Z = matrix[2, 0] * op.X + matrix[2, 1] * op.Y + matrix[2, 2] * op.Z + matrix[2, 3];
            op.Y = y;
            op.X = x;            
        }

        /// <summary>
        /// Composition of transformation. Matrix multiplication.
        /// </summary>
        /// <param name="t1"></param>
        /// <param name="t2"></param>
        /// <returns></returns>
        public static Transform3D2 operator *(Transform3D2 t1, Transform3D2 t2)
        {
            Transform3D2 t = new Transform3D2();
            t.matrix = new double[,]
            {
                {
                    t1.matrix[0,0]*t2.matrix[0,0] + t1.matrix[0,1]*t2.matrix[1,0] + t1.matrix[0,2]*t2.matrix[2,0]+t1.matrix[0,3]*t2.matrix[3,0],
                    t1.matrix[0,0]*t2.matrix[0,1] + t1.matrix[0,1]*t2.matrix[1,1] + t1.matrix[0,2]*t2.matrix[2,1]+t1.matrix[0,3]*t2.matrix[3,1],
                    t1.matrix[0,0]*t2.matrix[0,2] + t1.matrix[0,1]*t2.matrix[1,2] + t1.matrix[0,2]*t2.matrix[2,2]+t1.matrix[0,3]*t2.matrix[3,2],
                    t1.matrix[0,0]*t2.matrix[0,3] + t1.matrix[0,1]*t2.matrix[1,3] + t1.matrix[0,2]*t2.matrix[2,3]+t1.matrix[0,3]*t2.matrix[3,3]
                },
                {
                    t1.matrix[1,0]*t2.matrix[0,0] + t1.matrix[1,1]*t2.matrix[1,0] + t1.matrix[1,2]*t2.matrix[2,0]+t1.matrix[1,3]*t2.matrix[3,0],
                    t1.matrix[1,0]*t2.matrix[0,1] + t1.matrix[1,1]*t2.matrix[1,1] + t1.matrix[1,2]*t2.matrix[2,1]+t1.matrix[1,3]*t2.matrix[3,1],
                    t1.matrix[1,0]*t2.matrix[0,2] + t1.matrix[1,1]*t2.matrix[1,2] + t1.matrix[1,2]*t2.matrix[2,2]+t1.matrix[1,3]*t2.matrix[3,2],
                    t1.matrix[1,0]*t2.matrix[0,3] + t1.matrix[1,1]*t2.matrix[1,3] + t1.matrix[1,2]*t2.matrix[2,3]+t1.matrix[1,3]*t2.matrix[3,3]
                },
                {
                    t1.matrix[2,0]*t2.matrix[0,0] + t1.matrix[2,1]*t2.matrix[1,0] + t1.matrix[2,2]*t2.matrix[2,0]+t1.matrix[2,3]*t2.matrix[3,0],
                    t1.matrix[2,0]*t2.matrix[0,1] + t1.matrix[2,1]*t2.matrix[1,1] + t1.matrix[2,2]*t2.matrix[2,1]+t1.matrix[2,3]*t2.matrix[3,1],
                    t1.matrix[2,0]*t2.matrix[0,2] + t1.matrix[2,1]*t2.matrix[1,2] + t1.matrix[2,2]*t2.matrix[2,2]+t1.matrix[2,3]*t2.matrix[3,2],
                    t1.matrix[2,0]*t2.matrix[0,3] + t1.matrix[2,1]*t2.matrix[1,3] + t1.matrix[2,2]*t2.matrix[2,3]+t1.matrix[2,3]*t2.matrix[3,3]
                },
                {
                    t1.matrix[3,0]*t2.matrix[0,0] + t1.matrix[3,1]*t2.matrix[1,0] + t1.matrix[3,2]*t2.matrix[2,0]+t1.matrix[3,3]*t2.matrix[3,0],
                    t1.matrix[3,0]*t2.matrix[0,1] + t1.matrix[3,1]*t2.matrix[1,1] + t1.matrix[3,2]*t2.matrix[2,1]+t1.matrix[3,3]*t2.matrix[3,1],
                    t1.matrix[3,0]*t2.matrix[0,2] + t1.matrix[3,1]*t2.matrix[1,2] + t1.matrix[3,2]*t2.matrix[2,2]+t1.matrix[3,3]*t2.matrix[3,2],
                    t1.matrix[3,0]*t2.matrix[0,3] + t1.matrix[3,1]*t2.matrix[1,3] + t1.matrix[3,2]*t2.matrix[2,3]+t1.matrix[3,3]*t2.matrix[3,3]
                }
            };
            return (t);
        }

        /// <summary>
        /// Full access to particular matrix elements.
        /// </summary>
        public double this[int i, int j]
        {
            set
            {
                matrix[i, j] = value;
            }
            get
            {
                return matrix[i, j];
            }
        }
        
        public static Transform3D2[] randomRigidTransformPair(double shift)
        {
            Random r = new Random();

            var a1 = r.NextDouble();
            var r1 = new Transform3D2();
            r1.SetRotaionX(a1 * 2 * Math.PI);
            var r1i = new Transform3D2();
            r1i.SetRotaionX(-a1 * 2 * Math.PI);

            var a2 = r.NextDouble();
            var r2 = new Transform3D2();
            r2.SetRotaionY(a2 * 2 * Math.PI);
            var r2i = new Transform3D2();
            r2i.SetRotaionY(-a2 * 2 * Math.PI);

            var a3 = r.NextDouble();
            var r3 = new Transform3D2();
            r3.SetRotaionZ(a3 * 2 * Math.PI);
            var r3i = new Transform3D2();
            r3i.SetRotaionZ(-a3 * 2 * Math.PI);

            var x = r.NextDouble();
            var y = r.NextDouble();
            var z = r.NextDouble();
            var t = new Transform3D2();
            t.SetTranslation((x - 0.5) * shift, (y - 0.5) * shift, (z - 0.5) * shift);
            var ti = new Transform3D2();
            ti.SetTranslation((0.5 - x) * shift, (0.5 - y) * shift, (0.5 - z) * shift);

            return (new Transform3D2[] { t * r3 * r2 * r1, r1i * r2i * r3i * ti });
        }


        /// <summary>
        /// brute force transformation distance function
        /// </summary>
        /// <param name="t"></param>
        /// <param name="mesh"></param>
        /// <returns></returns>
        public double bfDistanceTo(Transform3D2 t, TriangleMesh mesh)
        {
            double sum = 0;
            for(int i = 0;i<mesh.Points.Length;i++)
            {
                Point3D p = mesh.Points[i];

                Point3D transformedPbyThis = this.TransformNoW(ref p);
                Point3D transformedPbyT = t.TransformNoW(ref p);

                double d = transformedPbyThis.SqDistTo(transformedPbyT);
                sum += d;
            }
            return (Math.Sqrt(sum/mesh.Points.Length));
        }

        // ===== Methods of the class Object ==============================================
        #region Overrided methods of the class Object

        /// <summary>
        /// Returns a String that represents the current Object.
        /// </summary>
        /// <returns>String that represents the current Object.</returns>
        public override string ToString()
        {
            string str = "";
            for (int i = 0; i < 4; i++)
                str += "   [" + matrix[i, 0].ToString("G4") + ", " + matrix[i, 1].ToString("G4") + ", " + matrix[i, 2].ToString("G4") + ", " + matrix[i, 3].ToString("G4") + "]\r\n";
            return str.Substring(0, str.Length - 2);
        }

        #endregion

    } // class Transform3D
} // namespace