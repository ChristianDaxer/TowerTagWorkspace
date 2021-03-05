using System;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using UnityEngine.Video;

public class IngameExplainerManager : MonoBehaviour {
    [Serializable]
    public class VideoDictionary : SerializableDictionaryBase<ControllerTypeDetector.ControllerType, VideoClip> {
    }

    [SerializeField] private VideoPlayer _videoPlayer;
    [SerializeField] private VideoDictionary _videoDictionary;

    void Start() {
        if (_videoDictionary.ContainsKey(ControllerTypeDetector.CurrentControllerType))
            _videoPlayer.clip = _videoDictionary[ControllerTypeDetector.CurrentControllerType];
    }
}