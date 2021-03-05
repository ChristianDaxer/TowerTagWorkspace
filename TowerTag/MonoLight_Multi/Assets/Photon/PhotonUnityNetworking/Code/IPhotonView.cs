using System.Collections.Generic;
using Photon.Realtime;
using UnityEngine;

namespace Photon.Pun {
    public interface IPhotonView {
        /// <summary>
        /// The ID of the PhotonView. Identifies it in a networked game (per room).
        /// </summary>
        /// <remarks>See: [Network Instantiation](@ref instantiateManual)</remarks>
        int ViewID { get; set; }

        /// <summary>True if the PhotonView was loaded with the scene (game object) or instantiated with InstantiateSceneObject.</summary>
        /// <remarks>
        /// Scene objects are not owned by a particular player but belong to the scene. Thus they don't get destroyed when their
        /// creator leaves the game and the current Master Client can control them (whoever that is).
        /// The ownerId is 0 (player IDs are 1 and up).
        /// </remarks>
        bool IsSceneView { get; }

        /// <summary>
        /// The owner of a PhotonView is the player who created the GameObject with that view. Objects in the scene don't have an owner.
        /// </summary>
        /// <remarks>
        /// The owner/controller of a PhotonView is also the client which sends position updates of the GameObject.
        ///
        /// Ownership can be transferred to another player with PhotonView.TransferOwnership or any player can request
        /// ownership by calling the PhotonView's RequestOwnership method.
        /// The current owner has to implement IPunCallbacks.OnOwnershipRequest to react to the ownership request.
        /// </remarks>
        IPlayer Owner { get; }

        int OwnerActorNr { get; }
        bool IsOwnerActive { get; }

        /// <summary>
        /// True if the PhotonView is "mine" and can be controlled by this client.
        /// </summary>
        /// <remarks>
        /// PUN has an ownership concept that defines who can control and destroy each PhotonView.
        /// True in case the owner matches the local Player.
        /// True if this is a scene photonview on the Master client.
        /// </remarks>
        bool IsMine { get; }

        /// <summary>
        /// Depending on the PhotonView's OwnershipTransfer setting, any client can request to become owner of the PhotonView.
        /// </summary>
        /// <remarks>
        /// Requesting ownership can give you control over a PhotonView, if the OwnershipTransfer setting allows that.
        /// The current owner might have to implement IPunCallbacks.OnOwnershipRequest to react to the ownership request.
        ///
        /// The owner/controller of a PhotonView is also the client which sends position updates of the GameObject.
        /// </remarks>
        void RequestOwnership();

        /// <summary>
        /// Transfers the ownership of this PhotonView (and GameObject) to another player.
        /// </summary>
        /// <remarks>
        /// The owner/controller of a PhotonView is also the client which sends position updates of the GameObject.
        /// </remarks>
        void TransferOwnership(IPlayer newOwner);

        /// <summary>
        /// Transfers the ownership of this PhotonView (and GameObject) to another player.
        /// </summary>
        /// <remarks>
        /// The owner/controller of a PhotonView is also the client which sends position updates of the GameObject.
        /// </remarks>
        void TransferOwnership(int newOwnerId);

        /// <summary>
        /// Call a RPC method of this GameObject on remote clients of this room (or on all, inclunding this client).
        /// </summary>
        /// <remarks>
        /// [Remote Procedure Calls](@ref rpcManual) are an essential tool in making multiplayer games with PUN.
        /// It enables you to make every client in a room call a specific method.
        ///
        /// RPC calls can target "All" or the "Others".
        /// Usually, the target "All" gets executed locally immediately after sending the RPC.
        /// The "*ViaServer" options send the RPC to the server and execute it on this client when it's sent back.
        /// Of course, calls are affected by this client's lag and that of remote clients.
        ///
        /// Each call automatically is routed to the same PhotonView (and GameObject) that was used on the
        /// originating client.
        ///
        /// See: [Remote Procedure Calls](@ref rpcManual).
        /// </remarks>
        /// <param name="methodName">The name of a fitting method that was has the RPC attribute.</param>
        /// <param name="target">The group of targets and the way the RPC gets sent.</param>
        /// <param name="parameters">The parameters that the RPC method has (must fit this call!).</param>
        void RPC(string methodName, RpcTarget target, params object[] parameters);

        /// <summary>
        /// Call a RPC method of this GameObject on remote clients of this room (or on all, inclunding this client).
        /// </summary>
        /// <remarks>
        /// [Remote Procedure Calls](@ref rpcManual) are an essential tool in making multiplayer games with PUN.
        /// It enables you to make every client in a room call a specific method.
        ///
        /// RPC calls can target "All" or the "Others".
        /// Usually, the target "All" gets executed locally immediately after sending the RPC.
        /// The "*ViaServer" options send the RPC to the server and execute it on this client when it's sent back.
        /// Of course, calls are affected by this client's lag and that of remote clients.
        ///
        /// Each call automatically is routed to the same PhotonView (and GameObject) that was used on the
        /// originating client.
        ///
        /// See: [Remote Procedure Calls](@ref rpcManual).
        /// </remarks>
        ///<param name="methodName">The name of a fitting method that was has the RPC attribute.</param>
        ///<param name="target">The group of targets and the way the RPC gets sent.</param>
        ///<param name="encrypt"> </param>
        ///<param name="parameters">The parameters that the RPC method has (must fit this call!).</param>
        void RpcSecure(string methodName, RpcTarget target, bool encrypt, params object[] parameters);

        /// <summary>
        /// Call a RPC method of this GameObject on remote clients of this room (or on all, inclunding this client).
        /// </summary>
        /// <remarks>
        /// [Remote Procedure Calls](@ref rpcManual) are an essential tool in making multiplayer games with PUN.
        /// It enables you to make every client in a room call a specific method.
        ///
        /// This method allows you to make an RPC calls on a specific player's client.
        /// Of course, calls are affected by this client's lag and that of remote clients.
        ///
        /// Each call automatically is routed to the same PhotonView (and GameObject) that was used on the
        /// originating client.
        ///
        /// See: [Remote Procedure Calls](@ref rpcManual).
        /// </remarks>
        /// <param name="methodName">The name of a fitting method that was has the RPC attribute.</param>
        /// <param name="targetPlayer">The group of targets and the way the RPC gets sent.</param>
        /// <param name="parameters">The parameters that the RPC method has (must fit this call!).</param>
        void RPC(string methodName, IPlayer targetPlayer, params object[] parameters);

        /// <summary>
        /// Call a RPC method of this GameObject on remote clients of this room (or on all, inclunding this client).
        /// </summary>
        /// <remarks>
        /// [Remote Procedure Calls](@ref rpcManual) are an essential tool in making multiplayer games with PUN.
        /// It enables you to make every client in a room call a specific method.
        ///
        /// This method allows you to make an RPC calls on a specific player's client.
        /// Of course, calls are affected by this client's lag and that of remote clients.
        ///
        /// Each call automatically is routed to the same PhotonView (and GameObject) that was used on the
        /// originating client.
        ///
        /// See: [Remote Procedure Calls](@ref rpcManual).
        /// </remarks>
        ///<param name="methodName">The name of a fitting method that was has the RPC attribute.</param>
        ///<param name="targetPlayer">The group of targets and the way the RPC gets sent.</param>
        ///<param name="encrypt"> </param>
        ///<param name="parameters">The parameters that the RPC method has (must fit this call!).</param>
        void RpcSecure(string methodName, IPlayer targetPlayer, bool encrypt, params object[] parameters);
    }
}