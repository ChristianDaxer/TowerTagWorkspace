using Runtime.Friending;
using System.Collections.Generic;
using UnityEngine;

public abstract class BaseFriendsManager : MonoBehaviour
{
    public delegate void FriendListUpdate(object sender, List<FriendLineInfo> currentFriendList);

    public delegate void OnPlatformInitializedCallback(object sender);

    public event FriendListUpdate FriendListUpdated;

    protected List<FriendLineInfo> FriendList { get; } = new List<FriendLineInfo>();

    protected static BaseFriendsManager _instance = null;

    public static BaseFriendsManager Instance { get { return _instance; } }

    protected Coroutine _waitOnPlatformInit = null;

    [SerializeField]
    private string _versionNote = "";

    public string VersionNote { get { return _versionNote; } }

    private void Awake()
    {
        if (_instance != null || !TowerTagSettings.Home)
        {
            Destroy(gameObject);
            return;
        }
         
        _instance = this;
        DontDestroyOnLoad(this);

        Init();
    }

    public abstract List<FriendLineInfo> GetCurrentActiveFriends();
    public abstract bool IsUserInGame(ulong appId, ulong userId);
    public abstract void OnFriendsManagerState(bool enabled);
    protected abstract void UpdateFriendList(FriendLineInfo newFriend);
    public abstract void Init();
    public abstract bool IsInitialized();
    public abstract void RegisterCallbackOnInitialized(OnPlatformInitializedCallback callback);
    public abstract void UnregisterCallbackOnInitialized(OnPlatformInitializedCallback callback);
    protected void RaisedFriendUpdateEvent(List<FriendLineInfo> currentFriendList)
    {
        FriendListUpdated?.Invoke(this, currentFriendList);
    }

    
}
