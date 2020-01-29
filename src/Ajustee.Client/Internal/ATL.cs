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
                {
                    lock (Messages)
                        Messages.Add(message);
                }
            }

            public override void WriteLine(string message)
            {
                if (TryTrace(ref message))
                {
                    lock (Messages)
                        Messages.Add(message);
                }
            }
        }

        private static AjusteeTraceListener GetListener()
        {
            if (m_Listener == null)
            {
                foreach (var _listener in Trace.Listeners)
                {
                    if (_listener is AjusteeTraceListener)
                    {
                        m_Listener = (AjusteeTraceListener)_listener;
                        break;
                    }
                }
            }
            return m_Listener;
        }
#endif

        private static bool m_Enabled;
        private static readonly object m_SyncRoot = new object();

#if !NETSTANDARD1_3
        private static AjusteeTraceListener m_Listener;
#endif

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
                            m_Listener = null;
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
            return (IList<string>)GetListener()?.Messages?.AsReadOnly() ?? new string[0];
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
    }
}
