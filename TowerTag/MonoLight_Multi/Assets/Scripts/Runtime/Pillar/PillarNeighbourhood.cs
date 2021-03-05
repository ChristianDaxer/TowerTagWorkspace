using System.Collections.Generic;
using System.Linq;
using Hub;
using UnityEngine;

public static class PillarNeighbourhood {
    private static readonly string[] _pillarLayers = {"Pillar", "PillarClaimCollider"};
    private const float MaxNeighbourDistance = 16;
    private static readonly Collider[] _collider = new Collider[128];

    public static Dictionary<int, Pillar[]> CalculatePillarNeighbourhood(Pillar[] scenePillars) {
        LayerMask pillarLayerMask = LayerMask.GetMask(_pillarLayers);
        return CalculatePillarNeighbourhood(scenePillars, MaxNeighbourDistance, pillarLayerMask);
    }

    public static Dictionary<int, Pillar[]> CalculateHubSceneNeighbourhood() {
        var neighbourhood = new Dictionary<int, Pillar[]>();
        Object.FindObjectsOfType<HubLaneController>()
            .Apply(hlc => neighbourhood.Add(hlc.SpawnPillar.ID, new[] {hlc.TagAndGoPillar}))
            .ForEach(hlc => neighbourhood.Add(hlc.TagAndGoPillar.ID, new[] {hlc.ReadyPillar}));
        Object.FindObjectsOfType<Pillar>()
            .Where(pillar => !neighbourhood.ContainsKey(pillar.ID))
            .ForEach(pillar => neighbourhood.Add(pillar.ID, new Pillar[] { }));
        return neighbourhood;
    }

    private static Dictionary<int, Pillar[]> CalculatePillarNeighbourhood(Pillar[] scenePillars, float distance,
        LayerMask pillarMask) {
        var neighbours = new Dictionary<int, Pillar[]>();

        // walk through pillars and get all of their neighbours (naive version -> Physics.CheckSphere for every Pillar)
        foreach (Pillar pillar in scenePillars) {
            var currentNeighbours = new List<Pillar>();
            // get all collider in Radius
            Physics.OverlapSphereNonAlloc(pillar.transform.position, distance, _collider, pillarMask,
                QueryTriggerInteraction.Collide);

            // filter the collider of claim visuals
            if (_collider != null) {
                foreach (Collider c in _collider) {
                    if (c == null)
                        continue;
                    var chargeableCollider = c.GetComponent<ChargeableCollider>();
                    if (chargeableCollider != null) {
                        var otherPillar = chargeableCollider.Chargeable as Pillar;
                        if (otherPillar != null) {
                            if (pillar.ID != otherPillar.ID)
                                currentNeighbours.Add(otherPillar);
                        }
                    }
                }
            }

            neighbours.Add(pillar.ID, currentNeighbours.ToArray());
        }

        return neighbours;
    }
}