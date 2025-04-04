using System;

namespace UnityEngine
{
    public class Debug
    {
        public static void LogError(object message) => Console.WriteLine("Error: " + message);
    }
}
