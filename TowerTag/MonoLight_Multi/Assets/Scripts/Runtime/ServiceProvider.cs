using System;
using System.Collections.Generic;
using Commendations;
#if !UNITY_ANDROID
using Cryptlex;
#endif
using GameManagement;
using UI;

public static class ServiceProvider {
    private static readonly Dictionary<Type, object> _defaultServices = new Dictionary<Type, object> {
        {typeof(IPhotonService), new PhotonService()},
        {typeof(IMatchMaker), new AdvancedMatchMaker()},
#if !UNITY_ANDROID
        {typeof(ICryptlexService), new CryptlexService()},
#endif
        {typeof(ISceneService), new SceneService()},
        {typeof(IMessageQueueService), new MessageQueueService()},
        {typeof(ICommendationService), new CommendationService()}
    };

    private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static T Get<T>() {
        try {
            return (T) _services[typeof(T)];
        }
        catch (KeyNotFoundException) {
            try {
                var service = (T) _defaultServices[typeof(T)];
                _services[typeof(T)] = service;
                return service;
            }
            catch (KeyNotFoundException) {
                throw new ApplicationException($"Could not locate service of type {typeof(T)}");
            }
        }
    }

    public static void Clear() {
        _services.Clear();
    }

    public static void Set<T>(T service) {
        _services[typeof(T)] = service;
    }
}