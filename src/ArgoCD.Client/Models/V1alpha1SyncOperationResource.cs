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
    /// SyncOperationResource contains resources to sync.
    /// </summary>
    public partial class V1alpha1SyncOperationResource
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1SyncOperationResource
        /// class.
        /// </summary>
        public V1alpha1SyncOperationResource()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1SyncOperationResource
        /// class.
        /// </summary>
        public V1alpha1SyncOperationResource(string group = default(string), string kind = default(string), string name = default(string), string namespaceProperty = default(string))
        {
            Group = group;
            Kind = kind;
            Name = name;
            NamespaceProperty = namespaceProperty;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "group")]
        public string Group { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "kind")]
        public string Kind { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "namespace")]
        public string NamespaceProperty { get; set; }

    }
}