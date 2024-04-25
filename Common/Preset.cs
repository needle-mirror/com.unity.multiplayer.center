using System;
using UnityEngine;

namespace Unity.Multiplayer.Center.Common
{
    /// <summary>
    /// Game genres that can be selected. Each one is associated with pre-selected answers for the questionnaire,
    /// except for `None`.
    /// </summary>
    [Serializable]
    public enum Preset
    {
        /// <summary>
        /// Start from scratch, no preset
        /// </summary>
        [InspectorName("-")]
        None,
        
        [InspectorName("Adventure")]
        Adventure,
        
        [InspectorName("Shooter, Battle Royale, Battle Arena")]
        Shooter,
        
        [InspectorName("Racing")]
        Racing,
        
        [InspectorName("Turn-based, Card Battle, Tabletop")]
        TurnBased,
        
        [InspectorName("Simulation")]
        Simulation,
        
        [InspectorName("Strategy")]
        Strategy,
        
        [InspectorName("Sports")]
        Sports,

        [InspectorName("RolePlaying, MMO")]
        RolePlaying,
        
        [InspectorName("Async, Idle, Hyper Casual, Puzzle")]
        Async,
        
        [InspectorName("Fighting")]
        Fighting,
        
        [InspectorName("Sandbox")]
        Sandbox
    }
}
