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
            if (string.IsNullOrEmpty(trigger)) return;

            await Task.Delay(1);
            SpinWait.SpinUntil(() =>
            {
                lock (m_Triggers)
                {
                    return m_Triggers.TryGetValue(trigger, out var _released) && _released;
                }
            });
        }

        public async Task WaitAsync(string[] triggers)
        {
            if (triggers == null || triggers.Length == 0) return;

            await Task.Delay(1);
            SpinWait.SpinUntil(() =>
            {
                lock (m_Triggers)
                {
                    bool _allReleased = true;
                    foreach (var _trigger in triggers)
                        _allReleased &= m_Triggers.TryGetValue(_trigger, out var _released) && _released;
                    return _allReleased;
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
