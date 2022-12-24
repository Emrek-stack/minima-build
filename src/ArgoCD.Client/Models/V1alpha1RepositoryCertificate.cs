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
    /// A RepositoryCertificate is either SSH known hosts entry or TLS
    /// certificate
    /// </summary>
    public partial class V1alpha1RepositoryCertificate
    {
        /// <summary>
        /// Initializes a new instance of the V1alpha1RepositoryCertificate
        /// class.
        /// </summary>
        public V1alpha1RepositoryCertificate()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the V1alpha1RepositoryCertificate
        /// class.
        /// </summary>
        /// <param name="certData">CertData contains the actual certificate
        /// data, dependent on the certificate type</param>
        /// <param name="certInfo">CertInfo will hold additional certificate
        /// info, depdendent on the certificate type (e.g. SSH fingerprint,
        /// X509 CommonName)</param>
        /// <param name="certSubType">CertSubType specifies the sub type of the
        /// cert, i.e. "ssh-rsa"</param>
        /// <param name="certType">CertType specifies the type of the
        /// certificate - currently one of "https" or "ssh"</param>
        /// <param name="serverName">ServerName specifies the DNS name of the
        /// server this certificate is intended for</param>
        public V1alpha1RepositoryCertificate(byte[] certData = default(byte[]), string certInfo = default(string), string certSubType = default(string), string certType = default(string), string serverName = default(string))
        {
            CertData = certData;
            CertInfo = certInfo;
            CertSubType = certSubType;
            CertType = certType;
            ServerName = serverName;
            CustomInit();
        }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults
        /// </summary>
        partial void CustomInit();

        /// <summary>
        /// Gets or sets certData contains the actual certificate data,
        /// dependent on the certificate type
        /// </summary>
        [JsonProperty(PropertyName = "certData")]
        public byte[] CertData { get; set; }

        /// <summary>
        /// Gets or sets certInfo will hold additional certificate info,
        /// depdendent on the certificate type (e.g. SSH fingerprint, X509
        /// CommonName)
        /// </summary>
        [JsonProperty(PropertyName = "certInfo")]
        public string CertInfo { get; set; }

        /// <summary>
        /// Gets or sets certSubType specifies the sub type of the cert, i.e.
        /// "ssh-rsa"
        /// </summary>
        [JsonProperty(PropertyName = "certSubType")]
        public string CertSubType { get; set; }

        /// <summary>
        /// Gets or sets certType specifies the type of the certificate -
        /// currently one of "https" or "ssh"
        /// </summary>
        [JsonProperty(PropertyName = "certType")]
        public string CertType { get; set; }

        /// <summary>
        /// Gets or sets serverName specifies the DNS name of the server this
        /// certificate is intended for
        /// </summary>
        [JsonProperty(PropertyName = "serverName")]
        public string ServerName { get; set; }

    }
}