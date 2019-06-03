using BaseComponent;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SatCore
{
    public class Debug
    {
        static Dictionary<string, int> Counter { get; } = new Dictionary<string, int>();
        static Dictionary<string, bool> Flag { get; } = new Dictionary<string, bool>();
        static Stopwatch Stopwatch { get; } = new Stopwatch();

        [Conditional("DEBUG")]
        public static void AddCount(string key)
        {
            object sync = new object();
            lock (sync)
            {
                if (!Counter.ContainsKey(key))
                    Counter[key] = 0;
                Counter[key] += 1;
            }
        }

        [Conditional("DEBUG")]
        public static void PrintCount(string key)
        {
            object sync = new object();
            lock (sync)
            {
                if (!Counter.ContainsKey(key))
                    Counter[key] = 0;
                Logger.Debug("Count " + key + ": " + Counter[key]);
                Counter[key] = 0;
            }
        }

        [Conditional("DEBUG")]
        public static void ResetTime()
        {
            if (!Stopwatch.IsRunning)
                Stopwatch.Start();
            else
                Stopwatch.Restart();
        }

        [Conditional("DEBUG")]
        public static void PrintTime(string message = "")
        {
            if (!Stopwatch.IsRunning)
                Stopwatch.Start();
            Stopwatch.Stop();
            Logger.Debug(message + "Time: " + Stopwatch.ElapsedMilliseconds + "ms");
            Stopwatch.Restart();
        }

        [Conditional("DEBUG")]
        public static void PrintTimeWithFlag(string message = "")
        {
            if (!Flag.ContainsKey("Timer") || !Flag["Timer"])
                return;
            if (!Stopwatch.IsRunning)
                Stopwatch.Start();
            Stopwatch.Stop();
            Logger.Debug(message + "Time: " + Stopwatch.ElapsedMilliseconds + "ms");
            Stopwatch.Restart();
        }

        [Conditional("DEBUG")]
        public static void SetFlag(string key, bool flag)
        {
            Flag[key] = flag;
        }
    }
}