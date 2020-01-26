using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Ajustee.Tools
{
    public class AtlConsoleTraceListener : TraceListener
    {
        public readonly object m_SyncRoot = new object();

        public bool TryTrace(ref string message)
        {
            if (message.StartsWith("ATL: "))
            {
                message = message.Substring(5);
                return true;
            }
            return false;
        }

        public override void Write(string message)
        {
            if (TryTrace(ref message))
            {
                lock (m_SyncRoot)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(message);
                    Console.Write("> ");
                    Console.ResetColor();
                }
            }
        }

        public override void WriteLine(string message)
        {
            if (TryTrace(ref message))
            {
                lock (m_SyncRoot)
                {
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.WriteLine(message);
                    Console.Write("> ");
                    Console.ResetColor();
                }
            }
        }
    }
}
