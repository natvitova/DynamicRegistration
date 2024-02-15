using System;
using System.IO;
using System.Globalization;
using System.Collections.Generic;
using Framework;

namespace ClusterRegister
{
    /// <summary>
    /// Obj triangle mesh file loader module.
    /// </summary>
    public class ObjLoader
    {
        bool flipNormals = false;
        private TriangleMesh output;       

        /// <summary>
        /// Loads the input file.
        /// I property values remain unchanged since the last run, then the file is not reloaded, unless the Realod property is set to true.
        /// </summary>
        public TriangleMesh Execute(string fn)
        {                       
            StreamReader sr;
            sr = new StreamReader(fn);
            List<Point3D> vertices = new List<Point3D>();
            List<Triangle> triangles = new List<Triangle>();
            List<String> additionalData = new List<string>();
            
            Point3D point;
            Triangle triangle;
            NumberFormatInfo nfi = new NumberFormatInfo();
            nfi.NumberDecimalSeparator = ".";
            nfi.NumberGroupSeparator = ",";
            
            string line = sr.ReadLine();
            while (line != null)
            {
                line = line.Trim();
                if (line.Length<=2)
                {
                    line = sr.ReadLine();
                    continue;
                }

                //parsing of a vertex
                //if (line.StartsWith("v "))
                if (line[0] == 'v')
                {
                    if (line[1] == ' ')
                    {
                        string[] coords = line.Split(new char[] { ' ' }, 5, StringSplitOptions.RemoveEmptyEntries);

                        if (coords.Length < 4)
                        {
                            line = sr.ReadLine();
                            continue;
                        }
                            

                        double c1 = double.Parse(coords[1], nfi);
                        double c2 = double.Parse(coords[2], nfi);
                        double c3 = double.Parse(coords[3], nfi);

                        point = new Point3D(c1, c2, c3);
                        vertices.Add(point);
                        if (coords.Length > 4)
                            additionalData.Add(coords[4]);
                        else
                            additionalData.Add("");
                    }
                }

                // parsing of a triangle
                //if ((line.StartsWith("f ")) || (line.StartsWith("fo ")) || (line.StartsWith("f\t")))
                else
                {
                    if (line[0] == 'f')
                        if ((line[1] == ' ') || (line[1] == 'o') || (line[1] == '\t'))
                        {
                            string[] indices = line.Split(new char[] { ' ', '\t' }, 5, StringSplitOptions.RemoveEmptyEntries);

                            string[] parts = indices[1].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            int v1 = int.Parse(parts[0]) - 1;

                            parts = indices[2].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            int v2 = int.Parse(parts[0]) - 1;

                            parts = indices[3].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                            int v3 = int.Parse(parts[0]) - 1;

                            if (flipNormals)
                                triangle = new Triangle(v1, v3, v2);
                            else
                                triangle = new Triangle(v1, v2, v3);

                            if (indices.Length == 4)
                                triangles.Add(triangle);

                            if (indices.Length == 5)
                            {
                                parts = indices[4].Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                                v2 = int.Parse(parts[0]) - 1;

                                if (flipNormals)
                                    triangle = new Triangle(v1, v2, v3);
                                else
                                    triangle = new Triangle(v1, v3, v2);
                                triangles.Add(triangle);
                            }
                        }
                }
                line = sr.ReadLine();                
            }

            // creating an output Mesh instance
            output = new TriangleMesh();
            output.Points = vertices.ToArray();
            output.Triangles = triangles.ToArray();
            output.AdditionalVertexData = additionalData.ToArray();
            return (output);
        }// Execute

    }//ObjLoader
}
