using System;

namespace Framework
{
	/// <summary>
	/// Represent min-max box.
	/// It is automatically maintained by given point array.
	/// </summary>
	public sealed class BoundingBox
	{
		private bool isMinMaxVaild;		

        private Point3D maxPoint;

        public Point3D MaxPoint
        {
            get { return maxPoint; }
            set { maxPoint = value; }
        }
        private Point3D minPoint;

        public Point3D MinPoint
        {
            get { return minPoint; }
            set { minPoint = value; }
        }
        private Point3D midPoint;

        public Point3D MidPoint
        {
            get { return midPoint; }
            set { midPoint = value; }
        }
		private double maxDiag;

		private Point3D[] points;

		/// <summary>
		/// Creates bounding box that is automatically maintained with respect to given array.
		/// </summary>
		/// <param name="points"></param>
		public BoundingBox(Point3D[] points)
		{
			this.points = points;
            this.ValidateMinMax();
            this.ValidateMidDiag();
		}        

        /// <summary>
        /// Returns point array that is managed by curent bounding box.
        /// </summary>
        public Point3D[] Points
		{
			get
			{
				return this.points;
			}
		}

		/// <summary>
		/// Recomputes the BoundingBox from all points
		/// </summary>
		private void ValidateMinMax()
		{
			minPoint.X = Double.MaxValue;
			maxPoint.X = Double.MinValue;
            minPoint.Y = Double.MaxValue;
            maxPoint.Y = Double.MinValue;
            minPoint.Z = Double.MaxValue;
            maxPoint.Z = Double.MinValue;

            for (int i = 0; i < points.Length; i++) 
			{
				Point3D point = points[i];

				minPoint.X = Math.Min(minPoint.X, point.X);
				maxPoint.X = Math.Max(maxPoint.X, point.X);

                minPoint.Y = Math.Min(minPoint.Y, point.Y);
                maxPoint.Y = Math.Max(maxPoint.Y, point.Y);

                minPoint.Z = Math.Min(minPoint.Z, point.Z);
                maxPoint.Z = Math.Max(maxPoint.Z, point.Z);
            }
			isMinMaxVaild = true;
			this.ValidateMidDiag();
		}

		/// <summary>
		/// Recompute mid-point and max diagonal length.
		/// </summary>
		private void ValidateMidDiag()
		{
            midPoint = (minPoint + maxPoint) * 0.5;
            Point3D diag = maxPoint - minPoint;
			this.maxDiag = diag.Abs();
		}

		/// <summary> 
		/// Return volume diagonal of the bounding box.
		/// </summary> 
		public double MaxDiag  
		{ 
			get 
			{ 
				if(!isMinMaxVaild)  
					this.ValidateMinMax();  

				return maxDiag; 
			} 
		}

		/// <summary>
		/// Returns description of BoundignBox.
		/// </summary>
		public override string ToString()
		{
			string str = "BoundingBox: ";

			str += "\n";
			str += "\t" + "- min point : " + this.MinPoint + "\n";
			str += "\t" + "- max point : " + this.MaxPoint + "\n";
			str += "\t" + "- mid point : " + this.MidPoint + "\n";
			str += "\t" + "- max diag  : " + this.MaxDiag + "\n";
			return str;
		}

	} // class BoundingBox
} // namespace
