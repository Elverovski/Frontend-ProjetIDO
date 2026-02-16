using System;
using UnityEngine;

namespace Robot.Control.Commands
{
    /// <summary>
    /// Base class for all robot commands
    /// All commands must be serializable to JSON
    /// </summary>
    [Serializable]
    public abstract class BaseCommand
    {
        [SerializeField] public string type;
        
        /// <summary>
        /// Convert command to JSON string
        /// </summary>
        public virtual string ToJson()
        {
            return JsonUtility.ToJson(this);
        }
        
        /// <summary>
        /// Get command type
        /// </summary>
        public string GetCommandType() => type;
    }
}