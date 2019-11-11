
namespace Ajustee
{
    internal class ApiRequestFactory
    {
        #region Public methods region

        public static IApiRequest Create()
        {
#if RWEB
            return new ApiWebRequest();
#endif

#if RHTTP
            return new ApiHttpRequest();
#endif
        }

        #endregion
    }
}
