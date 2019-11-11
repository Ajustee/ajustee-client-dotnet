
namespace Ajustee
{
    internal class JsonSerializerFactory
    {
        #region Public methods region

        public static IJsonSerializer Create()
        {
#if SJSON
            return new SystemJsonSerializer();
#endif

#if NJSON
            return new NewtonsoftJsonSerializer();
#endif
        }

        #endregion
    }
}
