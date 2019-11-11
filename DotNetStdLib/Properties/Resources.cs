using System.Globalization;
using System.Resources;

namespace Ajustee
{
    internal class Resources
    {
        #region Private fields region

        private static ResourceManager m_ResourceManager;
        private static CultureInfo m_ResourceCulture;

        #endregion

        #region Private properties region

        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        internal static ResourceManager ResourceManager
        {
            get
            {
                if (m_ResourceManager is null)
                {
#if NETSTANDARD
                    var _assembly = System.Reflection.IntrospectionExtensions.GetTypeInfo(typeof(Resources)).Assembly;
#else
                    var _assembly = typeof(Resources).Assembly;
#endif
                    m_ResourceManager = new ResourceManager("Ajustee.Properties.Resources", _assembly);
                }
                return m_ResourceManager;
            }
        }

        #endregion

        #region Public methods region

        /// <summary>
        /// Overrides the current thread's CurrentUICulture property for all resource lookups using this strongly typed resource class.
        /// </summary>
        public static CultureInfo Culture
        {
            get { return m_ResourceCulture; }
            set { m_ResourceCulture = value; }
        }

        /// <summary>
        /// Gets stringified value of the specified resource name.
        /// </summary>
        /// <param name="resourceName">A resouirce name to get string value.</param>
        /// <returns></returns>
        internal static string GetString(string resourceName)
        {
            return ResourceManager.GetString(resourceName, m_ResourceCulture);
        }

        #endregion
    }
}
