using System;
using Newtonsoft.Json;
using OpenStack.Serialization;

namespace OpenStack.Compute.v2_1
{
    /// <summary>
    /// Specifies how to connect a console to a <see cref="Server"/>.
    /// </summary>
    [JsonConverterWithConstructor(typeof(RootWrapperConverter), "console")]
    public class RemoteConsole
    {
        /// <summary>
        /// The console type.
        /// </summary>
        [JsonProperty("type")]
        public RemoteConsoleType Type { get; set; }

        /// <summary>
        /// The console URL.
        /// </summary>
        [JsonProperty("url")]
        public Uri Url { get; set; }
    }
}