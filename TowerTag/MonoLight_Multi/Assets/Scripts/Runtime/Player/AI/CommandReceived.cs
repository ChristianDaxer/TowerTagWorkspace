using System;
using BehaviorDesigner.Runtime;
using BehaviorDesigner.Runtime.Tasks;
using UnityEngine;

namespace AI
{
    /// <summary>
    /// Checks if the specified command has been received
    /// </summary>
    [TaskCategory("TT Bot")]
    [Serializable]
    public class CommandReceived : Conditional
    {
        [SerializeField] private SharedBool _command;

        public override TaskStatus OnUpdate()
        {
            if (_command.Value) return TaskStatus.Success;
            return TaskStatus.Failure;
        }
    }
}