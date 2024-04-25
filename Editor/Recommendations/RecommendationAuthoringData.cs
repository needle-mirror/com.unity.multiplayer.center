#if MULTIPLAYER_CENTER_DEV_MODE
using System;
using Unity.Multiplayer.Center.Questionnaire;
namespace Unity.Multiplayer.Center.Recommendations
{
    /// <summary>
    /// This file contains all data relevant for authoring recommendations.
    /// </summary>
    
    internal static class Packages
    {
        public const string VivoxId = "com.unity.services.vivox";
        public const string MultiplayerSdkId = "com.unity.services.multiplayer";
        public const string MultiplayerWidgetsId = "com.unity.multiplayer.widgets";
        public const string NetcodeForEntitiesId = "com.unity.netcode";
        public const string NetcodeGameObjectsId = "com.unity.netcode.gameobjects";
        public const string MultiplayerToolsId = "com.unity.multiplayer.tools";
        public const string MultiplayerPlayModeId = "com.unity.multiplayer.playmode";
        public const string CloudCodeId = "com.unity.services.cloudcode";
        public const string CustomNetcodeId = "com.unity.transport";
        public const string DeploymentPackageId = "com.unity.services.deployment";
        public const string DedicatedServerPackageId = "com.unity.dedicated-server";
    }
    internal static class Reasons
    {
        public const string MultiplayerPlayModeRecommended = "Multiplayer Play Mode will speed up your workflow, no matter the netcode choice.";
        public const string MultiplayerPlayModeNotRecommended = "Multiplayer Play Mode is not useful without a netcode solution.";
        public const string VivoxRecommended = "Adding real-time communication is always recommended to engage your users.";
        public const string MultiplayerToolsRecommended = "Multiplayer Tools will speed up your workflow, no matter the netcode choice.";
        public const string MultiplayerToolsIncompatible = "Multiplayer Tools is only compatible with Netcode for GameObjects.";
        public const string CloudCodeNotRecommended = "Cloud Code is not recommended for real-time multiplayer games.";
        public const string CloudCodeRecommended = "Cloud Code is recommended for asynchronous games.";
        public const string MultiplayerSdkRecommendedClientHosted = "The Multiplayer Services Sessions are typically used with a client hosted game.";
        public const string MultiplayerSdkRecommendedDedicatedServer = "The Multiplayer Services package's Multiplay Hosting and Matchmaking are Unity services that allow you to host your dedicated server in the cloud and connect players.";
        public const string MultiplayerSdkNotRecommendedCloudCode = "The Multiplayer Services package is not recommended when using Cloud Code.";
        public const string MultiplayerWidgetsRecommendedClientHosted = "The Multiplayer Widgets package helps with the rapid prototyping of the Multiplayer Services Session features.";
        public const string MultiplayerWidgetsNotRecommendedCloudCode = "The Multiplayer Widgets package is not recommended when using Cloud Code.";
        public const string DedicatedServerPackageRecommended = "The Dedicated Server package helps with the development for the dedicated server platform.";
        public const string DedicatedServerPackageNotRecommended = "The Dedicated Server package is only useful when you have a dedicated server architecture and use Netcode for Gameobjects.";
        public const string DedicatedServerPackageNotRecommendedN4E = "The Dedicated Server package is currently not compatible when using Netcode for Entities.";
    }
    
    internal static class DocLinks
    {
        public const string NetworkTopology = "https://docs-multiplayer.unity3d.com/netcode/current/terms-concepts/network-topologies/";
        public const string DedicatedServer = "https://docs-multiplayer.unity3d.com/netcode/current/terms-concepts/network-topologies/#dedicated-game-server";
        public const string ListenServer = "https://docs-multiplayer.unity3d.com/netcode/current/terms-concepts/network-topologies/#client-hosted-listen-server";
        public const string NetcodeForGameObjects = "https://docs-multiplayer.unity3d.com/netcode/current/about/";
        public const string NetcodeForEntities = "https://docs.unity3d.com/Packages/com.unity.netcode@latest";
        public const string Vivox = "https://docs.unity.com/ugs/en-us/manual/vivox-unity/manual/Unity/Unity";
        public const string MultiplayerTools = "https://docs-multiplayer.unity3d.com/tools/current/about/";
        public const string MultiplayerPlayMode = "https://docs-multiplayer.unity3d.com/mppm/current/about/";
        public const string MultiplayerSdk = null;
        public const string MultiplayerWidgets = null;
        public const string CloudCode = "https://docs.unity.com/ugs/manual/cloud-code/manual";
        public const string CustomNetcode = "https://docs.unity3d.com/Packages/com.unity.transport@latest";
        public const string DedicatedServerPackage = "https://docs.unity3d.com/Packages/com.unity.dedicated-server@latest";
    }
    
    internal static class ShortDescriptions
    {
        public const string NetworkTopology = "Network topology is the way in which your game is connected to the network. It is generally a dedicated server or a listen server architecture.";
        public const string DedicatedServer = "You will have to deploy a dedicated server to have the authority on your game logic.";
        public const string ListenServer = "A player will be the host of your game. You will not need to deploy a dedicated server to have the authority on your game logic. This is also called a Listen Server Architecture.";
        public const string NetcodeForGameObjects = "Netcode for GameObjects is a Unity package that allows you to create multiplayer games using the GameObjects paradigm.";
        public const string NetcodeForEntities = "Netcode for Entities is a Unity package that allows you to create multiplayer games using the ECS paradigm.";
        public const string Vivox = "Vivox is a Unity service that allows you to add voice and text chat to your game.";
        public const string MultiplayerTools = "Multiplayer Tools is a Unity package that allows you to analyze, debug, and test your multiplayer game.";
        public const string MultiplayerPlayMode = "Multiplayer Play Mode is a Unity package that allows you to test your game with several windows in the Editor, without building multiple Players.";
        public const string MultiplayerSdk = "The Multiplayer Services package allows you to create and handle Sessions in order to connect players via Lobbies, Matchmaking, Multiplay Hosting and Relay.";
        public const string MultiplayerWidgets = "The Multiplayer Widgets package provides a set of widgets that help implementing Unity's multiplayer services in a rapid prototyping fashion.";
        public const string DedicatedServerPackage = "The Dedicated Server package contains optimizations and workflow improvements for developing Dedicated Server platform.";
        public const string CloudCode = "Run your game logic in the cloud as serverless functions and interact with other backend services.";
        public const string CustomNetcode = "Use the Unity network transport layer as the low-level interface for connecting and sending data through a network for your custom netcode solution.";
        public const string NoNetcode = "No real time synchronization of game data required.";
    }

    internal static class Titles
    {
        public const string NetworkTopology = "Network topology";
        public const string DedicatedServer = "Dedicated Server";
        public const string ListenServer = "Client Hosted";
        public const string NetcodeForGameObjects = "Netcode for GameObjects";
        public const string NetcodeForEntities = "Netcode for Entities";
        public const string Vivox = "Voice/Text chat (Vivox)";
        public const string MultiplayerTools = "Multiplayer Tools";
        public const string MultiplayerPlayMode = "Multiplayer Play Mode";
        public const string NoUnityNetcode = "No Netcode";
        public const string MultiplayerSdk = "Multiplayer Services";
        public const string MultiplayerWidgets = "Multiplayer Widgets";
        public const string CloudCode = "Cloud Code";
        public const string CustomNetcode = "Custom Netcode (with Unity Transport)";
        public const string DedicatedServerPackage = "Dedicated Server Package";
        public const string DeploymentPackage = "Deployment Package";
    }

    static class RecommendationAssetUtils
    {
        public static RecommenderSystemData PopulateDefaultRecommendationData()
        {
            var data = new RecommenderSystemData();
            data.TargetUnityVersion = UnityEngine.Application.unityVersion;
            data.RecommendedSolutions = new RecommendedSolution[]
            {
                new()
                {
                    Type = PossibleSolution.LS,
                    Title = Titles.ListenServer,
                    DocUrl = DocLinks.ListenServer,
                    ShortDescription = ShortDescriptions.ListenServer,
                    RecommendedPackages = new RecommendedPackage[]
                    {
                        new(Packages.VivoxId, RecommendationType.OptionalFeatured, Reasons.VivoxRecommended),
                        new(Packages.MultiplayerSdkId, RecommendationType.OptionalStandard, Reasons.MultiplayerSdkRecommendedClientHosted),
                        new(Packages.DedicatedServerPackageId, RecommendationType.NotRecommended, Reasons.DedicatedServerPackageNotRecommended),
                        new(Packages.MultiplayerWidgetsId, RecommendationType.OptionalStandard, Reasons.MultiplayerWidgetsRecommendedClientHosted),
                    }
                },
                new()
                {
                    Type = PossibleSolution.DS,
                    Title = Titles.DedicatedServer,
                    DocUrl = DocLinks.DedicatedServer,
                    ShortDescription = ShortDescriptions.DedicatedServer,
                    RecommendedPackages = new RecommendedPackage[]
                    {
                        new(Packages.VivoxId, RecommendationType.OptionalFeatured, Reasons.VivoxRecommended),
                        new(Packages.MultiplayerSdkId, RecommendationType.OptionalStandard, Reasons.MultiplayerSdkRecommendedDedicatedServer),
                        new(Packages.DedicatedServerPackageId, RecommendationType.OptionalFeatured, Reasons.DedicatedServerPackageRecommended),
                        new(Packages.MultiplayerWidgetsId, RecommendationType.OptionalStandard, Reasons.MultiplayerWidgetsRecommendedClientHosted),
                    }
                },
                new()
                {
                    Type = PossibleSolution.CloudCode,
                    Title = Titles.CloudCode,
                    MainPackageId = Packages.CloudCodeId,
                    ShortDescription = ShortDescriptions.CloudCode,
                    DocUrl = DocLinks.CloudCode,
                    
                    RecommendedPackages = new RecommendedPackage[]
                    {
                        new(Packages.VivoxId, RecommendationType.OptionalFeatured, Reasons.VivoxRecommended),
                        new(Packages.MultiplayerSdkId, RecommendationType.NotRecommended, Reasons.MultiplayerSdkNotRecommendedCloudCode),
                        new(Packages.DedicatedServerPackageId, RecommendationType.NotRecommended, Reasons.DedicatedServerPackageNotRecommended),
                        new(Packages.MultiplayerWidgetsId, RecommendationType.NotRecommended, Reasons.MultiplayerWidgetsNotRecommendedCloudCode),
                    }
                },
                new()
                {
                    Type = PossibleSolution.NGO,
                    Title = Titles.NetcodeForGameObjects,
                    MainPackageId = Packages.NetcodeGameObjectsId,
                    ShortDescription = ShortDescriptions.NetcodeForGameObjects,
                    RecommendedPackages = new RecommendedPackage[]
                    {
                        new(Packages.MultiplayerPlayModeId, RecommendationType.OptionalStandard, Reasons.MultiplayerPlayModeRecommended),
                        new(Packages.MultiplayerToolsId, RecommendationType.OptionalStandard, Reasons.MultiplayerToolsRecommended)
                    }
                },
                new()
                {
                    Type = PossibleSolution.N4E,
                    Title = Titles.NetcodeForEntities,
                    MainPackageId = Packages.NetcodeForEntitiesId,
                    ShortDescription = ShortDescriptions.NetcodeForEntities,
                    RecommendedPackages = new RecommendedPackage[]
                    {
                        new(Packages.MultiplayerPlayModeId, RecommendationType.OptionalStandard, Reasons.MultiplayerPlayModeRecommended),
                        new(Packages.MultiplayerToolsId, RecommendationType.Incompatible, Reasons.MultiplayerToolsIncompatible)
                    }
                },
                new()
                {
                    Type = PossibleSolution.NoNetcode,
                    Title = Titles.NoUnityNetcode,
                    DocUrl = null,
                    MainPackageId = null,
                    ShortDescription = ShortDescriptions.NoNetcode,
                    RecommendedPackages = new RecommendedPackage[]
                    {
                        new(Packages.MultiplayerPlayModeId, RecommendationType.OptionalStandard, Reasons.MultiplayerPlayModeNotRecommended),
                        new(Packages.MultiplayerToolsId, RecommendationType.Incompatible, Reasons.MultiplayerToolsIncompatible)
                    }
                },
                new()
                {
                    Type = PossibleSolution.CustomNetcode,
                    Title = Titles.CustomNetcode,
                    MainPackageId = Packages.CustomNetcodeId,
                    ShortDescription = ShortDescriptions.CustomNetcode,
                    RecommendedPackages = new RecommendedPackage[]
                    {
                        new(Packages.MultiplayerPlayModeId, RecommendationType.OptionalStandard, Reasons.MultiplayerPlayModeRecommended),
                        new(Packages.MultiplayerToolsId, RecommendationType.Incompatible, Reasons.MultiplayerToolsIncompatible)
                    }
                }
            };
            data.Packages = new PackageDetails[]
            {
                new(Packages.VivoxId, Titles.Vivox, ShortDescriptions.Vivox, DocLinks.Vivox),
                new(Packages.MultiplayerSdkId, Titles.MultiplayerSdk, ShortDescriptions.MultiplayerSdk, DocLinks.MultiplayerSdk),
                new(Packages.MultiplayerWidgetsId, Titles.MultiplayerWidgets, ShortDescriptions.MultiplayerWidgets, DocLinks.MultiplayerWidgets),
                new(Packages.NetcodeForEntitiesId, Titles.NetcodeForEntities, ShortDescriptions.NetcodeForEntities, DocLinks.NetcodeForEntities),
                new(Packages.NetcodeGameObjectsId, Titles.NetcodeForGameObjects, ShortDescriptions.NetcodeForGameObjects, DocLinks.NetcodeForGameObjects),
                new(Packages.MultiplayerToolsId, Titles.MultiplayerTools, ShortDescriptions.MultiplayerTools, DocLinks.MultiplayerTools),
                new(Packages.MultiplayerPlayModeId, Titles.MultiplayerPlayMode, ShortDescriptions.MultiplayerPlayMode, DocLinks.MultiplayerPlayMode),
                new(Packages.CloudCodeId, Titles.CloudCode, ShortDescriptions.CloudCode, DocLinks.CloudCode, new string[]{Packages.DeploymentPackageId}),
                new(Packages.CustomNetcodeId, Titles.CustomNetcode, ShortDescriptions.CustomNetcode, DocLinks.CustomNetcode),
                new(Packages.DedicatedServerPackageId, Titles.DedicatedServerPackage, ShortDescriptions.DedicatedServerPackage, DocLinks.DedicatedServerPackage),
                new(Packages.DeploymentPackageId, Titles.DeploymentPackage, "Not shown in UI - Will be bundled with Cloud Code", ""),
            };

            return data;
        }    
    }
    
}
#endif