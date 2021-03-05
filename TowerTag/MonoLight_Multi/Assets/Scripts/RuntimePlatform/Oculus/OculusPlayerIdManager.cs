using Oculus.Platform;
using Home;
using Oculus.Platform.Models;
using Photon.Realtime;
using Photon.Pun;
using UnityEngine;

public class OculusPlayerIdManager : PlayerIdManager
{
    public override string GetUserId() => _userID;

    protected override void Init()
    {
        if (!OculusEntitlementChecker.GetInstance(out var entitlementChecker))
            return;
        entitlementChecker.onCompletedUserEntitlement += GetOculusUserID;
        entitlementChecker.onSkipUserEntitlement += OnSkipUserEntitlement;
    }

    private void OnSkipUserEntitlement ()
    {
        _userID = PlayerIdManager.TempUserId;
        _displayName = PlayerIdManager.TempDisplayName;
        InitializePhoton("test", _userID);
    }

    private void GetOculusUserID(bool entitled, Message msg)
    {
        if (!entitled)
            return;

        var request = Oculus.Platform.Users.GetLoggedInUser();

        request.OnComplete((user) =>
        {
            if (user.IsError)
            {
                Debug.LogErrorFormat("Error occurred while attempting to get the logged in user: \n{0}", user.GetError().Message);
                return;
            }

            _userID = user.GetUser().ID.ToString(); //Seems like we need to use the ulong for all the other systems including photon.
            Debug.LogFormat("Received Oculus user ID: \"{0}\".", _userID);

            Oculus.Platform.Users.GetUserProof().OnComplete(OnUserProofCallback);
        });
    }

    private void InitializePhoton (string oculusNonce, string userId)
    {
        // Debug.LogFormat("Received Oculus user Nonce: \"{0}\".", oculusNonce);
        Debug.LogFormat("Authenticating user for Oculus with Photon using ID: \"{0}\" and Nonce: \"{1}\".", userId, oculusNonce);

        PhotonNetwork.AuthValues = new AuthenticationValues(userId);
        PhotonNetwork.AuthValues.AuthType = CustomAuthenticationType.Oculus;

        PhotonNetwork.AuthValues.UserId = userId;
        PhotonNetwork.AuthValues.AddAuthParameter("nonce", oculusNonce);

        _isReady = true;
    }

    private void OnUserProofCallback(Message<UserProof> msg)
    {
        if (msg.IsError)
        {
            _isReady = true;
            Debug.LogErrorFormat("Oculus: Error getting user proof. Error Message: {0}",
                msg.GetError().Message);

            return;
        }

        string oculusNonce = msg.Data.Value;
        InitializePhoton(oculusNonce, _userID);
    }

    protected override void OnHubSceneLoaded()
    {
        if (!Core.IsInitialized()) return;

        if (PlayerManager.Instance.GetOwnPlayer() == null)
            Debug.LogError("cant find local player");

        PlayerManager.Instance.GetOwnPlayer()?.LogIn(GetUserId());

        // get player statistics
        if (!PlayerAccount.ReceivedPlayerStatistics)
        {
            PlayerAccount.Init(GetUserId());
            Debug.Log($"Player logged in: {PlayerManager.Instance.GetOwnPlayer()?.IsLoggedIn}");
            Debug.Log($"Player ID: {PlayerManager.Instance.GetOwnPlayer()?.MembershipID}");
        }

        GetPlayerStatistics(GetUserId());
    }
}
