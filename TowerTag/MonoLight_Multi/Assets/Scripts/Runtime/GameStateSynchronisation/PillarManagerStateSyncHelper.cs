using Photon.Pun;
using PHashtable = ExitGames.Client.Photon.Hashtable;

public static class PillarManagerStateSyncHelper {
    public static int LastPropertyUpdateTimestamp { get; set; }
    private const string PillarManagerPrefix = "PS";

    private static readonly BitSerializer _writeStream = new BitSerializer(new BitWriterNoAlloc(new byte[4096]));
    private static readonly BitSerializer _readStream = new BitSerializer(new BitReaderNoAlloc(new byte[4]));


    public static void WriteStateToHashtable(PHashtable tableToWriteTo) {
        if (tableToWriteTo == null) {
            Debug.LogError("Cannot write pillar state: hash table is null");
            return;
        }

        _writeStream.Reset();
        SerializePillars(_writeStream);
        tableToWriteTo[PillarManagerPrefix] = _writeStream.GetData();
    }


    public static void ApplyStateFromHashtable(PHashtable changedPropertiesToReadFrom) {
        // Read Pillar State
        if (changedPropertiesToReadFrom.ContainsKey(PillarManagerPrefix)) {
            var pillarData = (byte[]) changedPropertiesToReadFrom[PillarManagerPrefix];

            if (pillarData == null) {
                Debug.LogError("Cannot apply pillar state: pillar data is null");
                return;
            }

            _readStream.SetData(pillarData);
            SerializePillars(_readStream);
        }
    }

    private static void SerializePillars(BitSerializer stream) {
        Pillar[] pillars = PillarManager.Instance.GetAllPillars();

        if (pillars == null) {
            Debug.LogError("Cannot serialize pillars: pillar array is null");
            return;
        }

        if (stream.IsWriting) {
            stream.WriteInt(pillars.Length, 0, BitCompressionConstants.MaxPillarCount);
            stream.WriteInt(PhotonNetwork.ServerTimestamp, int.MinValue, int.MaxValue);
        }
        else {
            int length = stream.ReadInt(0, BitCompressionConstants.MaxPillarCount);
            LastPropertyUpdateTimestamp = stream.ReadInt(int.MinValue, int.MaxValue);
            if (length != pillars.Length) {
                Debug.LogError("Cannot deserialize pillars: " +
                               "the number of send pillars does not match the number of local pillars");
                return;
            }
        }

        foreach (Pillar pillar in pillars) {
            pillar.Serialize(stream);
        }
    }
}