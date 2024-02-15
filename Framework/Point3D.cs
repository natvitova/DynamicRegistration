using System;

namespace Framework
{
    public struct Point3D
    {        
        public double X;
        public double Y;
        public double Z;

        public double this[int index]
        {
            get
            {
                switch (index)
                {
                    case 0: return X;
                    case 1: return Y;
                    case 2: return Z;
                    default: throw new IndexOutOfRangeException();
                }
            }
            set
            {
                switch (index)
                {
                    case 0: X = value; break;
                    case 1: Y = value; break;
                    case 2: Z = value; break;
                    default: throw new IndexOutOfRangeException();
                }

            }
        }

        public Point3D(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double Abs()
        {
            return Math.Sqrt(X * X + Y * Y + Z * Z);
        }

        public double AbsSq()
        {
            return (X * X + Y * Y + Z * Z);
        }

        public double SqDistTo(Point3D p)
        {
            double dx = this.X - p.X;
            double dy = this.Y - p.Y;
            double dz = this.Z - p.Z;
            return (dx * dx + dy * dy + dz * dz);
        }

        #region Overriden methods of the class Object

        override public string ToString()
        {
            return "Point3D: [" + X + "; " + Y + "; " + Z + "]";
        }        

        public override int GetHashCode()
        {
            return this.X.GetHashCode() ^ this.Y.GetHashCode() ^ this.Z.GetHashCode();
        }

        #endregion

        #region Operators


        /// <summary>
        /// Addition of two points.
        /// </summary>
        /// <param name="a">First operand.</param>
        /// <param name="b">Second operand.</param>
        /// <returns>Resulting sum (a + b).</returns>
        public static Point3D operator +(Point3D a, Point3D b)
        {
            return (new Point3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z));
        }

        /// <summary>
        /// Subtraction of two points.
        /// </summary>
        /// <param name="a">First operand.</param>
        /// <param name="b">Second operand.</param>
        /// <returns>Resulting difference (a - b).</returns>
        public static Point3D operator -(Point3D a, Point3D b)
        {
            return (new Point3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z));
        }

        /// <summary>
        /// Sign inverse of a point.
        /// </summary>
        /// <param name="p">Operand.</param>
        /// <returns>Resulting point.</returns>
        public static Point3D operator -(Point3D p)
        {
            return (new Point3D(-p.X, -p.Y, -p.Z));
        }

        /// <summary>
        /// Multiplication by constant number.
        /// </summary>
        /// <param name="p">Point to be multiplied.</param>
        /// <param name="m">multiplicator.</param>
        /// <returns>Resulting point (m*p.x, m*p.y, m*p.z)</returns>
        public static Point3D operator *(Point3D p, double m)
        {
            return (new Point3D(p.X * m, p.Y * m, p.Z * m));
        }

        /// <summary>
        /// Multiplication by a scalar.
        /// </summary>
        /// <param name="p">Point to be multiplied.</param>
        /// <param name="m">Multiplicator.</param>
        /// <returns>Resulting point (m*p.x, m*p.y, m*p.z).</returns>
        public static Point3D operator *(double m, Point3D p)
        {
            return (new Point3D(p.X * m, p.Y * m, p.Z * m));
        }

        /// <summary>
        /// Division by a scalar.
        /// </summary>
        /// <param name="p">Point to be divided.</param>
        /// <param name="m">Divider.</param>
        /// <returns>Resulting point (p.x/m, p.y/m, p.z/m).</returns>
        public static Point3D operator /(Point3D p, double m)
        {
            return (new Point3D(p.X / m, p.Y / m, p.Z / m));
        }

        #endregion

        #region Operations

        /// <summary>
        /// Returns euclidean distance between two points.
        /// </summary>
        /// <param name="p">Argument point to which distance will be computed.</param>
        /// <returns>Euclidean distance between the points.</returns>
        public double DistanceTo(Point3D p)
        {
            return (Math.Sqrt((p.X - this.X) * (p.X - this.X) + (p.Y - this.Y) * (p.Y - this.Y) + (p.Z - this.Z) * (p.Z - this.Z)));
        }

        /// <summary>
        /// Returns squared euclidean distance between two points
        /// (is slightly faster than the DistanceTo function as no square root is computed),
        /// </summary>
        /// <param name="p">Argument point to which distance will be computed.</param>
        /// <returns>Square euclidean distance between the points.</returns>
        public double SquareDistanceTo(Point3D p)
        {
            return ((p.X - this.X) * (p.X - this.X) + (p.Y - this.Y) * (p.Y - this.Y) + (p.Z - this.Z) * (p.Z - this.Z));
        }

        /// <summary>
        /// Returns squared euclidean distance between two points
        /// (is slightly faster than the DistanceTo function as no square root is computed),
        /// </summary>
        /// <param name="p">Argument point to which distance will be computed.</param>
        /// <returns>Square euclidean distance between the points.</returns>
        public double SquareDistanceToRef(ref Point3D p)
        {
            return ((p.X - this.X) * (p.X - this.X) + (p.Y - this.Y) * (p.Y - this.Y) + (p.Z - this.Z) * (p.Z - this.Z));
        }

        #endregion

        #region DotProduct
        /// <summary>
        /// Returns the dot product of two points.
        /// </summary>
        /// <param name="arg">Second argument of the dot product operation.</param>
        /// <returns>Scalar value of this.arg.</returns>
        public double DotProduct(ref Point3D arg)
        {
            return (this.X * arg.X + this.Y * arg.Y + this.Z * arg.Z);
        }

        /// <summary>
        /// Returns the dot product of two points.
        /// </summary>
        /// <param name="arg">Second argument of the dot product operation.</param>
        /// <returns>Scalar value of this.arg.</returns>
        public double DotProduct(Point3D arg)
        {
            return (this.X * arg.X + this.Y * arg.Y + this.Z * arg.Z);
        }


        /// <summary>
        /// Returns the dot product of two points.
        /// </summary>
        /// <param name="p1">First operand.</param>
        /// <param name="p2">Second operand.</param>
        /// <returns>Dot product.</returns>
        public static double DotProduct(ref Point3D p1, ref Point3D p2)
        {
            return (p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z);
        }

        /// <summary>
        /// Returns the dot product of two points.
        /// </summary>
        /// <param name="p1">First operand.</param>
        /// <param name="p2">Second operand.</param>
        /// <returns>Dot product.</returns>
        public static double DotProduct(ref Point3D p1, Point3D p2)
        {
            return (p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z);
        }

        /// <summary>
        /// Returns the dot product of two points.
        /// </summary>
        /// <param name="p1">First operand.</param>
        /// <param name="p2">Second operand.</param>
        /// <returns>Dot product.</returns>
        public static double DotProduct(Point3D p1, ref Point3D p2)
        {
            return (p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z);
        }

        /// <summary>
        /// Returns the dot product of two points.
        /// </summary>
        /// <param name="p1">First operand.</param>
        /// <param name="p2">Second operand.</param>
        /// <returns>Dot product.</returns>
        public static double DotProduct(Point3D p1, Point3D p2)
        {
            return (p1.X * p2.X + p1.Y * p2.Y + p1.Z * p2.Z);
        }
        #endregion


        #region CrossProduct
        /// <summary>
        /// Returns the cross product of two points.
        /// </summary>
        /// <param name="arg">Second argument of the cross product operation.</param>
        /// <returns>Point3D representing the resulting vector.</returns>
        public Point3D CrossProduct(ref Point3D arg)
        {
            return (new Point3D(this.Y * arg.Z - this.Z * arg.Y, this.Z * arg.X - this.X * arg.Z, this.X * arg.Y - this.Y * arg.X));
        }

        /// <summary>
        /// Returns the cross product of two points.
        /// </summary>
        /// <param name="arg">Second argument of the cross product operation.</param>
        /// <returns>Point3D representing the resulting vector.</returns>
        public Point3D CrossProduct(Point3D arg)
        {
            return (new Point3D(this.Y * arg.Z - this.Z * arg.Y, this.Z * arg.X - this.X * arg.Z, this.X * arg.Y - this.Y * arg.X));
        }

        /// <summary>
        /// Returns the cross product of two points.
        /// </summary>
        /// <param name="p1">First argument of the cross product operation.</param>
        /// <param name="p2">Second argument of the cross product operation.</param>
        /// <returns>Point3D representing the resulting vector.</returns>
        public static Point3D CrossProduct(Point3D p1, Point3D p2)
        {
            return (new Point3D(p1.Y * p2.Z - p1.Z * p2.Y, p1.Z * p2.X - p1.X * p2.Z, p1.X * p2.Y - p1.Y * p2.X));
        }

        /// <summary>
        /// Returns the cross product of two points.
        /// </summary>
        /// <param name="p1">First argument of the cross product operation.</param>
        /// <param name="p2">Second argument of the cross product operation.</param>
        /// <returns>Point3D representing the resulting vector.</returns>
        public static Point3D CrossProduct(ref Point3D p1, ref Point3D p2)
        {
            return (new Point3D(p1.Y * p2.Z - p1.Z * p2.Y, p1.Z * p2.X - p1.X * p2.Z, p1.X * p2.Y - p1.Y * p2.X));
        }

        /// <summary>
        /// Returns the cross product of two points.
        /// </summary>
        /// <param name="p1">First argument of the cross product operation.</param>
        /// <param name="p2">Second argument of the cross product operation.</param>
        /// <returns>Point3D representing the resulting vector.</returns>
        public static Point3D CrossProduct(ref Point3D p1, Point3D p2)
        {
            return (new Point3D(p1.Y * p2.Z - p1.Z * p2.Y, p1.Z * p2.X - p1.X * p2.Z, p1.X * p2.Y - p1.Y * p2.X));
        }

        /// <summary>
        /// Returns the cross product of two points.
        /// </summary>
        /// <param name="p1">First argument of the cross product operation.</param>
        /// <param name="p2">Second argument of the cross product operation.</param>
        /// <returns>Point3D representing the resulting vector.</returns>
        public static Point3D CrossProduct(Point3D p1, ref Point3D p2)
        {
            return (new Point3D(p1.Y * p2.Z - p1.Z * p2.Y, p1.Z * p2.X - p1.X * p2.Z, p1.X * p2.Y - p1.Y * p2.X));
        }
        #endregion


        public Point3D GetSubtract(ref Point3D arg)
        {
            return new Point3D(this.X - arg.X, this.Y - arg.Y, this.Z - arg.Z);
        }

        public void Subtract(ref Point3D arg)
        {
            this.X -= arg.X;
            this.Y -= arg.Y;
            this.Z -= arg.Z;
        }

        public double DotProductRef(ref Point3D arg)
        {
            return (this.X * arg.X + this.Y * arg.Y + this.Z * arg.Z);
        }

        public Point3D CrossProductRef(ref Point3D arg)
        {
            return (new Point3D(this.Y * arg.Z - this.Z * arg.Y, this.Z * arg.X - this.X * arg.Z, this.X * arg.Y - this.Y * arg.X));
        }
        
        public void Multiply(double d)
        {
            this.X *= d;
            this.Y *= d;
            this.Z *= d;
        }

        public void AddRef(ref Point3D arg)
        {
            this.X += arg.X;
            this.Y += arg.Y;
            this.Z += arg.Z;
        }

        public void Normalize()
        {
            double l = Math.Sqrt(this.X * this.X + this.Y * this.Y + this.Z * this.Z);
            this.X /= l;
            this.Y /= l;
            this.Z /= l;
        }
    }
}
