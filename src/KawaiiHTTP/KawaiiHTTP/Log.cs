using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KawaiiHTTP
{
    public static class Log
    {
        public static bool Enabled { get; set; } = true;
        public static bool Debugging { get; set; }
        public static void m(string message) {
            Log.m(message, new object[0]);
        }
        public static void m(string message, params object[] args) {
            if (!Log.Enabled) { return; }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine(message, args);
            Console.ResetColor();
        }
        public static void d(string message)
        {
            Log.d(message, new object[0]);
        }
        public static void d(string message, params object[] args)
        {
            if (!Log.Enabled) { return; }
            if (!Log.Debugging) { return;  }
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine(message, args);
            Console.ResetColor();
        }
        public static void e(string message)
        {
            if (!Log.Enabled) { return; }
            Log.e(message, new object[0]);
        }
        public static void e(string message, params object[] args)
        {
            if (!Log.Enabled) { return; }
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message, args);
            Console.ResetColor();
        }
    }
}
