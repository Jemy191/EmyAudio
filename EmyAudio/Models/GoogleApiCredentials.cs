using System.Diagnostics.CodeAnalysis;

namespace EmyAudio.Models;

[SuppressMessage("ReSharper", "InconsistentNaming")]
public class GoogleApiCredentials
{
    public required Installed installed { get; set; }

    public class Installed
    {
        public required  string client_id { get; set; }
        public required  string project_id { get; set; }
        public required  string auth_uri { get; set; }
        public required  string token_uri { get; set; }
        public required  string auth_provider_x509_cert_url { get; set; }
        public required  string client_secret { get; set; }
        public required  List<string> redirect_uris { get; set; } = new List<string>();
    }
}