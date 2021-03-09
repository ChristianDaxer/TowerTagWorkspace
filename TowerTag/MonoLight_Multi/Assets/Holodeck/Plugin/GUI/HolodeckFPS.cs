/******************************************************************************
 * Copyright (C) Holodeck VR GmbH 2017 - 2020.                                *
 * SPREE Interactive SDK Version 4.0.27-RC                                    *
 ******************************************************************************/

using UnityEngine;

namespace Holodeck
{

    internal class HolodeckFPS : MonoBehaviour
    {
        private long tprev;
        private long tact;
        private long startTimestamp;
        private int frames;
        private long startfpstime;
        public static float realdelta;
        public static float fps;

        public UnityEngine.UI.Text textelem = null;

        /// <summary>
        /// Initial Timestamp is set
        /// </summary>
        void Start()
        {
            startTimestamp = System.Diagnostics.Stopwatch.GetTimestamp();
        }

        /// <summary>
        /// Refreshing the FPS each Update and display it in a Unity Text-Object
        /// </summary>
        void Update()
        {
            tprev = tact;
            tact = (System.Diagnostics.Stopwatch.GetTimestamp() - startTimestamp) * 1000L / System.Diagnostics.Stopwatch.Frequency;
            realdelta = (float)(tact - tprev);
            realdelta = realdelta * 0.001f;

            frames++;
            if (frames > 100)
            {
                fps = (float)(tact - startfpstime);
                fps = 100 / (fps * 0.001f);
                frames = 0;
                startfpstime = tact;
                string format = System.String.Format("{0:F2} FPS", fps);
                if (fps < 30.0f)
                {
                    textelem.color = Color.red;
                }
                else
                {
                    textelem.color = Color.green;
                }
                textelem.text = format;
            }
        }
    }
}