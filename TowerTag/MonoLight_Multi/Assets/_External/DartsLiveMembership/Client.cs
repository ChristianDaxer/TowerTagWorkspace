using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Membership
{
    public class Client
    {
        /// <summary> URL for post</summary>
        //private const string POST_URL = "http://dev-golfgear.aws.dartslive.com/towertag-api/test/playlog";
        //private const string POST_URL = "https://testapi.tower-tag.net/v1/Playlog";
        private const string POST_URL = "https://testapi.tower-tag.net/v1/MatchResult";

        /// <summary> seconds to timeout </summary>
        private const int TIMEOUT = 30;

        /// <summary> times of retry </summary>
        private const int MAX_RETRY = 3;

        /// <summary>  MonoBehaviour for  通信のCorutineを動かすMonoBehaviour </summary>
        private MonoBehaviour mMonoBehaviour;

        /// <summary>
        /// constructor
        /// </summary>
        /// <param name="monoBehaviour"></param>
        public Client(MonoBehaviour monoBehaviour)
        {
            mMonoBehaviour = monoBehaviour;
        }

        /// <summary>
        /// Post Data
        /// </summary>
        /// <param name="sendData">data to send</param>
        /// <param name="callback">callback for end result(optional)</param>
        public void PostData(Membership.Model.v2.MatchResult sendData, Action<int, string> callback = null)
        {
            // NotReachable
            if (Application.internetReachability == NetworkReachability.NotReachable)
            {
                if (callback != null)
                {
                    try
                    {
                        callback(-1, "NETWORK NOT REACHABLE");
                    }
                    catch { }
                }
                return;
            }

            // send data in coroutine
            mMonoBehaviour.StartCoroutine(PostDataCoroutine(0, sendData, callback));
        }

        /// <summary>
        /// Post Request Coroutine
        /// </summary>
        /// <param name="retry">retry count</param>
        /// <param name="sendData">data to send</param>
        /// <param name="callback">callback for end result</param>
        /// <returns></returns>
        private IEnumerator PostDataCoroutine(int retry, Membership.Model.v2.MatchResult sendData, Action<int, string> callback)
        {
            /// <summary> headers </summary>
            var headers = new Dictionary<string, string>
            {
                // User-Agent
                { "user-agent", "towertag"},

                // For Json
                { "accept", "application/json; charset=UTF-8" },
                { "content-type", "application/json; charset=UTF-8" },
                { "X-HTTP-Method-Override", "POST"},
            };

            // send data
            byte[] postJson;
            try
            {
                var strJson = JsonUtility.ToJson(sendData);
                //Debug.Log(strJson);
                postJson = System.Text.Encoding.UTF8.GetBytes(strJson);
            }
            catch (System.Exception e)
            {
                Debug.LogError(e);
                yield break;
            }

            var request = UnityWebRequest.Put(POST_URL, postJson);
            request.timeout = TIMEOUT;
            request.method = "POST";
            foreach (var h in headers)
                request.SetRequestHeader(h.Key, h.Value);

#if UNITY_2017_2_OR_NEWER
            yield return request.SendWebRequest();
#else
            yield return request.Send();
#endif

            // check error
            if (request.isNetworkError)
            {
                Debug.Log(request.isNetworkError);

                // retry
                retry++;
                if (retry < MAX_RETRY)
                {
                    yield return new WaitForSeconds(3f);
                    mMonoBehaviour.StartCoroutine(PostDataCoroutine(retry, sendData, callback));
                }
                else
                {
                    // Retry Over
                    if (callback != null)
                    {
                        try
                        {
                            callback(-3, "RETRY OVER");
                        }
                        catch { }
                    }
                }
            }
            else
            {
                if (request.responseCode == 200)
                {
                    // OK
                    if (callback != null)
                    {
                        try
                        {
                            callback(0, request.downloadHandler.text);
                        }
                        catch { }
                    }
                }
                else if (request.responseCode >= 300)
                {
                    // Http Status Error
                    if (callback != null)
                    {
                        try
                        {
                            callback(-2, "HTTP STATUS " + request.responseCode);
                        }
                        catch { }
                    }
                }
            }

        }
    }
}
