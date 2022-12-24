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
    /// RepoAppDetailsResponse application details
    /// </summary>
    public partial class RepositoryRepoAppDetailsResponse
    {
        /// <summary>
        /// Initializes a new instance of the RepositoryRepoAppDetailsResponse
        /// class.
        /// </summary>
        public RepositoryRepoAppDetailsResponse()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the RepositoryRepoAppDetailsResponse
        /// class.
        /// </summary>
        public RepositoryRepoAppDetailsResponse(object directory = default(object), RepositoryHelmAppSpec helm = default(RepositoryHelmAppSpec), RepositoryKsonnetAppSpec ksonnet = default(RepositoryKsonnetAppSpec), RepositoryKustomizeAppSpec kustomize = default(RepositoryKustomizeAppSpec), string type = default(string))
        {
            Directory = directory;
            Helm = helm;
            Ksonnet = ksonnet;
            Kustomize = kustomize;
            Type = type;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "directory")]
        public object Directory { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "helm")]
        public RepositoryHelmAppSpec Helm { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "ksonnet")]
        public RepositoryKsonnetAppSpec Ksonnet { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "kustomize")]
        public RepositoryKustomizeAppSpec Kustomize { get; set; }

        /// <summary>
        /// </summary>
        [JsonProperty(PropertyName = "type")]
        public string Type { get; set; }

    }
}