using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Visualization
{
    public partial class Form1 : Form
    {
        private Bitmap m_Canvas;
        double[,] densitiesR, densitiesG, densitiesB;
        double[,] maticeX;


        public Form1(double[,] matrix, string fn, int res, double spread, double radius, double[] q)

        {

            InitializeComponent();

            Draw(matrix, fn, res, spread, radius, q);

        }


        public void Draw(double[,] maticeA, string fn, int res, double spread, double radius, double[] q)

        {
            int w = res;
            int h = res;
            int PAD = 100;
            m_Canvas = new Bitmap(w, h);
            densitiesR = new double[w, h];
            densitiesG = new double[w, h];
            densitiesB = new double[w, h];


            //double[,] maticeA = nacteniMatice("C:\\Users\\Depot\\Desktop\\výzkum\\dist5000.txt");
            Console.WriteLine("Matice načtena");
            maticeX = mds(maticeA);
            Console.WriteLine("X nalezeno");
            drawDensities(fn, spread, radius, w, h, PAD, q);
        }

        private void drawDensities(string fn, double spread, double radius, int w, int h, int PAD, double[] q)
        {
            double maxY = getMaxY(maticeX);
            double minX = getMinX(maticeX);
            double scalex = (double)(w - 2 * PAD) / (getMaxX(maticeX) - minX);
            double scaley = (double)(h - 2 * PAD) / (maxY - getMinY(maticeX));
            double scale;
            if (scalex >= scaley)
            {
                scale = scaley;
            }
            else
            {
                scale = scalex;
            }
            //makeBlob(PAD + scale * (-minX), PAD + scale * (maxY), Color.Black, 100, scale, spread, radius);
            for (int i = 0; i < maticeX.GetLength(0); i++)
            {
                double x = PAD + scale * (maticeX[i, 0] - minX);
                double y = PAD + scale * (-maticeX[i, 1] + maxY);
                makeBlob(x, y, Color.Red, 100, scale, spread, radius, q[i]);
            }

            /*for (int i = 0;i<w;i++)
                for (int j = 0;j<h;j++)
                {
                    densitiesR[i, j] = Math.Log(densitiesR[i, j]+1);
                    densitiesG[i, j] = Math.Log(densitiesG[i, j]+1);
                    densitiesB[i, j] = Math.Log(densitiesB[i, j]+1);
                }*/

            double max = 0;
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                {
                    max = Math.Max(max, densitiesR[i, j]);
                    max = Math.Max(max, densitiesG[i, j]);
                    //max = Math.Max(max, densitiesB[i, j]);
                }
            for (int i = 0; i < w; i++)
                for (int j = 0; j < h; j++)
                    m_Canvas.SetPixel(i, j, Color.FromArgb((int)(255-densitiesR[i, j] / max * 255), 
                        (int)(255 - densitiesG[i, j] / max * 255), 
                        (int)(Math.Max(0, 255 - densitiesB[i, j] / max * 255))));

            SetCanvasAsImage();
            m_Canvas.Save(fn);
        }

        public void makeBlob(double x, double y, Color col, int size, double scale, double spread, double radius, double c)
        {
            //m_Canvas.SetPixel((int)x, (int)y, col);
            for (int i = -size; i < size; i++)
            {
                for (int j = -size; j < size; j++)
                {
                    double dist = Math.Sqrt(i * i + j * j) / scale;
                    double density = Math.Exp(-spread * dist / radius);
                    //double color = img.getRGB((int)x + i, (int)y - j) + col.getRGB() * 0.7 * Math.exp(-0.005 * Math.sqrt((i * i) + (j * j)));
                    // int c = (int)color;
                    densitiesR[(int)x + i, (int)y - j] += density*(1-c);
                    densitiesG[(int)x + i, (int)y - j] += density * (c);
                    densitiesB[(int)x + i, (int)y - j] += density;
                }
            }
            
        }
        private double getMinX(double[,] matice)
        {
            double min = Double.MaxValue;
            for (int i = 0; i < matice.GetLength(0); i++)
            {
                if (matice[i,0] < min)
                    min = matice[i,0];
            }
            return min;
        }


        private double getMinY(double[,] matice)
        {
            double min = Double.MaxValue;
            for (int i = 0; i < matice.GetLength(0); i++)
            {
                if (matice[i,1] < min)
                    min = matice[i,1];
            }
            return min;
        }


        private double getMaxY(double[,] matice)
        {
            double max = -Double.MaxValue;
             for (int i = 0; i < matice.GetLength(0); i++)
            {
                if (matice[i,1] > max)
                    max = matice[i,1];
            }
            return max;
        }

        private double getMaxX(double[,] matice)
        {
            double max = -Double.MaxValue;
            for (int i = 0; i < matice.GetLength(0); i++)
            {
                if (matice[i,0] > max)
                    max = matice[i,0];
            }
            return max;
        }
        static double[,] soucinMatic(double[,] A, double[,] B)
        {
            double[,] soucin = new double[A.GetLength(0), B.GetLength(1)];
            for (int i = 0; i < soucin.GetLength(0); i++)
            {
                for (int j = 0; j < soucin.GetLength(1); j++)
                {

                    soucin[i, j] = 0;
                    for (int k = 0; k < soucin.GetLength(1); k++)
                        soucin[i, j] += A[i, k] * B[k, j];

                }
            }
            return soucin;
        }
        static double[] soucinMatVect(double[,] A, double[] B)
        {
            double[] soucin = new double[B.GetLength(0)];
            for (int i = 0; i < soucin.GetLength(0); i++)
            {
                soucin[i] = 0;
                for (int k = 0; k < soucin.GetLength(0); k++)
                    soucin[i] += A[i, k] * B[k];
            }
            return soucin;
        }
        static double[] soucinVectMat(double[] B,double[,] A )
        {
            double[] soucin = new double[B.GetLength(0)];
            for (int i = 0; i < soucin.GetLength(0); i++)
            {
                soucin[i] = 0;
                for (int k = 0; k < soucin.GetLength(0); k++)
                    soucin[i] += B[k]*A[k,i];
            }
            return soucin;
        }
        static double[,] rozdilMatic(double[,] A, double[,] B)
        {
            double[,] soucet = new double[A.GetLength(0), A.GetLength(1)];
            for (int i = 0; i < soucet.GetLength(0); i++)
                for (int j = 0; j < soucet.GetLength(1); j++)
                    soucet[i, j] = A[i, j] - B[i, j];
            return soucet;
        }
        static void vypisMatice(double[,] matice)
        {

            for (int i = 0; i < matice.GetLength(0); i++)
            {
                for (int j = 0; j < matice.GetLength(1); j++)
                    Console.Write(matice[i, j] + "   ");
                Console.WriteLine();
            }
            Console.WriteLine();
        }
        static double[,] nacteniMatice(string soubor)
        {
            List<string> radky = new List<string>();
            FileStream fs = new FileStream(soubor, FileMode.Open);
            StreamReader cernoch = new StreamReader(fs);
            string lajna = cernoch.ReadLine();
            while (lajna != null)
            {
                radky.Add(lajna);
                lajna = cernoch.ReadLine();
            }
            int pocetRadku = radky.Count();
            double[,] matice = new double[pocetRadku, pocetRadku];
            for (int i = 0; i < pocetRadku; i++)
            {
                string[] prvky = radky[i].Split();
                for (int j = 0; j < prvky.Length - 1; j++)
                {
                    matice[i, j] = double.Parse(prvky[j], System.Globalization.NumberStyles.Float, System.Globalization.NumberFormatInfo.InvariantInfo);
                }
            }

            return matice;

        }
        private static double[,] mds(double[,] maticeA)
        {
            for (int i = 0; i < maticeA.GetLength(0); i++)
            {
                for (int j = 0; j < maticeA.GetLength(1); j++)
                    maticeA[i, j] = -0.5 * maticeA[i, j] * maticeA[i, j];
            }
                    Console.WriteLine("Umocněno");

            double[] J = new double[maticeA.GetLength(0)];
            for (int i = 0; i < J.GetLength(0); i++)
            {
                        J[i] = -(double)1 / J.GetLength(0);    
            }
            double[] v = soucinMatVect(maticeA, J);
            double[,] B = new double[maticeA.GetLength(0), maticeA.GetLength(0)];
            for (int i = 0; i < maticeA.GetLength(0); i++)
            {
                for (int j = 0; j < maticeA.GetLength(1); j++)
                    B[i, j] = v[i]+maticeA[i,j];
            }
            double[] h = soucinVectMat(J,B);
            for (int i = 0; i < maticeA.GetLength(0); i++)
            {
                for (int j = 0; j < maticeA.GetLength(1); j++)
                {
                    B[i, j] = h[j] + B[i, j];
                }
            }

            Console.WriteLine("Nalezena B");
            double[] b = new double[maticeA.GetLength(0)];
            double[] bo = null;
            for (int i = 0; i < J.GetLength(0); i++)
            {
                b[i] = 1;
            }
            for (int i = 0; i < 50; i++)
            {
                

                b = soucinMatVect(B, b);
                double souc = 0;
                for (int j = 0; j < b.GetLength(0); j++)
                {
                    souc += b[j] * b[j];
                }

                for (int j = 0; j < b.GetLength(0); j++)
                {
                    b[j] = b[j] / (Math.Sqrt(souc));
                }

                if (bo!=null)
                {
                    double dp = 0;
                    for (int j = 0; j < b.Length; j++)
                        dp += b[j] * bo[j];
                    double angle = Math.Acos(dp) / Math.PI * 180;
                    Console.WriteLine("Angle: {0}", angle);
                    if (angle < 0.00001)
                        break;
                }

                bo = b;
            }
            double sum = 0;
            for (int j = 0; j < b.GetLength(0); j++)
            {
                sum += B[j, 1] * b[j];
            }
            double lambda = sum / b[1];
                    Console.WriteLine("Nalezeno 1. vl. č");
            double[,] soucin = new double[B.GetLength(0), B.GetLength(0)];
            for (int i = 0; i < soucin.GetLength(0); i++)
            {
                for (int k = 0; k < soucin.GetLength(0); k++)
                    soucin[i, k] = b[i] * b[k] * lambda;
            }
            double[,] C = rozdilMatic(B, soucin);
            double[] b2 = new double[maticeA.GetLength(0)];
            for (int i = 0; i < J.GetLength(0); i++)
            {
                b2[i] = 1;
            }
            bo = null;
            for (int i = 0; i < 50; i++)
            {
                b2 = soucinMatVect(C, b2);
                double souc = 0;
                for (int j = 0; j < b.GetLength(0); j++)
                {
                    souc += b2[j] * b2[j];
                }

                for (int j = 0; j < b.GetLength(0); j++)
                {
                    b2[j] = b2[j] / (Math.Sqrt(souc));
                }

                if (bo != null)
                {
                    double dp = 0;
                    for (int j = 0; j < b.Length; j++)
                        dp += b2[j] * bo[j];
                    double angle = Math.Acos(dp) / Math.PI * 180;
                    Console.WriteLine("Angle: {0}", angle);
                    if (angle < 0.00001)
                        break;
                }

                bo = b2;
            }
            double suma = 0;
            for (int j = 0; j < b.GetLength(0); j++)
            {
                suma += C[j, 1] * b2[j];
            }
            double lambda2 = suma / b2[1];
                    Console.WriteLine("Nalezeno 2. vl. č");
            double[,] E = new double[B.GetLength(0), 2];
            for (int i = 0; i < E.GetLength(0); i++)
            {
                E[i, 0] = b[i];
                E[i, 1] = b2[i];
            }
            double[,] A = new double[2, 2];
            A[0, 0] = Math.Sqrt(lambda);
            A[0, 1] = 0;
            A[1, 0] = 0;
            A[1, 1] = Math.Sqrt(lambda2);
            double[,] X = soucinMatic(E, A);
            return X;
}



public void SetCanvasAsImage()

        {

            pictureBox1.Image = m_Canvas;

        }

        protected override void OnPaint(PaintEventArgs e)

        {

            

        }





    }

}


