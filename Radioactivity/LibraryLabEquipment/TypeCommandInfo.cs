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
        GetTubeHomeDistance, GetTubeMoveTime,
        GetSourceHomeLocation, GetSourceSelectTime, GetSourceReturnTime,
        GetAbsorberHomeLocation, GetAbsorberSelectTime, GetAbsorberReturnTime,
        GetLcdWriteLineTime, GetCaptureDataTime,
    }

    /// <summary>
    /// Commands that control and/or communicate with the equipment. These commands will
    /// completes execution before another command can start execution.
    /// </summary>
    public enum ExecuteCommands
    {
        SetTubeDistance, SetSourceLocation, SetAbsorberLocation,
        WriteLcdLine, GetCaptureData,
    }

}
