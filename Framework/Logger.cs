using System;

namespace Framework
{
    public static class Logger
    {
        public static bool mute;

        public static void Log()
        {
            Console.WriteLine();
        }

        public static void Log(object line)
        {
            Log(line.ToString(), null);
        }

        public static void Log(string line)
        {
            Log(line, null);
        }

        public static void Log(string line, object objects)
        {
            if (!mute) {
                Console.WriteLine(line, objects);
            }
        }

        public static void Log(string line, params object[] objects)
        {
            if(!mute)
            {
                Console.WriteLine(line, objects);
            }
        }
    }
}
