using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IPlatformReconfigureTask
{
    IEnumerator Reconfigure(HomeTypes homeType, Action<IPlatformReconfigureTaskDescriptor> startTaskCallback, Action<bool> completedTaskCallback);
}
