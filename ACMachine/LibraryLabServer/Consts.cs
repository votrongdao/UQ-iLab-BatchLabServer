using System;

namespace Library.LabServer
{
    public class Consts
    {
        //
        // Application configuration file key strings
        //

        //
        // XML Configuration
        //
        public const string STRXML_SetupId_LockedRotor = "LockedRotor";
        public const string STRXML_SetupId_NoLoad = "NoLoad";
        public const string STRXML_SetupId_SynchronousSpeed = "SynchronousSpeed";
        public const string STRXML_SetupId_FullLoad = "FullLoad";
        public const string STRXML_measurementCount = "measurementCount";

        //
        // XML Specification and ExperimentResult
        //

        //
        // XML Validation
        //

        //
        // XML elements for the commands in the equipment request strings
        //
        public const string STRXML_CmdGetResetACDriveTime = "GetResetACDriveTime";
        public const string STRXML_CmdGetConfigureACDriveTime = "GetConfigureACDriveTime";
        public const string STRXML_CmdGetStartACDriveTime = "GetStartACDriveTime";
        public const string STRXML_CmdGetStopACDriveTime = "GetStopACDriveTime";
        public const string STRXML_CmdGetTakeMeasurementTime = "GetTakeMeasurementTime";
        public const string STRXML_CmdCreateConnection = "CreateConnection";
        public const string STRXML_CmdCloseConnection = "CloseConnection";
        public const string STRXML_CmdResetACDrive = "ResetACDrive";
        public const string STRXML_CmdConfigureACDrive = "ConfigureACDrive";
        public const string STRXML_CmdStartACDrive = "StartACDrive";
        public const string STRXML_CmdStopACDrive = "StopACDrive";
        public const string STRXML_CmdTakeMeasurement = "TakeMeasurement";

        //
        // XML elements for the command arguments in the equipment request strings
        //
        public const string STRXML_ReqACDriveConfig = "ACDriveConfig";
        public const string STRXML_ReqACDriveMode = "ACDriveMode";

        //
        // Parameters for the command arguments in the equipment request strings
        //
        public const string STR_ACDriveConfig_Default = "Default";
        public const string STR_ACDriveConfig_MaximumCurrent = "MaximumCurrent";
        public const string STR_ACDriveMode_NoLoad = "NoLoad";
        public const string STR_ACDriveMode_FullLoad = "FullLoad";
        public const string STR_ACDriveMode_LockedRotor = "LockedRotor";
        public const string STR_ACDriveMode_SynchronousSpeed = "SynchronousSpeed";

        //
        // XML elements in the equipment response strings
        //
        public const string STRXML_RspResetACDriveTime = "ResetACDriveTime";
        public const string STRXML_RspConfigureACDriveTime = "ConfigureACDriveTime";
        public const string STRXML_RspStartACDriveTime = "StartACDriveTime";
        public const string STRXML_RspStopACDriveTime = "StopACDriveTime";
        public const string STRXML_RspTakeMeasurementTime = "TakeMeasurementTime";
        public const string STRXML_RspVoltageMut = "VoltageMut";
        public const string STRXML_RspCurrentMut = "CurrentMut";
        public const string STRXML_RspPowerFactorMut = "PowerFactorMut";
        public const string STRXML_RspVoltageVsd = "VoltageVsd";
        public const string STRXML_RspCurrentVsd = "CurrentVsd";
        public const string STRXML_RspPowerFactorVsd = "PowerFactorVsd";
        public const string STRXML_RspSpeed = "Speed";
        public const string STRXML_RspTorque = "Torque";

        //
        // XML ExperimentResult
        //
        public const string STRXML_voltage = "voltage";
        public const string STRXML_current = "current";
        public const string STRXML_powerFactor = "powerFactor";
        public const string STRXML_speed = "speed";

        //
        // String constants
        //

    }
}
