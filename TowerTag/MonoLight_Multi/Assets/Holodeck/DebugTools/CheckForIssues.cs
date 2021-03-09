using UnityEditor;
using UnityEngine;
using System.Xml;
using System;
using System.Collections.Generic;
#if UNITY_EDITOR
public class CheckForIssues : EditorWindow
{
    public static Dictionary<string, bool> HdvrCheckList;

    [MenuItem("SPREE/CheckSettingsAndFiles")]
    static void Init()
    {
        HdvrCheckList = new Dictionary<string, bool>
        {
            { "network_sec_config", false },
            { "AndroidManifest", false },
            { "PlayerPositioner", false },
        };
        EditorWindow window = GetWindowWithRect(typeof(CheckForIssues), new Rect(0, 0, 200, 200));
        window.Show();
    }

    void OnGUI()
    {
        if(HdvrCheckList == null)
        {
            Init();
        }
        if(GUILayout.Button("Check Settings And Files"))
        {
            CheckAll();
        }
        foreach(KeyValuePair<string, bool> kvp in HdvrCheckList)
        {
            Rect r = EditorGUILayout.BeginHorizontal();

            if (kvp.Value == false)
            {
                GUI.color = Color.red;   
            }
            else
            {
                GUI.color = Color.green;                
            }
            EditorGUILayout.LabelField(kvp.Key);
            GUI.color = Color.white;
            EditorGUILayout.EndHorizontal();
        }
        
    }

    void CheckAll()
    {
        CheckNetworkConfig();
        CheckAndroidManifest();
        CheckPlayerPositioner();
    }

    void CheckNetworkConfig()
    {
        try
        {
            TextAsset textXML = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/Oculus/VR/Editor/network_sec_config.xml", typeof(TextAsset));
            XmlDocument xml = new XmlDocument();
            xml.LoadXml(textXML.text);

            XmlNode node = xml.SelectSingleNode("/network-security-config");
            string attribute = node["base-config"].GetAttribute("cleartextTrafficPermitted").ToString();
            if (attribute != "true")
            {
                HdvrCheckList["network_sec_config"] = false;
                Debug.LogError("Change cleartextTrafficPermitted to true in Assets/Oculus/VR/Editor/network_sec_config.xml");
            }
            else
            {
                HdvrCheckList["network_sec_config"] = true;
                Debug.Log("network_sec_config okay!");
            }


        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        Repaint();
    }

    void CheckAndroidManifest()
    {
        try
        {
            TextAsset manifestTemplate = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/Holodeck/DebugTools/AndroidManifest_template.xml", typeof(TextAsset));
            XmlDocument manifestTemplatedoc = new XmlDocument();
            manifestTemplatedoc.LoadXml(manifestTemplate.text);

            TextAsset manifest = (TextAsset)AssetDatabase.LoadAssetAtPath("Assets/Plugins/Android/AndroidManifest.xml", typeof(TextAsset));
            XmlDocument manifesdoc = new XmlDocument();
            manifesdoc.LoadXml(manifest.text);


            if (manifest.text != manifestTemplate.text)
            {
                HdvrCheckList["AndroidManifest"] = false;
            }
            else
            {
                HdvrCheckList["AndroidManifest"] = true;
                Debug.Log("Android Manifest okay!");
            }
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        Repaint();
    }

    void CheckPlayerPositioner()
    {
        try
        {
            PlayerPositioner playerPositioner = FindObjectOfType<PlayerPositioner>();



            if (playerPositioner.forceNoBoundingBox)
            {
                HdvrCheckList["PlayerPositioner"] = false;
                Debug.LogError("Found Issue: Please uncheck PlayerPositioner.forceNoBoundingBox!");
            }
            if (playerPositioner.emulatedOrientation)
            {
                HdvrCheckList["PlayerPositioner"] = false;
                Debug.LogError("Found Issue: Please uncheck PlayerPositioner.emulatedOrientation!");
            }

            if (!playerPositioner.forceNoBoundingBox && !playerPositioner.emulatedOrientation)
            {
                HdvrCheckList["PlayerPositioner"] = true;
                Debug.Log("PlayerPositioner okay!");
            }


        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
        Repaint();
    }
}
#endif
