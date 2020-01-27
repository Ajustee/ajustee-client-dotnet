
namespace Ajustee
{
    public static class ATLAssert
    {
        public class AssertContext
        {
            public AssertContext(string message, int index)
            {
                Message = message;
                Index = index;
            }
            public string Message { get; }
            public int Index { get; }
        }

        private static AssertContext ExpectNextBy(AssertContext context, string message)
        {
            var _startIndex = (context?.Index ?? -1) + 1;
            var _messages = ATL.GetMessages();
            var _index = -1;
            for (int i = _startIndex; i < _messages.Count; i++)
            {
                if (_messages[i] == message)
                {
                    _index = i;
                    break;
                }
            }
            if (_index == -1)
            {
                var _failMessage = $"Not found message '{message}'{(context == null ? null : $" next by '{context.Message}'")}.";
#if XUNIT
                throw new Xunit.Sdk.XunitException(_failMessage);
#elif NUNIT
                throw new NUnit.Framework.AssertionException(_failMessage);
#else
                throw new System.Exception(_failMessage);
#endif
            }
            return new AssertContext(message, _index);
        }

        public static AssertContext Expect(string message)
        {
            return ExpectNextBy(null, message);
        }

        public static AssertContext Expect(string format, params object[] args)
        {
            return Expect(string.Format(format, args));
        }

        public static AssertContext NextBy(this AssertContext context, string message)
        {
            return ExpectNextBy(context, message);
        }

        public static AssertContext NextBy(this AssertContext context, string format, params object[] args)
        {
            return NextBy(context, string.Format(format, args));
        }
    }
}
