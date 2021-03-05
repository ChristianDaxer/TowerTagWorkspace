using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MuteTest {
    [UnityTest]
    public IEnumerator ShouldToggleMute() {
        // GIVEN
        var hotKeys = ScriptableObject.CreateInstance<DummyHotKeys>();
        CreateMute(hotKeys);
        yield return null;
        Assert.False(AudioListener.pause);

        // WHEN
        hotKeys.RaiseToggleMute();

        // THEN
        Assert.True(AudioListener.pause);
        Object.Destroy(hotKeys);
    }

    [UnityTest]
    public IEnumerator ShouldToggleMuteOff() {
        // GIVEN
        var hotKeys = ScriptableObject.CreateInstance<DummyHotKeys>();
        CreateMute(hotKeys);
        yield return null;
        if(!AudioListener.pause)
            hotKeys.RaiseToggleMute();
        Assert.True(AudioListener.pause);

        // WHEN
        hotKeys.RaiseToggleMute();

        // THEN
        Assert.False(AudioListener.pause);
        Object.Destroy(hotKeys);
    }

    private static void CreateMute(HotKeys hotKeys) {
        var gameObject = new GameObject();
        gameObject.SetActive(false); // disable so that Mute.OnEnable is called after fields are set
        var mute = gameObject.AddComponent<Mute>();
        mute.HotKeys = hotKeys;
        gameObject.SetActive(true);
        mute.enabled = false;
        mute.enabled = true;
    }

    private class DummyHotKeys : HotKeys {
        public int ListenCalled { get; set; }

        public override IEnumerator Listen() {
            ListenCalled++;
            yield return null;
        }

        public void RaiseToggleOperatorUI() {
            ToggleOperatorUI();
        }

        public void RaiseToggleSpectatorUI() {
            ToggleSpectatorUI();
        }

        public void RaiseToggleMute() {
            ToggleMute();
        }
    }
}