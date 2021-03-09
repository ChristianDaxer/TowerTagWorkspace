/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using System.Collections.Generic;
using UnityEngine;

namespace Holodeck
{
    public class HandManager : MonoBehaviour
    {
        [Header("Prefab of the handmodels for the clients")]
        [Tooltip("Drag your left handmodel-prefab here")]
        public HandPositioner standardHandLeft;
        [Tooltip("Drag your right handmodel-prefab here")]
        public HandPositioner standardHandRight;
        [Header("Debug")]
        [Tooltip("Shows received data like you are a client")]
        public bool showOwnHands = true;

        Dictionary<int, GameObject> handInstances = new Dictionary<int, GameObject>();
        
        /// <summary>
        /// Initial setup, adding events
        /// </summary>
        void Start()
        {
            InputAPI.inputIDEntered += InputIDEntered;
            InputAPI.inputIDLeft += InputIDLeft;
        }

        /// <summary>
        /// Called if a hand enters
        /// </summary>
        /// <param name="hi"></param>
        void InputIDEntered(HandIdentifier hi)
        {
            SpawnHand(hi);
        }

        /// <summary>
        /// Called if a hand leaves
        /// </summary>
        /// <param name="hi"></param>
        void InputIDLeft(HandIdentifier hi)
        {
            DespawnHand(hi);
        }

        /// <summary>
        /// Spwaning of a new hand
        /// </summary>
        /// <param name="hi"></param>
        void SpawnHand(HandIdentifier hi)
        {
            if(!handInstances.ContainsKey(hi.ToInt()) && ((hi.ID == IdAPI.GetPlayerId()) ? showOwnHands : true))
            {
                if (hi.inputType == HolodeckInputType.LeapMotionLeftHand)
                {
                    GameObject hand = Instantiate<GameObject>(standardHandLeft.gameObject);
                    hand.GetComponent<HandPositioner>().handIdentifier = hi;
                    hand.GetComponent<HandPositioner>().ID = hi.ID;
                    hand.GetComponent<HandPositioner>().inputType = hi.inputType;
                    handInstances.Add(hi.ToInt(),hand);
                }
                if (hi.inputType == HolodeckInputType.LeapMotionRightHand)
                {
                    GameObject hand = Instantiate<GameObject>(standardHandRight.gameObject);
                    hand.GetComponent<HandPositioner>().handIdentifier = hi;
                    hand.GetComponent<HandPositioner>().ID = hi.ID;
                    hand.GetComponent<HandPositioner>().inputType = hi.inputType;
                    handInstances.Add(hi.ToInt(), hand);
                }
            }
        }

        /// <summary>
        /// Despawn of a hand
        /// </summary>
        /// <param name="hi"></param>
        void DespawnHand(HandIdentifier hi)
        {
            if(handInstances.ContainsKey(hi.ToInt()))
            {
                Destroy(handInstances[hi.ToInt()]);
                handInstances.Remove(hi.ToInt());
            }
        }
    }
}