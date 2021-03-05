using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using Photon.Realtime;
using Photon.Pun;

public static class PhotonDispatchThreadManager
{
    private static PhotonDispatchThread dispatchThread = null;
    private static readonly Queue<PhotonRPC> _queue = new Queue<PhotonRPC>();

    public static void StartBackgroundDispatch ()
    {
        dispatchThread = new PhotonDispatchThread(_queue);
    }

    public static void StopBackgroundDispatch ()
    {
        if (dispatchThread != null)
            dispatchThread.Stop();
    }

    public static void Queue (PhotonView photonView, string methodName, RpcTarget target, bool useEncryption, params object[] parameters)
    {
        PhotonRPC rpc = new PhotonRPC
        {
            photonView= photonView,
            methodName = methodName,
            useEncryption = useEncryption,
            rpcType = PhotonRPCType.Target,
            target = target,
            parameters = parameters
        };

        Debug.LogFormat($"Queued secure RPC to method: {rpc.methodName} to target: {rpc.target} with encryption: {rpc.useEncryption}");
        _queue.Enqueue(rpc);
    }

    public static void Queue (PhotonView photonView, string methodName, IPlayer player, bool useEncryption, params object[] parameters)
    {
        PhotonRPC rpc = new PhotonRPC
        {
            photonView = photonView,
            methodName = methodName,
            useEncryption = useEncryption,
            rpcType = PhotonRPCType.IPlayer,
            player = player,
            parameters = parameters
        };

        Debug.LogFormat($"Queued secure RPC to method: {rpc.methodName} to target: {rpc.target} with encryption: {rpc.useEncryption}");
        _queue.Enqueue(rpc);
    }
}

public enum PhotonRPCType
{
    IPlayer,
    Target
}

public struct PhotonRPC
{
    public string methodName;
    public PhotonView photonView;
    public bool useEncryption;

    public PhotonRPCType rpcType;
    public RpcTarget target;
    public IPlayer player;

    public object[] parameters;
}

public class PhotonDispatchThread
{
    private Queue<PhotonRPC> _queue = null;
    private bool _stop = false;
    private object threadLock = new object();
    private Thread _thread;

    public void Stop ()
    {
        lock (threadLock)
            _stop = true;
    }

    public PhotonDispatchThread (Queue<PhotonRPC> queue)
    {
        this._queue = queue;
        this._stop = false;

        _thread = new Thread(ThreadLoop);
        _thread.Start();

        Debug.Log("Started Photon dispatch thread.");
    }

    private void ThreadLoop ()
    {
        while (true)
        {
            lock (threadLock)
                if (_stop)
                    break;

            while (_queue.Count > 0)
            {
                PhotonRPC rpc = _queue.Dequeue();
                // Debug.LogFormat($"Emitting secure RPC to method: {rpc.methodName} to target: {rpc.target} with encryption: {rpc.useEncryption}");
                if (rpc.photonView == null)
                    continue;
                switch (rpc.rpcType)
                {
                    case PhotonRPCType.IPlayer:
                    rpc.photonView.RpcSecure(rpc.methodName, rpc.player, rpc.useEncryption, rpc.parameters);
                        break;
                    case PhotonRPCType.Target:
                    rpc.photonView.RpcSecure(rpc.methodName, rpc.target, rpc.useEncryption, rpc.parameters);
                        break;
                    default:
                        break;
                }
                // Debug.Log($"Emitted secure RPC.");
            }
        }
    }
}
