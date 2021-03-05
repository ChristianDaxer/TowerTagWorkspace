using System.Xml.Serialization;
// ReSharper disable InconsistentNaming

namespace Toornament {
    [System.Serializable]
    public class ToornamentProfile {
        [XmlElement("apiKey")] public string _apiKey = "";

        [XmlElement("client_id")] public string _client_id = "";

        [XmlElement("client_secret")] public string _client_secret = "";
    }
}