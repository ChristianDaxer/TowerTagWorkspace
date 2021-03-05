using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class PlatformServices: TTSingleton<PlatformServices>
{
    public abstract ILeaderboardManager LeaderboardManager { get; }
    public abstract IVRChaperone Chaperone { get; }
}
