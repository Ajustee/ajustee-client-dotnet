using System;
using System.Diagnostics;
using System.Reflection;

namespace Ajustee.Tools
{
    internal class ATLConsoleTraceListener : TraceListener
    {
        public readonly object m_SyncRoot = new object();

        public ATLConsoleTraceListener()
            : base()
        {
            typeof(AjusteeClient).Assembly.GetType("Ajustee.ATL").GetProperty("Enabled", BindingFlags.Static | BindingFlags.Public).SetValue(null, true);
        }

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
