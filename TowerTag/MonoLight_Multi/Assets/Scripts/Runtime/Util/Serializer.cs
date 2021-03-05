using System.Xml.Serialization;
using System.IO;
using System.Runtime.Serialization;
using System.Xml;

public class Serializer {
    public static bool SerializeToXML<T>(string dataPath, T data) {
        try {
            using (Stream stream = new FileStream(dataPath, FileMode.Create, FileAccess.Write, FileShare.ReadWrite)) {
                var writer = new StreamWriter(stream, System.Text.Encoding.UTF8);
                var serializer = new XmlSerializer(typeof(T));
                serializer.Serialize(writer, data);
            }
        }
        catch (System.Exception e) {
            Debug.LogError("Error by XML Deserialization!: " + e);
            return false;
        }

        return true;
    }

    public static T DeserializeFromXML<T>(string dataPath) {
        var info = new FileInfo(dataPath);

        if (!info.Exists)
            return default;

        try {
            using (var stream = new FileStream(dataPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                var serializer = new XmlSerializer(typeof(T));
                var data = (T) serializer.Deserialize(stream);
                return data;
            }
        }
        catch (System.Exception e) {
            Debug.LogError("Error by XML Deserialization!: " + e);
            return default;
        }
    }

    public static bool SerializeToXMLAsDataContract<T>(string dataPath, T data) {
        try {
            var serializer = new DataContractSerializer(typeof(T));
            var settings = new XmlWriterSettings {Indent = true, IndentChars = "\t"};
            using (XmlWriter writer = XmlWriter.Create(dataPath, settings)) {
                serializer.WriteObject(writer, data);
            }
        }
        catch (System.Exception e) {
            Debug.LogError("Error by XML Serialization as DataContract!: " + e);
            return false;
        }

        return true;
    }

    public static bool DeSerializeFromXMLAsDataContract<T>(string dataPath, out T data) {
        var info = new FileInfo(dataPath);
        data = default;

        if (!info.Exists)
            return false;

        try {
            var serializer = new DataContractSerializer(typeof(T));
            using (XmlReader reader = XmlReader.Create(dataPath)) {
                data = (T) serializer.ReadObject(reader);
                return true;
            }
        }
        catch (System.Exception e) {
            Debug.LogError("Error by XML Deserialization as DataContract!: " + e);
            return false;
        }
    }
}