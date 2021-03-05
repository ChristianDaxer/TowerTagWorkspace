using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine.Networking;
using VRNerdsUtilities;

namespace REST {
    public class Client : SingletonMonoBehaviour<Client> {
        public static void Get(string uri, Dictionary<string, string> headers,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Get(uri);
            request.SetRequestHeader(headers);
            Instance.StartCoroutine(SendRequest(request, successCallback, errorCallback));
        }

        public static void Post(string uri, Dictionary<string, string> data,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Post(uri, data);
            Instance.StartCoroutine(SendRequest(request, successCallback, errorCallback));
        }

        public static void Post(string uri, Dictionary<string, string> headers, Dictionary<string, string> data,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Post(uri, data);
            request.SetRequestHeader(headers);
            Instance.StartCoroutine(SendRequest(request, successCallback, errorCallback));
        }

        public static void Post(string uri, Dictionary<string, string> headers, string data,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Put(uri, Encoding.UTF8.GetBytes(data));
            request.method = UnityWebRequest.kHttpVerbPOST;
            request.SetRequestHeader("X-HTTP-Method-Override", "POST");
            request.SetRequestHeader(headers);
            request.SetJsonHeader();
            Instance.StartCoroutine(SendRequest(request, successCallback, errorCallback));
        }

        public static void Put(string uri, Dictionary<string, string> headers, Dictionary<string, string> data,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Post(uri, data);
            request.SetRequestHeader(headers);
            request.method = UnityWebRequest.kHttpVerbPUT;
            Instance.StartCoroutine(SendRequest(request, successCallback, errorCallback));
        }

        public static void Put(string uri, Dictionary<string, string> headers, string data,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Put(uri, Encoding.UTF8.GetBytes(data));
            request.method = UnityWebRequest.kHttpVerbPUT;
            request.SetRequestHeader(headers);
            request.SetJsonHeader();
            Instance.StartCoroutine(SendRequest(request, successCallback, errorCallback));
        }

        public static void Patch(string uri, Dictionary<string, string> headers, Dictionary<string, string> data,
            Action<long, string> successCallback, Action<long, string> errorCallback) {

            UnityWebRequest request = UnityWebRequest.Post(uri, data);
            request.method = "PATCH";
            request.SetRequestHeader(headers);

            Instance.StartCoroutine(SendRequest(request, successCallback, errorCallback));
        }

        public static void Patch(string uri, Dictionary<string, string> headers,
            string data, Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Put(uri, Encoding.UTF8.GetBytes(data));
            request.method = "PATCH";
            request.SetRequestHeader(headers);
            request.SetJsonHeader();
            Instance.StartCoroutine(SendRequest(request, successCallback, errorCallback));
        }

        public static void Delete(string uri, Dictionary<string, string> headers,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Delete(uri);
            request.SetRequestHeader(headers);
            Instance.StartCoroutine(SendRequest(request, successCallback, errorCallback));
        }

        public static void Head(string uri, Dictionary<string, string> headers,
            Action<long, string> successCallback, Action<long, string> errorCallback) {
            UnityWebRequest request = UnityWebRequest.Head(uri);
            request.SetRequestHeader(headers);
            Instance.StartCoroutine(SendRequest(request, successCallback, errorCallback));
        }

        private static IEnumerator SendRequest(
            UnityWebRequest request,
            Action<long, string> successCallback,
            Action<long, string> errorCallback) {
            yield return request.SendWebRequest();

            if (request.isNetworkError || request.isHttpError) {
                errorCallback(request.responseCode, request.error + "\n" + request.downloadHandler.text);
            }
            else {
                successCallback(request.responseCode,
                    request.downloadHandler != null ? request.downloadHandler.text : "");
            }
        }
    }
}