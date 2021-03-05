using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using TowerTag;
using UnityEngine;

public class PillarManager {
    private static PillarManager _instance;
    private readonly IPhotonService _photonService;

    [NotNull]
    public static PillarManager Instance { get; } = _instance ?? (_instance = new PillarManager());

    private readonly Dictionary<int, Pillar> _pillarDictionary = new Dictionary<int, Pillar>();
    private Dictionary<int, Pillar[]> _neighbourhoodDictionary = new Dictionary<int, Pillar[]>();

    private Pillar[] _sortedPillarArray = new Pillar[0];
    private Pillar[] _spawnPillars = new Pillar[0];
    private Pillar[] _spectatorPillars = new Pillar[0];
    private Pillar[] _goalPillars = new Pillar[0];
    private Pillar[] _teamBasedPillars = new Pillar[0];

    private static void OnPlayerRemoved(IPlayer player) {
        if (player == null) {
            Debug.LogError("Cannot remove player, because it is null");
            return;
        }

        if (player.CurrentPillar != null) {
            player.CurrentPillar.Owner = null;
        }
    }

    private PillarManager() {
        PlayerManager.Instance.PlayerRemoved += OnPlayerRemoved;
        _photonService = ServiceProvider.Get<IPhotonService>();
    }

    public void RegisterPillar (Pillar pillar)
    {
        if (_pillarDictionary.ContainsKey(pillar.ID))
        {
            Debug.LogErrorFormat("Cannot register pillar: \"{0}\" with ID: \"{1}\", it has already been registered.", pillar.gameObject.name, pillar.ID);
            return;
        }

        _pillarDictionary.Add(pillar.ID, pillar);
        pillar.Init();


        #if UNITY_EDITOR
        Debug.LogFormat("Registered pillar: \"{0}\" with ID: \"{1}\".", pillar.gameObject.name, pillar.ID);
        #endif

        SortPillarData(pillar);
    }

    public void UnregisterPillar (int ID)
    {
        Pillar tmp = GetPillarByID(ID);

        if (tmp != null)
        {
            _pillarDictionary.Remove(ID);
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogFormat("Unregistered pillar with ID: \"{0}\".", ID);
#endif
            return;
        }

        //Debug.LogErrorFormat("Pillar with ID: \"{0}\", attempted to unregister more than once.", ID);
        RemovePillarData(tmp);
    }

    private void RemovePillarData(Pillar value)
    {
        RemovePillarFromSortedArray(ref _sortedPillarArray, value);

        RemovePillarFromSortedArray(ref _spawnPillars, value, x => x.IsSpawnPillar);
        RemovePillarFromSortedArray(ref _spectatorPillars, value, x => x.IsSpectatorPillar);
        RemovePillarFromSortedArray(ref _goalPillars, value, x => x.IsGoalPillar);
        RemovePillarFromSortedArray(ref _teamBasedPillars, value, x => x.IsTeamBased && !x.IsSpawnPillar);
    }

    private void SortPillarData (Pillar toAdd)
    {
        AddSortedPillarData(ref _sortedPillarArray, toAdd);

        AddSortedPillarData(ref _spawnPillars, toAdd, x => x.IsSpawnPillar);
        AddSortedPillarData(ref _spectatorPillars, toAdd, x => x.IsSpectatorPillar);
        AddSortedPillarData(ref _goalPillars, toAdd, x => x.IsGoalPillar);
        AddSortedPillarData(ref _teamBasedPillars, toAdd, x => x.IsTeamBased && !x.IsSpawnPillar);

#if UNITY_EDITOR
        Debug.LogFormat("Sorted pillar data:\n\tSpawn Pillars: {0}\n\tSpectator Pillars: {1}\n\tGoal PIllars: {2}\n\tTeam Based Pilalrs: {3}",
            _spawnPillars.Length,
            _spawnPillars.Length,
            _goalPillars.Length,
            _teamBasedPillars.Length);
        #endif

        // calculate neighbourhood graph for all pillars
        _neighbourhoodDictionary = TTSceneManager.Instance.IsInHubScene
            ? PillarNeighbourhood.CalculateHubSceneNeighbourhood()
            : PillarNeighbourhood.CalculatePillarNeighbourhood(_sortedPillarArray);
    }

    public Pillar GetPillarByID(int pillarID) {
        if (_pillarDictionary.TryGetValue(pillarID, out Pillar tmp))
        {
            return tmp;
        }

        //Debug.LogError("PillarManager.GetPillarByID: No Pillar with this ID(" + pillarID + ") in lookupTable available!");
        return null;
    }

    public Pillar[] GetNeighboursByPillarID(int pillarID) {
        if (_neighbourhoodDictionary != null && _neighbourhoodDictionary.TryGetValue(pillarID, out Pillar[] tmp))
        {
            return tmp;
        }

        //Debug.LogError("PillarManager.GetNeighboursByPillarID: No Pillar with this ID(" + pillarID + ") in lookupTable available!");
        return null;
    }

    public Pillar[] GetNeighboursByPlayer(IPlayer player) {
        if (player == null) {
            Debug.LogError("PillarManager.GetNeighboursByPlayer: Player is null!");
            return null;
        }

        Pillar pillar = player.CurrentPillar;
        if (pillar != null) {
            return GetNeighboursByPillarID(pillar.ID);
        }

        Debug.LogError("PillarManager.GetNeighboursByPlayer: The Players Pillar is null!");
        return null;
    }

    public bool IsPillarNeighbourOf(Pillar pillar, Pillar possibleNeighbour) {
        if (pillar == null) {
            Debug.LogError("PillarManager.IsNeighbourOf: Can't check Neighbours because Pillar to check is null!");
            return false;
        }

        Pillar[] neighbours = GetNeighboursByPillarID(pillar.ID);

        return neighbours != null && neighbours.Contains(possibleNeighbour);
    }

    public Pillar[] GetAllPillars() {
        int count = 0;
        int length = _sortedPillarArray.Length;
        Pillar[] outputArray = new Pillar[length];

        for (int i = 0; i < length; i++)
        {
            if (_sortedPillarArray[i] != null)
            {
                outputArray[count] = _sortedPillarArray[i];
                count++;
            }
        }

        if (count < 0)
            return null;
        
        if (length != count)
            Array.Resize(ref outputArray, count);

        return outputArray;
    }

    private void AddSortedPillarData(ref Pillar[] destArray, Pillar value, Func<Pillar, bool> predicate = null)
    {
        if ((predicate != null && predicate(value)) || predicate == null)
        {
            if (destArray.Length == 0)
            {
                Array.Resize(ref destArray, 1);
                destArray[0] = value;
                return;
            }

            SortedPillarIDInsert(ref destArray, value);
        }
    }

    private void RemovePillarFromSortedArray(ref Pillar[] destArray, Pillar value, Func<Pillar, bool> predicate = null)
    {
        if (((predicate != null && predicate(value)) || predicate == null) && destArray.Length > 0)
        {
            int startIndex = 0;
            int endIndex = destArray.Length;
            int index = -1;

            while (endIndex > startIndex)
            {
                int windowSize = endIndex - startIndex;
                int middleIndex = startIndex + (windowSize / 2);
                int compareToResult = destArray[middleIndex].ID - value.ID;

                if (compareToResult == 0)
                {
                    index = middleIndex;
                    break;
                }
                else if (compareToResult < 0)
                {
                    startIndex = middleIndex + 1;
                }
                else
                {
                    endIndex = middleIndex;
                }
            }
            if (index < 0)
                return;

            for (int i = index; i < (destArray.Length - 1); i++)
            {
                destArray[i] = destArray[i + 1];
            }
            Array.Resize(ref destArray, destArray.Length - 1);
        }
    }

    private void PillarArrayInsert(ref Pillar[] destArray, Pillar value, int index)
    {
        if (index < 0 || index > (destArray.Length))
        {
            Debug.LogError("Index out of range when trying to insert new pillar");
            return;
        }

        if (index == destArray.Length)
        {
            Array.Resize(ref destArray, destArray.Length + 1);
            destArray[index] = value;
            return;
        }

        Pillar[] tmp = new Pillar[destArray.Length + 1];

        Array.Copy(destArray, tmp, index);
        Array.Copy(destArray, index, tmp, index + 1, destArray.Length - index);
        tmp[index] = value;

        destArray = tmp;
    }

    private void SortedPillarIDInsert(ref Pillar[] destArray, Pillar value)
    {
        int startIndex = 0;
        int endIndex = destArray.Length;
        while (endIndex > startIndex)
        {
            int windowSize = endIndex - startIndex;
            int middleIndex = startIndex + (windowSize / 2);
            int compareToResult = destArray[middleIndex].ID - value.ID;
            if (compareToResult == 0)
            {
                PillarArrayInsert(ref destArray, value, middleIndex);
                return;
            }
            else if (compareToResult < 0)
            {
                startIndex = middleIndex + 1;
            }
            else
            {
                endIndex = middleIndex;
            }
        }
        PillarArrayInsert(ref destArray, value, startIndex);
    }

    private bool CanPlayerTeleportToPillar(IPlayer player, Pillar targetPillar) {

        // Debug.LogFormat($"Player Valid: {player != null}, Target Pillar Valid: {targetPillar != null}");

        if (targetPillar == null)
            return false;

        if (player == null)
            return false;

        return IsPillarIDValidInCurrentScene(targetPillar.ID) && targetPillar.CanTeleport(player);
    }

    /// <summary>
    /// Request teleport and alters internal state (change the occupying Player on requested Pillars) on Master client.
    /// Should only be called on Master client.
    /// </summary>
    /// <param name="player">The Player whop wants to teleport.</param>
    /// <param name="targetPillar">The Pillar the player wants to teleport to</param>
    /// <returns>If teleport was acknowledged and the internal state was altered.</returns>
    public bool RequestTeleportOnMaster(IPlayer player, Pillar targetPillar) {
        if (!_photonService.IsMasterClient) {
            Debug.LogWarning("Teleport denied! Teleport was not requested on Master client!");
            return false;
        }
        return RequestTeleport(player, targetPillar);
    }

    /// <summary>
    /// Request predicted teleport and alters internal state (change the occupying Player on requested Pillars) on client (to remove network delay when teleporting).
    /// Should only be called on clients for predicted teleport (Master client has the last word on this and resets the predicted teleport if needed).
    /// </summary>
    /// <param name="player">The Player whop wants to teleport.</param>
    /// <param name="targetPillar">The Pillar the player wants to teleport to</param>
    /// <returns>If teleport was acknowledged and the internal state was altered.</returns>
    public bool RequestPredictedTeleportOnClient(IPlayer player, Pillar targetPillar) {
        return RequestTeleport(player, targetPillar);
    }

    /// <summary>
    /// Requests Teleport and sets internal state (change the occupying Player on requested Pillars) if teleport is Acknowledged.
    /// </summary>
    /// <param name="player">The Player whop wants to teleport.</param>
    /// <param name="targetPillar">The Pillar the player wants to teleport to</param>
    /// <returns>If teleport was acknowledged and the internal state was altered.</returns>
    private bool RequestTeleport(IPlayer player, Pillar targetPillar) {
        // changed timing of events (could be a problem in other subsystems):
        //      - set new playerPillar in dictionary
        //      - remove from old pillar -> trigger event on old pillar
        //      - add to new Pillar -> trigger event on new Pillar

        if (CanPlayerTeleportToPillar(player, targetPillar)) {
            Pillar oldPillar = player.CurrentPillar;
            if (oldPillar != null)
                oldPillar.Owner = null;

            targetPillar.Owner = player;
            return true;
        }

        return false;
    }


    public void ResetPillarOwningTeamForAllPillars() {
        foreach (Pillar pillar in _sortedPillarArray) {
            ResetPillarOwningTeam(pillar);
        }
    }

    public static void ResetPillarOwningTeam(Pillar pillar) {
        if (pillar != null)
            pillar.ResetOwningTeam();
    }

    public Pillar[] GetAllGoalPillarsInScene() {
        return _goalPillars;
    }

    public int GetNumberOfGoalPillarsInScene() {
        return _goalPillars?.Length ?? 0;
    }

    public Pillar[] GetAllTeamBasedPillarsInScene() {
        return _teamBasedPillars;
    }


    public Pillar FindSpawnPillarForPlayer(IPlayer player) {
        foreach (Pillar pillar in _spawnPillars)
            if (pillar.IsSpawnPillar && CanPlayerTeleportToPillar(player, pillar))
                return pillar;

        if (!player.IsMe)
            return null;

        return null;
    }

    /// <summary>
    /// Finds a spawn pillar by teamID
    /// </summary>
    /// <param name="teamID">ID of a team.</param>
    /// <returns>Returns spawn pillar for given teamID.</returns>
    public Pillar FindSpawnPillar(TeamID teamID) {
        return _spawnPillars.FirstOrDefault(pillar => pillar.IsSpawnPillar && pillar.OwningTeamID == teamID);
    }

    public Pillar GetDefaultPillar ()
    {
        return _spawnPillars.FirstOrDefault(pillar => pillar != null && pillar.IsDefaultPillar);
    }


    public Pillar FindSpectatorPillarForPlayer(IPlayer player) {
        return _spectatorPillars.FirstOrDefault(pillar =>
            pillar.IsSpectatorPillar && CanPlayerTeleportToPillar(player, pillar));
    }

    public bool IsPillarValidInCurrentScene(Pillar pillar) {
        if (pillar == null)
            return false;

        return IsPillarIDValidInCurrentScene(pillar.ID);
    }

    public bool IsPillarIDValidInCurrentScene(int pillarID) {
        // Debug.LogFormat($"Pillar dictionary status: {_pillarDictionary != null}");
        if (_pillarDictionary == null)
            return false;

        // Debug.LogFormat($"Pillar dictionary contains: {_pillarDictionary.ContainsKey(pillarID)}");
        return _pillarDictionary.ContainsKey(pillarID);
    }
}