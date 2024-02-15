using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Framework
{
    public struct Quaternion
    {
        public double q0, q1, q2, q3;

        public Quaternion(double qp0, double qp1, double qp2, double qp3)
        {
            this.q0 = qp0;
            this.q1 = qp1;
            this.q2 = qp2;
            this.q3 = qp3;
        }

        public static Quaternion rotX(double alpha)
        {
            return new Quaternion(Math.Cos(alpha / 2), Math.Sin(alpha / 2), 0, 0);            
        }

        public static Quaternion rotY(double alpha)
        {
            return new Quaternion(Math.Cos(alpha / 2), 0, Math.Sin(alpha / 2), 0);
        }

        public static Quaternion rotZ(double alpha)
        {
            return new Quaternion(Math.Cos(alpha / 2), 0, 0, Math.Sin(alpha / 2));
        }

        public static Quaternion operator *(Quaternion a, Quaternion b)
        {
            return new Quaternion(            
                a.q0 * b.q0 - a.q1 * b.q1 - a.q2 * b.q2 - a.q3 * b.q3,
                a.q0 * b.q1 + a.q1 * b.q0 + a.q2 * b.q3 - a.q3 * b.q2,
                a.q0 * b.q2 - a.q1 * b.q3 + a.q2 * b.q0 + a.q3 * b.q1,
                a.q0 * b.q3 + a.q1 * b.q2 - a.q2 * b.q1 + a.q3 * b.q0);
        }

        public static double[] threeCompMult2(Quaternion a, Quaternion b)
        {
            return new double[]{                
                a.q0 * b.q1 - a.q1 * b.q0 + a.q2 * b.q3 - a.q3 * b.q2,
                a.q0 * b.q2 - a.q1 * b.q3 - a.q2 * b.q0 + a.q3 * b.q1,
                a.q0 * b.q3 + a.q1 * b.q2 - a.q2 * b.q1 - a.q3 * b.q0};
        }

        public static double[] threeCompMult3(Quaternion a, Quaternion b)
        {
            return new double[]{                
                - a.q0 * b.q1 + a.q1 * b.q0 - a.q2 * b.q3 + a.q3 * b.q2,
                - a.q0 * b.q2 + a.q1 * b.q3 + a.q2 * b.q0 - a.q3 * b.q1,
                - a.q0 * b.q3 - a.q1 * b.q2 + a.q2 * b.q1 + a.q3 * b.q0};
        }



        public static Quaternion operator +(Quaternion a, Quaternion b)
        {
            return new Quaternion(
                a.q0 + b.q0,
                a.q1 + b.q1,
                a.q2 + b.q2,
                a.q3 + b.q3);
        }

        public Quaternion Conj()
        {
            return new Quaternion(q0, -q1, -q2, -q3);
        }

        public Point3D transform(Point3D p)
        {
            Quaternion v = new Quaternion(0, p.X, p.Y, p.Z);
            Quaternion con = this.Conj();
            Quaternion r = this * v * con;
            return (new Point3D(r.q1, r.q2, r.q3));
        }

        public double Dot(Quaternion p)
        {
            return (this.q0 * p.q0 + this.q1 * p.q1 + this.q2 * p.q2 + this.q3 * p.q3);
        }    
    }
}
