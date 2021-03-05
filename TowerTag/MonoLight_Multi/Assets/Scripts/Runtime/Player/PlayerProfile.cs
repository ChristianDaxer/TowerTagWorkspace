using System.Xml.Serialization;

public class PlayerProfile
{
    [XmlElement("PlayerName")]
    public string PlayerName = "";

    [XmlElement("PlayerID")]
    public string PlayerGUID;
}
