using System.Collections.Generic;
using System.Diagnostics;

namespace Ajustee
{
    internal static class ATL
    {
#if !NETSTANDARD1_3
        private class AjusteeTraceListener : TraceListener
        {
            public readonly List<string> Messages = new List<string>();

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
                    Messages.Add(message);
            }

            public override void WriteLine(string message)
            {
                if (TryTrace(ref message))
                    Messages.Add(message);
            }
        }

        private static AjusteeTraceListener GetListener()
        {
            foreach (var _listener in Trace.Listeners)
            {
                if (_listener is AjusteeTraceListener)
                    return (AjusteeTraceListener)_listener;
            }
            return null;
        }
#endif

        private static bool m_Enabled;
        private static readonly object m_SyncRoot = new object();

        public static bool Enabled
        {
            get { return m_Enabled; }
            set
            {
#if NETSTANDARD1_3
                m_Enabled = false;
#else
                if (m_Enabled != value)
                {
                    lock (m_SyncRoot)
                    {
                        if (m_Enabled != value)
                        {
                            if (value)
                            {
                                if (GetListener() == null)
                                    Trace.Listeners.Add(new AjusteeTraceListener());
                            }
                            else
                            {
                                var _listener = GetListener();
                                if (_listener != null)
                                    Trace.Listeners.Remove(_listener);
                            }
                            m_Enabled = value;
                        }
                    }
                }
#endif
            }
        }

        public static IList<string> GetMessages()
        {
#if NETSTANDARD1_3
            throw new System.NotSupportedException();
#else
            return GetListener()?.Messages;
#endif
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string message)
        {
#if !NETSTANDARD1_3
            if (m_Enabled)
                Trace.WriteLine(message, "ATL");
#endif
        }

        [Conditional("DEBUG")]
        public static void WriteLine(string format, params object[] args)
        {
#if !NETSTANDARD1_3
            if (m_Enabled)
                Trace.WriteLine(string.Format(format, args), "ATL");
#endif
        }
    }
}
