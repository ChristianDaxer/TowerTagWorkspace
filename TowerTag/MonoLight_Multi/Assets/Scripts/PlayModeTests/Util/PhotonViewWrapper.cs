using System.Collections.Generic;
using NSubstitute;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class PhotonViewWrapper : MonoBehaviour, IPhotonView {
    public IPhotonView Implementation { get; } = Substitute.For<IPhotonView>();

    public int ViewID {
        get => Implementation.ViewID;
        set => Implementation.ViewID = value;
    }

    public bool IsSceneView => Implementation.IsSceneView;
    public IPlayer Owner => Implementation.Owner;
    public int OwnerActorNr => Implementation.OwnerActorNr;
    public bool IsOwnerActive => Implementation.IsOwnerActive;
    public bool IsMine => Implementation.IsMine;

    public List<Component> ObservedComponents;

    public void RequestOwnership() {
        Implementation.RequestOwnership();
    }

    public void TransferOwnership(IPlayer newOwner) {
        Implementation.TransferOwnership(newOwner);
    }

    public void TransferOwnership(int newOwnerId) {
        Implementation.TransferOwnership(newOwnerId);
    }

    public void RPC(string methodName, RpcTarget target, params object[] parameters) {
        Implementation.RPC(methodName, target, parameters);
    }

    public void RpcSecure(string methodName, RpcTarget target, bool encrypt, params object[] parameters) {
        Implementation.RpcSecure(methodName, target, encrypt, parameters);
    }

    public void RPC(string methodName, IPlayer targetPlayer, params object[] parameters) {
        Implementation.RPC(methodName, targetPlayer, parameters);
    }

    public void RpcSecure(string methodName, IPlayer targetPlayer, bool encrypt, params object[] parameters) {
        Implementation.RpcSecure(methodName, targetPlayer, encrypt, parameters);
    }
}