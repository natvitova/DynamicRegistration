using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Framework;
using OpenGL4NET;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;

namespace ClusterRegister
{
    class GLVerifier : AbstractVerifier
    {
        #region parameters
        double minOverlapRatio = 0.5;
        double minAcceptScore = 0.8;
        int resolution;

        #endregion

        Point3D AABBmin;
        Point3D AABBmax;
        Point3D CamBBmin;
        Point3D CamBBmax;
        Point3D ViewDir;

        Matrixd ViewMatrix;
        Matrixd ProjMatrix;

        System.Windows.Forms.Form form;
        RenderingContext rc;

        int Qtris;

        OpenGL4NET.Program programP;
        OpenGL4NET.Program programQ;

        uint pVao;
        uint qVao;

        uint PFrameBuffer;
        uint PColorBuffer;
        uint QFrameBuffer;
        uint QColorBuffer;

        long Ppixels;
        long Qpixels;


        gl.GLDEBUGPROC debugCallback;

        /// <summary>
        /// OpenGL Debug callback
        /// </summary>
        /// <param name="source"></param>
        /// <param name="type"></param>
        /// <param name="id"></param>
        /// <param name="severity"></param>
        /// <param name="length"></param>
        /// <param name="message"></param>
        /// <param name="userParam"></param>
        void DebugCallback(int source, int type, int id, int severity, int length, string message, IntPtr userParam)
        {
            StackTrace trace = new StackTrace(true);
            StackFrame glcall = trace.GetFrame(1);
            StackFrame appcall = trace.GetFrame(2);

            if ((GL.DEBUG_SEVERITY)severity == GL.DEBUG_SEVERITY.HIGH) Console.ForegroundColor = ConsoleColor.Red;
            if ((GL.DEBUG_SEVERITY)severity == GL.DEBUG_SEVERITY.MEDIUM) Console.ForegroundColor = ConsoleColor.Yellow;
            if ((GL.DEBUG_SEVERITY)severity == GL.DEBUG_SEVERITY.LOW) Console.ForegroundColor = ConsoleColor.Blue;

            Console.WriteLine("[{0}]\t{1}:\n\t{2} \n\tmethod: {5} in {3}:{4}\n\tGL call: {6}", (GL.DEBUG_SOURCE)source, (GL.DEBUG_TYPE)type, message, appcall.GetFileName(), appcall.GetFileLineNumber(), appcall.GetMethod().Name, glcall.GetMethod().ToString());
            Console.ForegroundColor = ConsoleColor.Gray;
        }


        public GLVerifier(TriangleMesh P, TriangleMesh Q, int resolution = 64)
        {
            this.resolution = resolution;
            //lock (lockObj)
            {
                Qtris = Q.Triangles.Length;

                form = new System.Windows.Forms.Form();
                //form.FormBorderStyle = FormBorderStyle.None;
                //form.TopMost = true;
                form.Text = "GL Verifier";
                form.ClientSize = new System.Drawing.Size(resolution,0);
                form.Show();

                rc = RenderingContext.CreateContext(form, new RenderingContextSetting() { multisample = 0, majorVersion = 4, profile = RenderingContextSetting.ProfileEnum.Compatibility/*, context = RenderingContextSetting.ContextEnum.Debug */});
                rc.MakeCurrent();

                /*gl.Enable(GL.DEBUG_OUTPUT_SYNCHRONOUS_ARB);
                debugCallback = new gl.GLDEBUGPROC(DebugCallback);
                gl.DebugMessageControl(GL.DONT_CARE, GL.DONT_CARE, GL.DONT_CARE, GL.DONT_CARE, new uint[] { 0 }, true);
                gl.DebugMessageCallback(debugCallback, IntPtr.Zero);*/

                gl.ClearColor(0, 0, 0, 0);
                gl.Enable(GL.DEPTH_TEST);
                gl.ClearDepth(1);
                gl.DepthFunc(GL.LESS);
                gl.Enable(GL.CULL_FACE);

                programP = new OpenGL4NET.Program("P", System.IO.File.ReadAllText("GLVerifier.P.vert"), System.IO.File.ReadAllText("GLVerifier.P.frag"));
                programQ = new OpenGL4NET.Program("Q", System.IO.File.ReadAllText("GLVerifier.Q.vert"), System.IO.File.ReadAllText("GLVerifier.Q.frag"));



                PFrameBuffer = CreateFrameBuffer(resolution, resolution, out PColorBuffer, GL.RG32F, GL.RG, GL.FLOAT);
                QFrameBuffer = CreateFrameBuffer(resolution, resolution, out QColorBuffer, GL.RG32F, GL.RG, GL.FLOAT);
                pVao = CreateVao(P);
                qVao = CreateVao(Q);
                FindAABBandViewDir(P);
                //ViewDir = new Point3D(0, 0, -1);
                ViewMatrix = Matrixd.LookAt(0, 0, 0, -ViewDir.X, -ViewDir.Y, -ViewDir.Z, 0, 1, 0);
                FindCameraBB();
                ProjMatrix = Matrixd.Ortho(CamBBmin.X, CamBBmax.X, CamBBmin.Y, CamBBmax.Y, CamBBmin.Z, CamBBmax.Z);

                gl.BindFramebuffer(GL.FRAMEBUFFER, PFrameBuffer);
                gl.Viewport(0, 0, resolution, resolution);
                gl.UseProgram(programP.id);
                gl.Clear(GL.COLOR_BUFFER_BIT | GL.DEPTH_BUFFER_BIT);
                gl.UniformMatrix4fv(0, 1, false, ViewMatrix.FloatArray);
                gl.UniformMatrix4fv(1, 1, false, ProjMatrix.FloatArray);

                gl.BindVertexArray(pVao);
                gl.DrawElements(GL.TRIANGLES, 3 * P.Triangles.Length, GL.UNSIGNED_INT, IntPtr.Zero);

                gl.BindFramebuffer(GL.FRAMEBUFFER, 0);

                gl.BindFramebuffer(GL.READ_FRAMEBUFFER, PFrameBuffer);
                gl.ReadBuffer(GL.COLOR_ATTACHMENT0);
                float[] pixels = new float[2 * resolution * resolution];
                gl.ReadPixels(0, 0, resolution, resolution, GL.RG, GL.FLOAT, pixels);

                gl.BindFramebuffer(GL.READ_FRAMEBUFFER, 0);

                Ppixels = 0;
                for(int i=1; i<pixels.Length; i+=4)
                {
                    Ppixels += (int)pixels[i];
                }

                gl.BindFramebuffer(GL.FRAMEBUFFER, QFrameBuffer);

                gl.UseProgram(programQ.id);
                gl.UniformMatrix4fv(0, 1, false, ViewMatrix.FloatArray);
                gl.UniformMatrix4fv(1, 1, false, ProjMatrix.FloatArray);

                gl.ActiveTexture(GL.TEXTURE0);
                gl.BindTexture(GL.TEXTURE_2D, PColorBuffer);

                gl.BindVertexArray(qVao);

                //rc.SwapBuffers();
                rc.UnmakeCurrent();

            }
        }

        uint CreateFrameBuffer(int width, int height, out uint colorBuffer, int colorBufferInternalFormat, int colorBufferFormat, int colorBufferType)
        {
            uint frameBuffer;
            uint depthBuffer;

            gl.GenFramebuffers(1, out frameBuffer);            
            gl.GenRenderbuffers(1, out depthBuffer);
            gl.GenTextures(1, out colorBuffer);
            
            gl.BindFramebuffer(GL.FRAMEBUFFER_EXT, frameBuffer);

            gl.BindRenderbuffer(GL.RENDERBUFFER_EXT, depthBuffer);
            gl.RenderbufferStorage(GL.RENDERBUFFER_EXT, GL.DEPTH_COMPONENT, width, height);
            gl.FramebufferRenderbuffer(GL.FRAMEBUFFER_EXT, GL.DEPTH_ATTACHMENT_EXT, GL.RENDERBUFFER_EXT, depthBuffer);

            gl.BindTexture(GL.TEXTURE_2D, colorBuffer);
            gl.TexImage2D(GL.TEXTURE_2D, 0, colorBufferInternalFormat, width, height, 0, colorBufferFormat, colorBufferType, null);
            gl.GenerateMipmap(GL.TEXTURE_2D);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MIN_FILTER, GL.NEAREST);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_MAG_FILTER, GL.NEAREST);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_S, GL.CLAMP);
            gl.TexParameteri(GL.TEXTURE_2D, GL.TEXTURE_WRAP_T, GL.CLAMP);
            gl.FramebufferTexture2D(GL.FRAMEBUFFER, GL.COLOR_ATTACHMENT0, GL.TEXTURE_2D, colorBuffer, 0);
            int status = gl.CheckFramebufferStatus(GL.FRAMEBUFFER_EXT);
            gl.BindTexture(GL.TEXTURE_2D, 0);

            gl.BindFramebuffer(GL.FRAMEBUFFER_EXT, 0);

            return frameBuffer;
        }

        uint CreateVao(TriangleMesh m)
        {
            uint vao = gl.GenVertexArray();
            gl.BindVertexArray(vao);

            uint vertices = gl.GenBuffer();                                                                                    
            gl.BindBuffer(GL.ARRAY_BUFFER, vertices);                                                                     
            gl.BufferData(GL.ARRAY_BUFFER, m.Points.Length * 3 * 8, m.Points, GL.STATIC_DRAW);
            gl.EnableVertexAttribArray(0);                                                                                  
            gl.VertexAttribPointer(0, 3, GL.DOUBLE, false, 3*8, (IntPtr)0);

            uint indices = gl.GenBuffer();                                                                              
            gl.BindBuffer(GL.ELEMENT_ARRAY_BUFFER, indices);                                                            
            gl.BufferData(GL.ELEMENT_ARRAY_BUFFER, 3*m.Triangles.Length * sizeof(uint), m.Triangles, GL.STATIC_DRAW);     

            gl.BindVertexArray(0);

            return vao;
        }


        void FindCameraBB()
        {
            Vector3d[] aabb = new Vector3d[]
            {
                new Vector3d(AABBmin.X, AABBmin.Y, AABBmin.Z),
                new Vector3d(AABBmin.X, AABBmin.Y, AABBmax.Z),
                new Vector3d(AABBmin.X, AABBmax.Y, AABBmin.Z),
                new Vector3d(AABBmin.X, AABBmax.Y, AABBmax.Z),
                new Vector3d(AABBmax.X, AABBmin.Y, AABBmin.Z),
                new Vector3d(AABBmax.X, AABBmin.Y, AABBmax.Z),
                new Vector3d(AABBmax.X, AABBmax.Y, AABBmin.Z),
                new Vector3d(AABBmax.X, AABBmax.Y, AABBmax.Z),
            };

            double AABBlenX = AABBmin.X - AABBmax.X;
            double AABBlenY = AABBmin.Y - AABBmax.Y;
            double AABBlenZ = AABBmin.Z - AABBmax.Z;

            double AABBdiag = Math.Sqrt(AABBlenX * AABBlenX + AABBlenY * AABBlenY + AABBlenZ * AABBlenZ);

            aabb[0].Mult(ref ViewMatrix);
            CamBBmin.X = aabb[0].x;
            CamBBmin.Y = aabb[0].y;
            CamBBmin.Z = aabb[0].z;
            CamBBmax = CamBBmin;

            for (int i=1; i<8; i++)
            {
                aabb[i].Mult(ref ViewMatrix);

                CamBBmin.X = Math.Min(aabb[i].x, CamBBmin.X);
                CamBBmin.Y = Math.Min(aabb[i].y, CamBBmin.Y);
                CamBBmin.Z = Math.Min(aabb[i].z, CamBBmin.Z);

                CamBBmax.X = Math.Max(aabb[i].x, CamBBmax.X);
                CamBBmax.Y = Math.Max(aabb[i].y, CamBBmax.Y);
                CamBBmax.Z = Math.Max(aabb[i].z, CamBBmax.Z);
            }

            /*CamBBmin.X -= AABBdiag;
            CamBBmin.Y -= AABBdiag;
            CamBBmax.X += AABBdiag;
            CamBBmax.Y += AABBdiag;*/

            double CamBBmidZ = (CamBBmin.Z + CamBBmax.Z) / 2;
            CamBBmin.Z = CamBBmidZ - AABBdiag;
            CamBBmax.Z = CamBBmidZ + AABBdiag;
        }

        void FindAABBandViewDir(TriangleMesh m)
        {
            AABBmin = m.Points[0];
            AABBmax = m.Points[0];

            for (int i = 1; i < m.Points.Length; i++)
            {
                ViewDir += m.Normals[i];
                ViewDir.Normalize();

                AABBmin.X = Math.Min(m.Points[i].X, AABBmin.X);
                AABBmin.Y = Math.Min(m.Points[i].Y, AABBmin.Y);
                AABBmin.Z = Math.Min(m.Points[i].Z, AABBmin.Z);

                AABBmax.X = Math.Max(m.Points[i].X, AABBmax.X);
                AABBmax.Y = Math.Max(m.Points[i].Y, AABBmax.Y);
                AABBmax.Z = Math.Max(m.Points[i].Z, AABBmax.Z);
            }
        }

        Matrixd FromArray(double[] array)
        {
            Matrixd mat = new Matrixd();
            mat.m11 = array[ 0]; mat.m21 = array[ 1]; mat.m31 = array[ 2]; mat.m41 = array[ 3];
            mat.m12 = array[ 4]; mat.m22 = array[ 5]; mat.m32 = array[ 6]; mat.m42 = array[ 7];
            mat.m13 = array[ 8]; mat.m23 = array[ 9]; mat.m33 = array[10]; mat.m43 = array[11];
            mat.m14 = array[12]; mat.m24 = array[13]; mat.m34 = array[14]; mat.m44 = array[15];
            return mat;

        }

        int pass = 0;
        object lockObj = new object();
        public override bool AssignScore(Candidate c)
        {
            double distance = c.score;

            float[] pixels;
            lock (lockObj)
            {
                rc.MakeCurrent();
                Transform3D2 qmat = c.dq.getTransform2();
                Matrixd mat = new Matrixd();
                mat.m11 = qmat[0, 0]; mat.m12 = qmat[1, 0]; mat.m13 = qmat[2, 0]; mat.m14 = qmat[3, 0];
                mat.m21 = qmat[0, 1]; mat.m22 = qmat[1, 1]; mat.m23 = qmat[2, 1]; mat.m24 = qmat[3, 1];
                mat.m31 = qmat[0, 2]; mat.m32 = qmat[1, 2]; mat.m33 = qmat[2, 2]; mat.m34 = qmat[3, 2];
                mat.m41 = qmat[0, 3]; mat.m42 = qmat[1, 3]; mat.m43 = qmat[2, 3]; mat.m44 = qmat[3, 3];

                gl.BindFramebuffer(GL.FRAMEBUFFER, QFrameBuffer);
                gl.Clear(GL.DEPTH_BUFFER_BIT | GL.COLOR_BUFFER_BIT);

                gl.UniformMatrix4fv(2, 1, false, mat.FloatArray);

                gl.DrawElements(GL.TRIANGLES, 3 * Qtris, GL.UNSIGNED_INT, IntPtr.Zero);

                gl.BindFramebuffer(GL.FRAMEBUFFER, 0);

                gl.BindFramebuffer(GL.READ_FRAMEBUFFER, QFrameBuffer);
                gl.ReadBuffer(GL.COLOR_ATTACHMENT0);
                pixels = new float[2 * resolution * resolution];
                gl.ReadPixels(0, 0, resolution, resolution, GL.RG, GL.FLOAT, pixels);

                //rc.SwapBuffers();
                rc.UnmakeCurrent();


                Qpixels = 0;
                double sqrdist = 0;
                for (int i = 0; i < pixels.Length; i += 2)
                {
                    Qpixels += (int)pixels[i + 1];
                    sqrdist += pixels[i];
                }

                sqrdist /= Qpixels;
                c.score = 1 - sqrdist;

                double overlapRatio = (double)Qpixels / Ppixels;

                if (overlapRatio < minOverlapRatio)
                {
                    c.score *= overlapRatio / minOverlapRatio;
                }

                if (Qpixels == 0)
                {
                    c.score = 0;
                    /*if (distance < 5)
                        Console.WriteLine($"Score 0 for distance {distance}");*/
                }

                //if(distance<10)
                   // Console.WriteLine($"Score {c.score} for distance {distance}: {Qpixels}/{Ppixels}");

                if (c.score < minAcceptScore)
                    return false;
                return true;
            }

        }
    }
}
