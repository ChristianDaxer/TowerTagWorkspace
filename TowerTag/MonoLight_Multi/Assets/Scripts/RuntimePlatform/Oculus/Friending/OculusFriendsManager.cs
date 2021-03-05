using Runtime.Friending;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform.Models;
using Oculus.Platform;

public class OculusFriendsManager : BaseFriendsManager
{
    //public override BaseFriendsManager Instance { get { return (OculusFriendsManager)_instance; } }

    private bool _updateFriendList = false;

    [SerializeField]
    private float _friendListUpdateFrequency = 1.0f;

    public override List<FriendLineInfo> GetCurrentActiveFriends()
    {
        return FriendList;
    }

    public override void Init()
    {
    }

    public override bool IsUserInGame(ulong appId, ulong userId)
    {
        throw new System.NotImplementedException();
    }

    public override void OnFriendsManagerState(bool enabled)
    {
        _updateFriendList = enabled;

        if (enabled)
        {
            StartCoroutine(InternalUpdateFriendList());
        }
    }

    protected override void UpdateFriendList(FriendLineInfo newFriend)
    {
        int index = FriendList.FindIndex(friend => friend.UserId == newFriend.UserId);
        if (index >= 0)
        {
            FriendList[index] = newFriend;
        }
        else
        {
            FriendList.Add(newFriend);
            RaisedFriendUpdateEvent(new List<FriendLineInfo> { newFriend });
        }
    }

    private IEnumerator InternalUpdateFriendList()
    {
        while (_updateFriendList)
        {
            if (Core.IsInitialized())
            {
                Users.GetLoggedInUserFriends()?.OnComplete((uList) =>
                {
                    if (uList != null && !uList.IsError)
                    {
                        Debug.Log("Oculus uList count : " + uList.Data.Count);
                        List<FriendLineInfo> fList = new List<FriendLineInfo>();
                        foreach (User tmp in uList.Data)
                        {
                            var friend = new FriendLineInfo(tmp.ID, tmp.DisplayName, tmp.PresenceStatus == UserPresenceStatus.Online, false, "", null);
                            Debug.Log("Oculus friend : " + tmp.DisplayName + " - " + tmp.OculusID + " - " + tmp.ID + " - " + friend.IsInGame);
                            UpdateFriendList(friend);
                        }
                    }
                });
            }
            else
            {
                Debug.Log("Trying to get logged in friends but Oculus Platform is not initialized");
            }
            yield return new WaitForSecondsRealtime(1.0f);
        }
        yield return null;
    }

    public override bool IsInitialized()
    {
        return Oculus.Platform.Core.IsInitialized();
    }

    public override void RegisterCallbackOnInitialized(OnPlatformInitializedCallback callback)
    {
        if (!Oculus.Platform.Core.IsInitialized())
        {
            _waitOnPlatformInit = StartCoroutine(InternalOnInitialized(callback));
        }
        else
        {
            callback.Invoke(this);
        }
    }

    public override void UnregisterCallbackOnInitialized(OnPlatformInitializedCallback callback)
    {
        if (_waitOnPlatformInit != null)
            StopCoroutine(_waitOnPlatformInit);
    }

    private IEnumerator InternalOnInitialized(OnPlatformInitializedCallback callback)
    {
        while (!Oculus.Platform.Core.IsInitialized())
        {
            yield return new WaitForEndOfFrame();
        }

        callback.Invoke(this);

        yield return null;
    }
}
