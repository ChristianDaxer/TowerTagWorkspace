using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class OculusClientStatistics : ClientStatistics
{
    private string cloudStorageDataStatisticsPath;
    private const string statisticsJsonFileName = "statistics.json";
    public override void StoreStatisticsInDictionary()
    {
        var request = Oculus.Platform.CloudStorage2.GetUserDirectoryPath();
        request.OnComplete((message) =>
        {
            if (message.IsError)
            {
                Debug.LogErrorFormat("Error occurred while attempting to access Oculus cloud storage: \n{0}", message.GetError().Message);
                return;
            }

            cloudStorageDataStatisticsPath = $"{message.Data}{statisticsJsonFileName}".Replace('\\', '/');
            Debug.LogFormat("Built statistics cloud storage path path: \"{0}\".", cloudStorageDataStatisticsPath);

            LoadStatistics();
            base.StoreStatisticsInDictionary();
        });
    }

    private void LoadStatistics ()
    {
        if (string.IsNullOrEmpty(cloudStorageDataStatisticsPath))
            return;

        try
        {
            if (!File.Exists(cloudStorageDataStatisticsPath))
                WriteStatistics();

            var statisticsString = File.ReadAllText(cloudStorageDataStatisticsPath);
            SetStatistics(JsonConvert.DeserializeObject<Dictionary<string, int>>(statisticsString));
            Debug.LogFormat("Loaded statistics from cloud storage path: \"{0}\".", cloudStorageDataStatisticsPath);

        } catch (System.Exception exception)
        {
            Debug.LogErrorFormat("Exception occurred while attempting to read/parse local statistics file: \"{0}\".", cloudStorageDataStatisticsPath);
            Debug.LogException(exception);
        }
    }

    private void OnApplicationQuit() => WriteStatistics();
    private void WriteStatistics ()
    {
        if (string.IsNullOrEmpty(cloudStorageDataStatisticsPath))
            return;

        var statisticsString = JsonConvert.SerializeObject(Statistics);
        try
        {
            string directory = Directory.GetDirectoryRoot(cloudStorageDataStatisticsPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
                Debug.LogFormat("Created directory tree at path: \"{0}\".", directory);
            }

            File.WriteAllText(cloudStorageDataStatisticsPath, statisticsString);
            Debug.LogFormat("Wrote statistics to cloud storage path: \"{0}\".", cloudStorageDataStatisticsPath);

        } catch (System.Exception exception)
        {
            Debug.LogErrorFormat("Exception occurred while attempting to write local statistics file: \"{0}\".", cloudStorageDataStatisticsPath);
            Debug.LogException(exception);
        }
    }
}
