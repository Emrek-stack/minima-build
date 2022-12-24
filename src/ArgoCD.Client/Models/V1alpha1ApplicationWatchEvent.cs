// <auto-generated>
// Code generated by Microsoft (R) AutoRest Code Generator.
// Changes may cause incorrect behavior and will be lost if the code is
// regenerated.
// </auto-generated>

namespace ArgoCD.Client.Models
{
    using Newtonsoft.Json;
    using System.Linq;

    /// <summary>
    /// ApplicationWatchEvent contains information about application change.
    /// </summary>
    public partial class V1alpha1ApplicationWatchEvent
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1ApplicationWatchEvent
        /// class.
        /// </summary>
        public V1alpha1ApplicationWatchEvent()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1ApplicationWatchEvent
        /// class.
        /// </summary>
        public V1alpha1ApplicationWatchEvent(V1alpha1Application application = default(V1alpha1Application), string type = default(string))
        {
            Application = application;
            Type = type;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "application")]
        public V1alpha1Application Application { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

    }
}