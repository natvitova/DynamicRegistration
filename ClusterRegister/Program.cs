using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Framework;

namespace ClusterRegister
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.WriteLine("Welcome, this is an implementation of the algorithm described in Automatic registration of 3D point cloud sequences, GRAPP 2024\n(c) University of West Bohemia, lvasa@kiv.zcu.cz");
            string errorLine = "There are two parameters expected - the path to the source data and the path to the target data.\nEx: %~dp0\\ClusterRegister\\bin\\Release\\match.exe C:\\Users\\user\\Documents\\source C:\\Users\\user\\Documents\\target\nThe names of the files in the source folder should match the names of the corresponiding files in the target folder.";

            string sourcePath = "";
            string targetPath = "";
            try
            {
                sourcePath = args[0];
                targetPath = args[1];
            }
            catch (Exception ex)
            {
                Console.WriteLine(errorLine);
                Console.ReadKey();
                System.Environment.Exit(1);
            }

            IOrderedEnumerable<string> filesSource = null;
            IOrderedEnumerable<string> filesTarget = null;

            try 
            {
                filesSource = Directory.GetFiles(sourcePath).OrderBy(file => Regex.Replace(Path.GetFileName(file), @"\d+", match => match.Value.PadLeft(4, '0')));
                filesTarget = Directory.GetFiles(sourcePath).OrderBy(file => Regex.Replace(Path.GetFileName(file), @"\d+", match => match.Value.PadLeft(4, '0')));
            }
            catch 
            {
                Console.WriteLine(errorLine);
                Console.ReadKey();
                System.Environment.Exit(1);
            }

            
            if (filesSource != null && filesTarget != null)
            {
                string[] pathsSource = filesSource.ToArray();
                string[] pathsTarget = filesTarget.ToArray();
                int numberOfFrames = pathsSource.Length;
                //numberOfFrames = 5;

                TriangleMesh[] sequenceSource = new TriangleMesh[numberOfFrames];
                TriangleMesh[] sequenceTarget = new TriangleMesh[numberOfFrames];
                LoadSequences(numberOfFrames, 0, sequenceSource, sequenceTarget, pathsSource, pathsTarget);

                string startLine = "\n\n___________________STARTING THE ALGORITHM______________________________";
                Console.WriteLine(startLine);
                Transform3D2 t = ComputeTransformation(sequenceSource, sequenceTarget);
                Console.WriteLine("Resulting transformation:\n" + t.ToString());

                Console.WriteLine("My job is done here.");
                Console.WriteLine(new string('_', startLine.Length));
                Console.ReadKey();
            }
        }

        static Transform3D2 ComputeTransformation(TriangleMesh[] sequenceSource, TriangleMesh[] sequenceTarget)
        {
            ClusterRegistration test = new ClusterRegistration();

            //__________________________________COMPUTE TRANSFORMATION_____________________________________
            int clusterIndex;
            ElapsedTime time = new ElapsedTime();
            Transform3D2 t = test.Execute(sequenceTarget, sequenceSource, out clusterIndex, time);
            return t;
        }

        static void LoadSequences(int numberOfMeshes, int index, TriangleMesh[] sequenceSource, TriangleMesh[] sequenceTarget, string[] pathsSource, string[] pathsTarget)
        {
            TriangleMesh meshTarget;
            TriangleMesh meshSource;

            //__________________________________LOAD SEQUENCES_____________________________________________
            Stopwatch s = Stopwatch.StartNew();

            ObjLoader loader = new ObjLoader();
            for (int i = 0; i < numberOfMeshes; i++)
            {
                string fileName = Path.GetFileName(pathsSource[index + i]);
                string loading = string.Format("{0} ... loading", fileName);

                Console.WriteLine(loading + " - source");
                meshSource = loader.Execute(pathsSource[index + i]);
                Console.WriteLine(new string(' ', loading.Length) + " - target");
                meshTarget = loader.Execute(pathsTarget[index + i]);

                sequenceSource[i] = meshSource;
                sequenceTarget[i] = meshTarget;

                Console.WriteLine(new string(' ', fileName.Length) + " ... computing normals");
                sequenceSource[i].ComputeNormals();
                sequenceTarget[i].ComputeNormals();
            }
            s.Stop();
            Console.WriteLine("Loading meshes & normals computation done in {0} ms", s.ElapsedMilliseconds);
        }

        static void SaveMesh(string name, TriangleMesh mesh)
        {
            ObjSaver saver = new ObjSaver();
            saver.FileName = name;
            saver.SaveNormals = false;
            saver.Execute(mesh);
        }
    }
}
