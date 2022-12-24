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
    /// HealthStatus contains information about the currently observed health
    /// state of an application or resource
    /// </summary>
    public partial class V1alpha1HealthStatus
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1HealthStatus class.
        /// </summary>
        public V1alpha1HealthStatus()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1HealthStatus class.
        /// </summary>
        /// <param name="message">Message is a human-readable informational
        /// message describing the health status</param>
        /// <param name="status">Status holds the status code of the
        /// application or resource</param>
        public V1alpha1HealthStatus(string message = default(string), string status = default(string))
        {
            Message = message;
            Status = status;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets message is a human-readable informational message
        /// describing the health status
        /// </summary>
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }

        /// <summary>
        /// Gets or sets status holds the status code of the application or
        /// resource
        /// </summary>
        [JsonProperty(PropertyName = "status")]
        public string Status { get; set; }

    }
}