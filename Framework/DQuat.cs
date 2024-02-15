using System;

namespace Framework
{
    public struct DQuat
    {
        public Quaternion real, dual;

        public DQuat(Quaternion realp, Quaternion dualp)
        {
            real = realp;
            dual = dualp;
        }

        public static DQuat Identity()
        {
            Quaternion real = new Quaternion(1, 0, 0, 0);
            Quaternion dual = new Quaternion(0, 0, 0, 0);
            return(new DQuat(real, dual));
        }

        public static DQuat operator *(DQuat a, DQuat b)
        {
            Quaternion r = a.real * b.real;
            Quaternion d = a.real*b.dual + a.dual*b.real;
            DQuat res = new DQuat(r,d);
            return(res);
        }

        public void multiply(DQuat b)
        {
            Quaternion r = this.real * b.real;
            Quaternion d = this.real * b.dual + this.dual * b.real;
            this.real = r;
            this.dual = d;
        }

        public static Point3D dualMult(DQuat a, DQuat b)
        {
            double[] d1 = Quaternion.threeCompMult2(a.real, b.dual);
            double[] d2 = Quaternion.threeCompMult3(a.dual, b.real);
            return new Point3D(d1[0] + d2[0], d1[1] + d2[1], d1[2] + d2[2]);
        }

        public static DQuat operator *(double f, DQuat q)
        {
            DQuat result = new DQuat();
            result.real.q0 = q.real.q0 * f;
            result.real.q1 = q.real.q1 * f;
            result.real.q2 = q.real.q2 * f;
            result.real.q3 = q.real.q3 * f;
            result.dual.q0 = q.dual.q0 * f;
            result.dual.q1 = q.dual.q1 * f;
            result.dual.q2 = q.dual.q2 * f;
            result.dual.q3 = q.dual.q3 * f;
            return (result);
        }

        public static DQuat operator +(DQuat a, DQuat b)
        {
            return new DQuat(a.real + b.real, a.dual + b.dual);
        }

        public Point3D Transform(Point3D p)
        {
            DQuat tx = new DQuat();
            tx.real.q0 = this.real.q0;
            tx.real.q1 = this.real.q1;
            tx.real.q2 = this.real.q2;
            tx.real.q3 = this.real.q3;
            tx.dual.q0 = -this.real.q1 * p.X - this.real.q2 * p.Y - this.real.q3 * p.Z + this.dual.q0;
            tx.dual.q1 = this.real.q0 * p.X + this.real.q2 * p.Z - this.real.q3 * p.Y + this.dual.q1;
            tx.dual.q2 = this.real.q0 * p.Y - this.real.q1 * p.Z + this.real.q3 * p.X + this.dual.q2;
            tx.dual.q3 = this.real.q0 * p.Z + this.real.q1 * p.Y - this.real.q2 * p.X + this.dual.q3;
            Point3D r2 = DQuat.dualMult(tx, this);
            return r2;            
        }

        public static DQuat translation(Point3D p)
        {
            Quaternion r = new Quaternion(1, 0, 0, 0);
            Quaternion d = new Quaternion(0, p.X/2, p.Y/2, p.Z/2);
            DQuat dq = new DQuat(r, d);
            return (dq);
        }

        public void AddTranslation(Point3D p)
        {
            DQuat trn = DQuat.translation(p);
            this.multiply(trn);
        }

        public void AddTranslation(double x, double y, double z)
        {
            DQuat trn = DQuat.translation(new Point3D(x,y,z));
            this.multiply(trn);
        }

        public void AddRotationX(double a)
        {
            Quaternion r = Quaternion.rotX(a);
            DQuat dq = new DQuat(r, new Quaternion(0, 0, 0, 0));
            this.multiply(dq);
        }

        public void AddRotationY(double a)
        {
            Quaternion r = Quaternion.rotY(a);
            DQuat dq = new DQuat(r, new Quaternion(0, 0, 0, 0));
            this.multiply(dq);
        }

        public void AddRotationZ(double a)
        {
            Quaternion r = Quaternion.rotZ(a);
            DQuat dq = new DQuat(r, new Quaternion(0, 0, 0, 0));
            this.multiply(dq);
        }

        public void Normalize()
        {
            double n = this.Norm();
            this = 1 / n * this;
        }

        public double Norm()
        {
            return (Math.Sqrt(this.real.Dot(this.real)));
        }

        public Transform3D2 getTransform2()
        {
            Point3D x = new Point3D(1, 0, 0);
            Point3D y = new Point3D(0, 1, 0);
            Point3D z = new Point3D(0, 0, 1);
            Point3D o = new Point3D(0, 0, 0);
            o = this.Transform(o);
            x = this.Transform(x)-o;
            y = this.Transform(y)-o;
            z = this.Transform(z)-o;
            
            Transform3D2 result = new Transform3D2();
            result.matrix = new double[,] { { x.X, y.X, z.X, o.X }, { x.Y, y.Y, z.Y, o.Y }, { x.Z, y.Z, z.Z, o.Z }, { 0, 0, 0, 1 } };
            return (result);
        }

        /// <summary>
        /// Creates a general rotation matrix. The rotation axis is specified by vertices passed as <paramref name="axisA"/> and <paramref name="axisB"/>, the rotation angle is specified by <paramref name="angleRadians"/>.
        /// </summary>
        /// <param name="axisA">First point specifying the rotation axis</param>
        /// <param name="axisB">Second point specifying the rotation axis</param>
        /// <param name="angleRadians">Angle of rotation in radians.</param>
        /// <returns>Resulting general rotation matrix</returns>
        public static DQuat generalRotation(Point3D axisA, Point3D axisB, double angleRadians)
        {
            DQuat translateBack = DQuat.Identity();
            translateBack.AddTranslation(-axisA.X, -axisA.Y, -axisA.Z);
            double projectedX = axisB.X - axisA.X;
            double projectedY = axisB.Y - axisA.Y;
            double projectedZ = axisB.Z - axisA.Z;

            double lengthXZ = Math.Sqrt(projectedX * projectedX + projectedZ * projectedZ);
            double arg = projectedX / lengthXZ;
            if (arg > 1)
                arg = 1;
            if (arg < -1)
                arg = -1;
            double alpha = Math.Acos(arg);
            if (projectedZ < 0)
                alpha = Math.PI * 2 - alpha;

            if (lengthXZ == 0)
                alpha = 0;

            DQuat rotate1 = DQuat.Identity();
            rotate1.AddRotationY(alpha);
            Point3D projectedRotated = rotate1.Transform(new Point3D(projectedX, projectedY, projectedZ));

            double lengthXY = Math.Sqrt(projectedRotated.X * projectedRotated.X + projectedRotated.Y * projectedRotated.Y);
            arg = projectedRotated.X / lengthXY;
            if (arg > 1)
                arg = 1;
            if (arg < -1)
                arg = -1;
            double beta = Math.Acos(arg);
            if (projectedY < 0)
                beta = Math.PI * 2 - beta;

            if (lengthXY == 0)
                beta = 0;



            DQuat rotate2 = DQuat.Identity();
            rotate2.AddRotationZ(-beta);

            DQuat mainRotate = DQuat.Identity();
            mainRotate.AddRotationX(angleRadians);

            DQuat rotate2inv = DQuat.Identity();
            rotate2inv.AddRotationZ(beta);

            DQuat rotate1inv = DQuat.Identity();
            rotate1inv.AddRotationY(-alpha);

            DQuat translate = DQuat.Identity();
            translate.AddTranslation(axisA.X, axisA.Y, axisA.Z);

            DQuat result = translate * rotate1inv * rotate2inv * mainRotate * rotate2 * rotate1 * translateBack;
            return (result);
        }

        public void fillRT(double[] rot, double[] t)
        {
            Point3D x = new Point3D(1, 0, 0);
            Point3D y = new Point3D(0, 1, 0);
            Point3D z = new Point3D(0, 0, 1);
            Point3D o = new Point3D(0, 0, 0);
            o = this.Transform(o);
            x = this.Transform(x) - o;
            y = this.Transform(y) - o;
            z = this.Transform(z) - o;
            
            rot[0] = x.X;
            rot[3] = x.Y;
            rot[6] = x.Z;
            rot[1] = y.X;
            rot[4] = y.Y;
            rot[7] = y.Z;
            rot[2] = z.X;
            rot[5] = z.Y;
            rot[8] = z.Z;
            t[0] = o.X;
            t[1] = o.Y;
            t[2] = o.Z;
        }
    }
}
