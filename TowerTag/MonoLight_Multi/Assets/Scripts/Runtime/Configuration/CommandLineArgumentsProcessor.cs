using System;
using Hologate;
using TowerTagSOES;

public static class CommandLineArgumentsProcessor {
    public static void ProcessCommandLineArguments(SharedControllerType controllerType) {
        ProcessCommandLineArguments(Environment.GetCommandLineArgs(), controllerType);
    }

    public static void ProcessCommandLineArguments(string[] args, SharedControllerType controllerType)
    {
        Configuration config = ConfigurationManager.Configuration;
        var controllerTypeSet = false;
        if (TowerTagSettings.Hologate)
        {
            HologateManager.InitFromMachineData(controllerType);
            controllerTypeSet = true;
        }
        foreach (string arg in args)
        {
            if (!TowerTagSettings.Hologate)
            {
                if (arg.Equals("-vr"))
                {
                    if (controllerTypeSet) Debug.LogWarning("Conflicting settings for controller type");
                    controllerTypeSet = true;
                    controllerType.Set(typeof(CommandLineArgumentsProcessor),
                        ControllerType.VR);
                }
                else if (arg.Equals("-fps"))
                {
                    if (controllerTypeSet) Debug.LogWarning("Conflicting settings for controller type");
                    controllerTypeSet = true;
                    controllerType.Set(typeof(CommandLineArgumentsProcessor),
                        ControllerType.NormalFPS);
                }
                else if (arg.Equals("-admin"))
                {
                    if (controllerTypeSet) Debug.LogWarning("Conflicting settings for controller type");
                    controllerTypeSet = true;
                    controllerType.Set(typeof(CommandLineArgumentsProcessor),
                        ControllerType.Admin);
                }
                else if (arg.Equals("-spectator"))
                {
                    if (controllerTypeSet) Debug.LogWarning("Conflicting settings for controller type");
                    controllerTypeSet = true;
                    controllerType.Set(typeof(CommandLineArgumentsProcessor), ControllerType.Spectator);
                }
                else if (arg.Equals("-poCtrlr"))
                {
                    if (controllerTypeSet) Debug.LogWarning("Conflicting settings for controller type");
                    controllerTypeSet = true;
                    controllerType.Set(typeof(CommandLineArgumentsProcessor), ControllerType.PillarOffsetController);
                }
            }
            if (arg.Equals("-autostart"))
            {
                BalancingConfiguration.Singleton.AutoStart = true;
            }
            else if (arg.StartsWith("-scene="))
            {
                BalancingConfiguration.Singleton.AutoStartSceneName = arg.Replace("-scene=", "").Trim('"');
            }
            else if (arg.StartsWith("-team="))
            {
                if (int.TryParse(arg.Replace("-team=", ""), out int teamId))
                {
                    config.TeamID = teamId;
                }
            }
            else if (arg.StartsWith("-name="))
            {
                string playerName = arg.Replace("-name=", "").Trim('"');

                if (!string.IsNullOrEmpty(playerName))
                {
                    if (PlayerProfileManager.CurrentPlayerProfile == null)
                    {
                        PlayerProfileManager.CreateNew();
                    }

                    if (PlayerProfileManager.CurrentPlayerProfile != null)
                        PlayerProfileManager.CurrentPlayerProfile.PlayerName = playerName;
                }
            }
            else if (arg.StartsWith("-room="))
            {
                string roomName = arg.Replace("-room=", "").Trim('"');
                ConfigurationManager.Configuration.Room = roomName;
            }
            else if (arg.StartsWith("-ip="))
            {
                string ip = arg.Replace("-ip=", "").Trim('"');
                ConfigurationManager.Configuration.PlayInLocalNetwork = true;
                ConfigurationManager.Configuration.ServerIp = ip;
                ConfigurationManager.Configuration.ServerPort = 5055;
            }
        }
    }
}