using System;
using System.Collections.Generic;

namespace Ajustee
{
    /// <summary>
    /// Configuration to use in <see cref="AjusteeClient"/> class.
    /// </summary>
    public class AjusteeConnectionSettings
    {
        #region Public properties region

        /// <summary>
        /// Gets or sets API url. Value is optional.
        /// </summary>
        public Uri ApiUrl { get; set; }

        /// <summary>
        /// Gets or sets application id. Values is manditory.
        /// </summary>
        public string ApplicationId { get; set; }

        /// <summary>
        /// Gets or sets default path to get configurations. Value is optional.
        /// </summary>
        public string DefaultPath { get; set; }

        /// <summary>
        /// Gets or sets default properties to get configurations. Value is optional.
        /// </summary>
        public IDictionary<string, string> DefaultProperties { get; set; }

        /// <summary>
        /// Gets or sets tracker id.
        /// </summary>
        public object TrackerId { get; set; }

        #endregion
    }
}
