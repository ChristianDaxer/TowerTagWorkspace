using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Jobs;
using UnityEngine;

public class RaycastScheduler : TTSingleton<RaycastScheduler>
{
    private readonly Dictionary<JobHandle, System.Action> callbacks = new Dictionary<JobHandle, System.Action>();
    private readonly List<JobHandle> jobsToRemove = new List<JobHandle>(10);
    protected override void Init() {}

    private void LateUpdate ()
    {
        foreach (var item in callbacks)
        {
            if (!item.Key.IsCompleted)
                continue;

            item.Key.Complete();

            if (item.Value != null)
                item.Value();

            jobsToRemove.Add(item.Key);
        }

        if (jobsToRemove.Count > 0)
        {
            for (int i = 0; i < jobsToRemove.Count; i++)
                callbacks.Remove(jobsToRemove[i]);
            jobsToRemove.Clear();
        }
    }

    public JobHandle Schedule (Ray[] rays, int rayCount, NativeArray<RaycastHit> buffer, float[] distances, int layerMask, System.Action callback)
    {
        var commands = new NativeArray<RaycastCommand>(rayCount, Allocator.Temp);

        for (int i = 0; i < rayCount; i++)
            commands[i] = new RaycastCommand(rays[i].origin, rays[i].direction, distances[i], layerMask);

        JobHandle jobHandle = RaycastCommand.ScheduleBatch(commands, buffer, 4);
        callbacks.Add(jobHandle, callback);

#if UNITY_EDITOR
        Debug.LogFormat("Scheduled batch of raycast: {0} commands.", rayCount);
#endif

        return jobHandle;
    }
}
