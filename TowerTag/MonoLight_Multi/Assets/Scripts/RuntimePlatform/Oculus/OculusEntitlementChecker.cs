using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Platform;

public class OculusEntitlementChecker : TTSingleton<OculusEntitlementChecker>
{
    public delegate void OnCompletedUserEntitlement(bool entitled, Message msg);
    public delegate void OnSkipUserEntitlement();

    public OnCompletedUserEntitlement onCompletedUserEntitlement;
    public OnSkipUserEntitlement onSkipUserEntitlement;

    public bool Entitled => entitled;
    private bool entitled = false;

    public bool SkipCheck => skipCheck;

    [SerializeField] private bool skipCheck = false;
    [SerializeField] private bool quitOnFailure = false;

    private IEnumerator WaitOneFrame ()
    {
        yield return null;
        onSkipUserEntitlement();
    }

    protected override void Init()
    {
        /*
        if (UnityEngine.Application.platform == RuntimePlatform.Android)
        {
            Debug.Log("=== TEMPORARY === Skipping entitlement check on Quest.");
            skipCheck = true;
        }
        */

        if (skipCheck)
        {
            StartCoroutine(WaitOneFrame());
            return;
        }

        try
        {
            Core.AsyncInitialize();
            Entitlements.IsUserEntitledToApplication().OnComplete(EntitlementCallback);
        }

        catch (UnityException e)
        {
            Debug.LogError("Platform failed to initialize due to exception.");
            Debug.LogException(e);
#if !UNITY_EDITOR
            if (quitOnFailure)
                OVRManager.PlatformUIConfirmQuit();
#endif
        }
    }


    void EntitlementCallback(Message msg)
    {
        if (msg.IsError) // User failed entitlement check
        {
            Debug.LogError("You are NOT entitled to use this app.");
            if (onCompletedUserEntitlement != null)
                onCompletedUserEntitlement(false, msg);

#if !UNITY_EDITOR
            if (quitOnFailure)
                OVRManager.PlatformUIConfirmQuit();
#endif
            return;
        }

        Debug.Log("You are entitled to use this app.");

        entitled = true;
        if (onCompletedUserEntitlement != null)
            onCompletedUserEntitlement(true, msg);
    }
}
