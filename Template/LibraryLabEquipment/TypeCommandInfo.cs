using System;
using Library.LabEquipment.Engine;

namespace Library.LabEquipment
{
    public class CommandInfo : ExecuteCommandInfo
    {
        public CommandInfo(object command)
            : base(command)
        {
        }
    }

    /// <summary>
    /// These commands do not control and/or communicate with the equipment. They may get
    /// the time it takes for the equipment to do something. Multiple commands may be
    /// executed at the same time.
    /// </summary>
    public enum NonExecuteCommands
    {
        // Remove this command in your implementation
        GetTimeToDoSomething
    }

    /// <summary>
    /// These commands control and/or communicate with the equipment. These commands will
    /// complete execution before another command can start execution.
    /// </summary>
    public enum ExecuteCommands
    {
        // Remove this command in your implementation
        DoSomething
    }

}
