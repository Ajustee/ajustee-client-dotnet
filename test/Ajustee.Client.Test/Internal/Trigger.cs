using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Ajustee
{
    internal class Trigger
    {
        private readonly Dictionary<string, bool> m_Triggers = new Dictionary<string, bool>();

        public async Task WaitAsync(string trigger)
        {
            await Task.Delay(1);
            SpinWait.SpinUntil(() =>
            {
                lock (m_Triggers)
                {
                    if (m_Triggers.TryGetValue(trigger, out var _released) && _released)
                        return true;
                    return false;
                }
            });
        }

        public void Release(string trigger)
        {
            if (string.IsNullOrEmpty(trigger)) return;
            lock (m_Triggers)
            {
                m_Triggers[trigger] = true;
            }
        }
    }
}
