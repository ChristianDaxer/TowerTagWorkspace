using TowerTag;
using UnityEngine;
using PHashtable = ExitGames.Client.Photon.Hashtable;

public static class GameManagerStateSyncHelper {
    // Key values for room property Hashtable
    private const string GameManagerCurrentStateStreamKey = "GMState";
    private static bool IsTutorialAlreadyDone => PlayerPrefs.GetInt(PlayerPrefKeys.Tutorial) == 1;
    private static readonly BitSerializer _writeStream = new BitSerializer(new BitWriterNoAlloc(new byte[1024]));
    private static readonly BitSerializer _readStream = new BitSerializer(new BitReaderNoAlloc(new byte[4]));

    /// <summary>
    /// Serialize internal state of GameManager and write it to given Hashtable (which is later send to Server as RoomProperties).
    /// </summary>
    /// <param name="tableToWriteTo"></param>
    public static void WriteGameStateToHashtable(PHashtable tableToWriteTo) {
        if (tableToWriteTo == null) {
            Debug.LogError("Cannot write game state: hash table is null");
            return;
        }

        _writeStream.Reset();

        if (GameManager.Instance.Serialize(_writeStream)) {
            byte[] bytes = _writeStream.GetData();

            tableToWriteTo[GameManagerCurrentStateStreamKey] = bytes;
        }
    }

    /// <summary>
    /// Read data from Hashtable and apply it to GameManager (calls Serialize(readStream))
    /// </summary>
    /// <param name="tableToReadFromTo">Hashtable with entries from room properties to apply it to GamaManager.</param>
    public static void ApplyGameStateFromHashtable(PHashtable tableToReadFromTo) {
        if (tableToReadFromTo.ContainsKey(GameManagerCurrentStateStreamKey)) {
            var bytes = (byte[])tableToReadFromTo[GameManagerCurrentStateStreamKey];

            if (bytes != null) {
                _readStream.SetData(bytes);
                GameManager.Instance.Serialize(_readStream);
            }
        }
    }

    /// <summary>
    /// Try to run through all missed states to ensure a late joining client is up to date and gets all events on the way
    /// </summary>
    /// <param name="roomProps">Hashtable to read data about missed states.</param>
    public static void InitGameManagerStateMachineFromRoomProps(PHashtable roomProps) {
        if (roomProps == null)
            return;

        if (roomProps.ContainsKey(GameManagerCurrentStateStreamKey)) {
            var currentState = (byte[]) roomProps[GameManagerCurrentStateStreamKey];
            GameManager.Instance.Serialize(new BitSerializer(new BitReaderNoAlloc(currentState)));
        }
        else
        {
            if (!IsTutorialAlreadyDone || GameManager.Instance.ForceTutorial)
                GameManager.Instance.ChangeStateToTutorial();
            else
                GameManager.Instance.ChangeStateToDefault();
        }
    }
}