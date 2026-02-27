using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MapMaker.Core.Logger;

namespace MapMaker.Editor.Logging
{
    internal class DebugLogger : ILogger
    {
        public void Info(string message)
       => Debug.WriteLine($"[INFO] {message}");

        public void Warning(string message)
            => Debug.WriteLine($"[WARN] {message}");

        public void Error(string message)
            => Debug.WriteLine($"[ERROR] {message}");
    }
}
