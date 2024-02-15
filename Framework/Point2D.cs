using System;
using System.Globalization;

namespace Framework
{
	/// <summary>
	/// Represents a 2D point defined by two coordinates - X and Y.
	/// </summary>
	public struct Point2D
	{
		/// <summary>
		/// x-coordinate of point.
		/// </summary>
		public double X;
		/// <summary>
		/// y-coordinate of point.
		/// </summary>
		public double Y;

		/// <summary>
		/// Create a Point2D object.
		/// </summary>
		/// <param name="x">x-coordinate of point.</param>
		/// <param name="y">y-coordinate of point.</param>
		public Point2D(double x, double y)
		{
			this.X = x;
			this.Y = y;
		} // Point2D()

		/// <summary>
		/// Returns the distance of point and coordinate origin.
		/// </summary>
		/// <returns>The distance of point from and coordinate origin.</returns>
		public double Abs()
		{
			return Math.Sqrt(this.X * this.X + this.Y * this.Y);
		} // Abs()

        /// <summary>
        /// Returns the dot product of two points.
        /// </summary>
        /// <param name="arg">Second argument of the dot product operation.</param>
        /// <returns>Scalar value of this.arg.</returns>
        public double DotProduct(Point2D arg)
        {
            return (this.X * arg.X + this.Y * arg.Y);
        }

        /// <summary>
        /// Returns euclidean distance between two points.
        /// </summary>
        /// <param name="p">Argument point to which distance will be computed.</param>
        /// <returns>Euclidean distance between the points.</returns>
        public double DistanceTo(Point3D p)
        {
            return (Math.Sqrt((p.X - this.X) * (p.X - this.X) + (p.Y - this.Y) * (p.Y - this.Y)));
        }

        /// <summary>
        /// Returns squared euclidean distance between two points
        /// (is slightly faster than the DistanceTo function as no square root is computed),
        /// </summary>
        /// <param name="p">Argument point to which distance will be computed.</param>
        /// <returns>Square euclidean distance between the points.</returns>
        public double SquareDistanceTo(Point3D p)
        {
            return ((p.X - this.X) * (p.X - this.X) + (p.Y - this.Y) * (p.Y - this.Y));
        }

		// ===== Methods of the class Object ==============================================
		#region Overrided methods of the class Object

		/// <summary>
		/// Returns a String that represents the current Object.
		/// </summary>
		/// <returns>String that represents the current Object.</returns>
		override public string ToString()
		{
			return "Point2D: [" + this.X + "; " + this.Y + "]";
		} // ToString()


		/// <summary>
		/// Determines whether the specified Object is equal to the current Object.
		/// </summary>
		/// <param name="obj">Specified Object to compare with the current Object.</param>
		/// <returns>True if the specified Object is equal to the current Object. Otherwise, false.</returns>
		public override bool Equals(object obj)
		{
			return Point2D.Equals(this, obj);
		}
		
		/// <summary>
		/// Returns a hash code for the current Object.
		/// </summary>
		/// <returns>A hash code for the current Object.</returns>
		public override int GetHashCode()
		{
			return this.X.GetHashCode() ^ this.Y.GetHashCode();
		}

		#endregion



		// ===== Operators ================================================================
		#region Operators

        /// <summary>
        /// Operator of postmultiplication by double value. Performs per element multiplication.
        /// </summary>
        /// <param name="p">Left operand.</param>
        /// <param name="d">Right operand.</param>
        /// <returns>Result of multiplication.</returns>
        public static Point2D operator *(Point2D p, double d)
        {
            return(new Point2D(p.X*d, p.Y*d));
        }

        /// <summary>
        /// Operator of premultiplication by double value. Performs per element multiplication.
        /// </summary>
        /// <param name="d">Left operand.</param>
        /// <param name="p">Right operand.</param>
        /// <returns>Result of multiplication.</returns>
        public static Point2D operator *(double d, Point2D p)
        {
            return (new Point2D(p.X * d, p.Y * d));
        }

        /// <summary>
        /// Multiplication operator. Performs per element multiplication.
        /// </summary>
        /// <param name="p1">First argument.</param>
        /// <param name="p2">Second argument.</param>
        /// <returns>Result of multiplication.</returns>
        public static Point2D operator *(Point2D p1, Point2D p2)
        {
            return (new Point2D(p1.X * p2.X, p1.Y * p2.Y));
        }

        /// <summary>
        /// Division operator. Performs per element division.
        /// </summary>
        /// <param name="p1">First argument.</param>
        /// <param name="p2">Second argument.</param>
        /// <returns>Result of division.</returns>
        public static Point2D operator /(Point2D p1, Point2D p2)
        {
            return (new Point2D(p1.X / p2.X, p1.Y / p2.Y));
        }

        /// <summary>
        /// Operator of division by double value. Performs per element division.
        /// </summary>
        /// <param name="p">Point to be divided.</param>
        /// <param name="d">Divisor.</param>
        /// <returns>Result of division.</returns>
        public static Point2D operator /(Point2D p, double d)
        {
            return (new Point2D(p.X / d, p.Y / d));
        }

        /// <summary>
        /// Addition operator. Performs per element addition.
        /// </summary>
        /// <param name="p1">First argument.</param>
        /// <param name="p2">Second argument.</param>
        /// <returns>Resulting sum.</returns>
        public static Point2D operator +(Point2D p1, Point2D p2)
        {
            return (new Point2D(p1.X + p2.X, p1.Y + p2.Y));
        }

        /// <summary>
        /// Subtraction operator. Performs per element subtraction.
        /// </summary>
        /// <param name="p1">First argument.</param>
        /// <param name="p2">Second argument.</param>
        /// <returns>Difference.</returns>
        public static Point2D operator -(Point2D p1, Point2D p2)
        {
            return (new Point2D(p1.X - p2.X, p1.Y - p2.Y));
        }

		#endregion

	} // class Point2D
} // namespace
