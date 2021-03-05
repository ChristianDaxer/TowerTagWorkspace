﻿using System.Collections.Generic;
using UnityEngine.Networking;

namespace TowerTagAPIClient {
    public static class UnityWebRequestExtension {
        public static void SetRequestHeader(this UnityWebRequest unityWebRequest, Dictionary<string, string> headers) {
            if (headers == null) return;
            foreach (KeyValuePair<string, string> header in headers) {
                unityWebRequest.SetRequestHeader(header.Key, header.Value);
            }
        }

        public static void SetJsonHeader(this UnityWebRequest unityWebRequest) {
            unityWebRequest.SetRequestHeader("accept", "application/json; charset=UTF-8");
            unityWebRequest.SetRequestHeader("content-type", "application/json; charset=UTF-8");
        }
    }
}