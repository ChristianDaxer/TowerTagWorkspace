using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AI;
using Photon.Pun;
using SOEventSystem.Shared;
using TowerTag;
using UnityEngine;
using VRNerdsUtilities;
using Random = UnityEngine.Random;

public class BotManager : SingletonMonoBehaviour<BotManager> {
    [SerializeField] private SharedStringList _sharedBotNames;

    [SerializeField] [Tooltip("If list of available bot names is empty")]
    private string _botDefaultName = "TowerTag Bot";

    private const string BotTag = "[BOT]";

    private List<string> _listOfBotNames; //List to copy bot names into - needs minimum as much names as available empty slots
    private List<string> _listOfBotNamesInUse;

    [SerializeField] private Player _playerPrefab;

    private void Start() {
        //save bot names into list in order to add and remove names as bots are added/kicked
        _listOfBotNames = _sharedBotNames.Value.Select(name => BotTag + " " + name).ToList();

        _listOfBotNamesInUse = new List<string>();
        
        if (TowerTagSettings.Home) gameObject.AddComponent<BotManagerHome>();
    }

    public IPlayer AddBot(TeamID teamID, BotBrain.BotDifficulty botLevel = BotBrain.BotDifficulty.Easy) {
        if (TeamManager.Singleton.Get(teamID).GetPlayerCount() >= TowerTagSettings.MaxTeamSize) {
            Debug.LogWarning($"Not able to start Bot in team {teamID}: team is full");
            return null;
        }

        var botName = SelectRandomBotNameFromList();

        var player = ServiceProvider.Get<IPhotonService>()
            .Instantiate(_playerPrefab.name, Vector3.zero, Quaternion.identity)
            .GetComponent<Player>();

        if (player != null)
        {
            player.InitPropertyKeys();
            player.IsBot = true;
            player.SetIsBotFlagInProperties(player.IsBot);
            player.SetName(botName);
            player.SetTeam(teamID);
            player.BotDifficulty = botLevel;
            return player;
        }
        Debug.LogError("BotManager:AddBot -> Bot Player is null. This should not have happened.");
        return null;
    }

    public void RemoveAndDestroyBots(IPlayer[] botArray)
    {
        foreach (IPlayer bot in botArray)
        {
            _listOfBotNamesInUse.Remove(bot.PlayerName);
        }
        
        StartCoroutine(DestroyBots(botArray));
    }
    
    private IEnumerator DestroyBots(IPlayer[] botArray) {
        foreach (IPlayer player in botArray) {
            yield return new WaitForSeconds(Random.Range(0.5f, 1f));
            PhotonNetwork.Destroy(player.GameObject);
        }
    }
    
    private string SelectRandomBotNameFromList()
    {
        if (_listOfBotNames != null && _listOfBotNames.Count > 0 
                                    && _listOfBotNames.Count > _listOfBotNamesInUse.Count)
        {
            int randomStartIndex = Random.Range(0, _listOfBotNames.Count - 1);
            string botName = SelectNextAvailableBotNameInList(randomStartIndex);
            
            _listOfBotNamesInUse.Add(botName);

            return botName;
        }
        return _botDefaultName;
    }

    private string SelectNextAvailableBotNameInList(int index)
    {
        string botNameAtIndex = _listOfBotNames[index];

        if (!_listOfBotNamesInUse.Contains(botNameAtIndex))
        {
            return botNameAtIndex;
        }
        
        index++;
        if (index >= _listOfBotNames.Count) index = 0;
        
        return SelectNextAvailableBotNameInList(index);
    }
}