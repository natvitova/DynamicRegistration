using System.IO;
using System.Globalization;
using Framework;

namespace ClusterRegister
{
    public class ObjSaver
    {

        private string fileName = "mesh.obj";

        public string FileName
        {
            get { return fileName; }
            set { fileName = value; }
        }

        bool writeUnusedVertices = true;
        bool saveNormals = true;

        public bool SaveNormals
        {
            get { return saveNormals; }
            set { saveNormals = value; }
        }

        public void Execute(TriangleMesh mesh)
        {
            StreamWriter sw = new StreamWriter(fileName);

            Point3D[] points = mesh.Points;

            Triangle[] triangles = mesh.Triangles;

            bool[] used = new bool[mesh.Points.Length];
            foreach (Triangle t in triangles)
            {
                used[t.V1] = true;
                used[t.V2] = true;
                used[t.V3] = true;
            }

            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            nfi.NumberGroupSeparator = "";

            for (int i = 0; i < points.Length; i++)
            {
                if (used[i]||writeUnusedVertices)
                {
                    Point3D p = (Point3D)points[i];
                    sw.WriteLine("v " + p.X.ToString(nfi) + " "
                        + p.Y.ToString(nfi) + " " + p.Z.ToString(nfi) + " " + mesh.AdditionalVertexData[i]);
                }
                
            }

            if (saveNormals)
            {
                Point3D[] normals = mesh.Normals;
                if (normals.Length > 0)
                {                    
                    for (int i = 0; i < points.Length; i++)
                    {
                        if (used[i] || writeUnusedVertices)
                        {
                            Point3D n = normals[i];
                            sw.WriteLine("vn " + n.X.ToString(nfi) + " "
                                + n.Y.ToString(nfi) + " " + n.Z.ToString(nfi));
                        }

                    }
                }
            }

            for (int i = 0; i < triangles.Length; i++)
            {
                Triangle t = (Triangle)triangles[i];
                sw.WriteLine("f " + (t.V1 + 1).ToString() +" "+ (t.V2 + 1).ToString() +" "+ (t.V3 + 1).ToString());
            }
            sw.Close();
        }
    }
}
