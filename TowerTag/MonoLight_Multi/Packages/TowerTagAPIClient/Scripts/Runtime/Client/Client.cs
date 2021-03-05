using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace TowerTagAPIClient {
    public static class Client {
        public static void Get(string uri, Dictionary<string, string> headers,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Get(uri);
            request.SetRequestHeader(headers);

            UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = request.SendWebRequest();
            unityWebRequestAsyncOperation.completed +=
                operation => OnCompletion(successCallback, errorCallback, request);
        }

        public static void Post(string uri, Dictionary<string, string> data,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Post(uri, data);

            UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = request.SendWebRequest();
            unityWebRequestAsyncOperation.completed +=
                operation => OnCompletion(successCallback, errorCallback, request);
        }

        public static void Post(string uri, Dictionary<string, string> headers, Dictionary<string, string> data,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Post(uri, data);
            request.SetRequestHeader(headers);

            UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = request.SendWebRequest();
            unityWebRequestAsyncOperation.completed +=
                operation => OnCompletion(successCallback, errorCallback, request);
        }

        public static void Post(string uri, Dictionary<string, string> headers, string data,
            Action<long, string> successCallback, Action<long, string> errorCallback, int tries) {
            if (tries == 0)
                return;

            if (tries == 1) {
                Post(uri, headers, data, successCallback, errorCallback);
            }
            else {
                Post(uri, headers, data, successCallback, (status, response) => {
                    Debug.LogWarning($"Failed post with error ${status}:{response}. Trying again.");
                    Post(uri, headers, data, successCallback, errorCallback, --tries);
                });
            }
        }

        public static void Post(string uri, Dictionary<string, string> headers, string data,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Put(uri, Encoding.UTF8.GetBytes(data));
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("X-HTTP-Method-Override", "POST");
            request.SetRequestHeader(headers);
            request.SetJsonHeader();

            UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = request.SendWebRequest();
            unityWebRequestAsyncOperation.completed +=
                operation => OnCompletion(successCallback, errorCallback, request);
        }

        public static void Put(string uri, Dictionary<string, string> headers, Dictionary<string, string> data,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Post(uri, data);
            request.SetRequestHeader(headers);
            request.method = UnityWebRequest.kHttpVerbPUT;

            UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = request.SendWebRequest();
            unityWebRequestAsyncOperation.completed +=
                operation => OnCompletion(successCallback, errorCallback, request);
        }

        public static void Put(string uri, Dictionary<string, string> headers, string data,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Put(uri, Encoding.UTF8.GetBytes(data));
            request.method = UnityWebRequest.kHttpVerbPUT;
            request.SetRequestHeader(headers);
            request.SetJsonHeader();

            UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = request.SendWebRequest();
            unityWebRequestAsyncOperation.completed +=
                operation => OnCompletion(successCallback, errorCallback, request);
        }

        public static void Patch(string uri, Dictionary<string, string> headers, Dictionary<string, string> data,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Post(uri, data);
            request.method = "PATCH";
            request.SetRequestHeader(headers);

            UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = request.SendWebRequest();
            unityWebRequestAsyncOperation.completed +=
                operation => OnCompletion(successCallback, errorCallback, request);
        }

        public static void Patch(string uri, Dictionary<string, string> headers,
            string data, Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Put(uri, Encoding.UTF8.GetBytes(data));
            request.method = "PATCH";
            request.SetRequestHeader(headers);
            request.SetJsonHeader();

            UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = request.SendWebRequest();
            unityWebRequestAsyncOperation.completed +=
                operation => OnCompletion(successCallback, errorCallback, request);
        }

        public static void Delete(string uri, Dictionary<string, string> headers,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Delete(uri);
            request.SetRequestHeader(headers);

            UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = request.SendWebRequest();
            unityWebRequestAsyncOperation.completed +=
                operation => OnCompletion(successCallback, errorCallback, request);
        }

        public static void Head(string uri, Dictionary<string, string> headers,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Head(uri);
            request.SetRequestHeader(headers);

            UnityWebRequestAsyncOperation unityWebRequestAsyncOperation = request.SendWebRequest();
            unityWebRequestAsyncOperation.completed +=
                operation => OnCompletion(successCallback, errorCallback, request);
        }

        private static void OnCompletion(Action<long, string> successCallback, Action<long, string> errorCallback,
            UnityWebRequest request) {
            if (!request.isHttpError && !request.isNetworkError)
                successCallback(
                    request.responseCode,
                    request.downloadHandler != null ? request.downloadHandler.text : "");
            else
                errorCallback(
                    request.responseCode,
                    request.downloadHandler != null ? request.error + "\n" + request.downloadHandler.text : "");
        }
    }
}