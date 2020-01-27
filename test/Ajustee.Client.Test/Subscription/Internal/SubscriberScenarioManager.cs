
namespace Ajustee
{
    internal class SubscriberScenarioManager : ScenarioManager
    {
        public SubscriberScenarioManager(IAjusteeClient client)
            : base(client, new FakeSocketServer(client))
        { }
    }
}
