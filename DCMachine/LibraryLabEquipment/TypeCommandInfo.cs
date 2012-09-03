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
        GetResetACDriveTime, GetConfigureACDriveTime, GetStartACDriveTime, GetStopACDriveTime,
        GetResetDCDriveMutTime, GetConfigureDCDriveMutTime, GetStartDCDriveMutTime, GetStopDCDriveMutTime,
        GetSetSpeedACDriveTime, GetSetSpeedDCDriveMutTime, GetSetTorqueDCDriveMutTime, GetSetFieldDCDriveMutTime,
        GetTakeMeasurementTime,
        GetACDriveInfo, GetDCDriveMutInfo
    }

    /// <summary>
    /// These commands control and/or communicate with the equipment. These commands will
    /// completes execution before another command can start execution.
    /// </summary>
    public enum ExecuteCommands
    {
        CreateConnection, CloseConnection,
        ResetACDrive, ConfigureACDrive, StartACDrive, StopACDrive,
        ResetDCDriveMut, ConfigureDCDriveMut, StartDCDriveMut, StopDCDriveMut,
        SetSpeedACDrive, SetSpeedDCDriveMut, SetTorqueDCDriveMut, SetFieldDCDriveMut,
        TakeMeasurement
    }

}
