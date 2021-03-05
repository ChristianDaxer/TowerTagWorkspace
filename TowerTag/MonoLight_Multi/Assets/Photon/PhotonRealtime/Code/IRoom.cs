using System.Collections.Generic;
using ExitGames.Client.Photon;

namespace Photon.Realtime {
    public interface IRoom {
        string Name { get; }
        int PlayerCount { get; }
        Hashtable CustomProperties { get; }
        bool SetCustomProperties(Hashtable propertiesToSet, Hashtable expectedProperties = null, WebFlags webFlags = null);
        Dictionary<int, Player> Players { get; }
        bool SetPropertiesListedInLobby(string[] propertiesListedInLobby);
    }
}