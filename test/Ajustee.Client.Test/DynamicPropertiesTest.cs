using System;

#if XUNIT
using Xunit;
#elif NUNIT
using NUnit.Framework;
using Fact = NUnit.Framework.TestAttribute;
using InlineData = NUnit.Framework.TestCaseAttribute;
#endif

namespace Ajustee
{
    public class DynamicPropertiesTest
    {
        //[Fact]
        //public void ReflectProperties()
        //{
        //    var _properties = Helper.ReflectProperties(Tuple.Create("key_path", ConfigKeyType.DateTime, DateTime.Today));
        //}
    }
}
