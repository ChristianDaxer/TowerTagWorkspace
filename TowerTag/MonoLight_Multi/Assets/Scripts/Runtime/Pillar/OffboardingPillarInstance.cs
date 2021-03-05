using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Pillar))]
public class OffboardingPillarInstance : TTSingleton<OffboardingPillarInstance>
{
    private Pillar pillar;
    public Pillar PillarInstance
    {

        get
        {
            if (pillar == null)
                pillar = GetComponent<Pillar>();
            return pillar;
        }
    }

    protected override void Init() {}
}
