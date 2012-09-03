using System;

namespace Library.LabServer
{
    public class Consts
    {
        //
        // Application configuration file key strings
        //
        public const string STRCFG_MeasurementCount = "MeasurementCount";

        //
        // XML Configuration
        //
        public const string STRXML_SetupId_VoltageVsSpeed = "VoltageVsSpeed";
        public const string STRXML_SetupId_VoltageVsField = "VoltageVsField";
        public const string STRXML_SetupId_VoltageVsLoad = "VoltageVsLoad";
        public const string STRXML_SetupId_SpeedVsVoltage = "SpeedVsVoltage";
        public const string STRXML_SetupId_SpeedVsField = "SpeedVsField";

        //
        // XML Specification and ExperimentResult
        //
        public const string STRXML_speedMin = "speedMin";
        public const string STRXML_speedMax = "speedMax";
        public const string STRXML_speedStep = "speedStep";
        public const string STRXML_fieldMin = "fieldMin";
        public const string STRXML_fieldMax = "fieldMax";
        public const string STRXML_fieldStep = "fieldStep";
        public const string STRXML_loadMin = "loadMin";
        public const string STRXML_loadMax = "loadMax";
        public const string STRXML_loadStep = "loadStep";

        //
        // XML Validation
        //
        public const string STRXML_vdnSpeed = "vdnSpeed";
        public const string STRXML_vdnField = "vdnField";
        public const string STRXML_vdnLoad = "vdnLoad";
        public const string STRXML_minimum = "minimum";
        public const string STRXML_maximum = "maximum";
        public const string STRXML_stepMin = "stepMin";
        public const string STRXML_stepMax = "stepMax";

        //
        // XML elements for the commands in the equipment request strings
        //
        public const string STRXML_CmdGetResetACDriveTime = "GetResetACDriveTime";
        public const string STRXML_CmdGetConfigureACDriveTime = "GetConfigureACDriveTime";
        public const string STRXML_CmdGetStartACDriveTime = "GetStartACDriveTime";
        public const string STRXML_CmdGetStopACDriveTime = "GetStopACDriveTime";
        public const string STRXML_CmdGetResetDCDriveMutTime = "GetResetDCDriveMutTime";
        public const string STRXML_CmdGetConfigureDCDriveMutTime = "GetConfigureDCDriveMutTime";
        public const string STRXML_CmdGetStartDCDriveMutTime = "GetStartDCDriveMutTime";
        public const string STRXML_CmdGetStopDCDriveMutTime = "GetStopDCDriveMutTime";
        public const string STRXML_CmdGetSetSpeedACDriveTime = "GetSetSpeedACDriveTime";
        public const string STRXML_CmdGetSetSpeedDCDriveMutTime = "GetSetSpeedDCDriveMutTime";
        public const string STRXML_CmdGetSetTorqueDCDriveMutTime = "GetSetTorqueDCDriveMutTime";
        public const string STRXML_CmdGetSetFieldDCDriveMutTime = "GetSetFieldDCDriveMutTime";
        public const string STRXML_CmdGetTakeMeasurementTime = "GetTakeMeasurementTime";
        public const string STRXML_CmdGetACDriveInfo = "GetACDriveInfo";
        public const string STRXML_CmdGetDCDriveMutInfo = "GetDCDriveMutInfo";
        public const string STRXML_CmdCreateConnection = "CreateConnection";
        public const string STRXML_CmdCloseConnection = "CloseConnection";
        public const string STRXML_CmdResetACDrive = "ResetACDrive";
        public const string STRXML_CmdConfigureACDrive = "ConfigureACDrive";
        public const string STRXML_CmdStartACDrive = "StartACDrive";
        public const string STRXML_CmdStopACDrive = "StopACDrive";
        public const string STRXML_CmdResetDCDriveMut = "ResetDCDriveMut";
        public const string STRXML_CmdConfigureDCDriveMut = "ConfigureDCDriveMut";
        public const string STRXML_CmdStartDCDriveMut = "StartDCDriveMut";
        public const string STRXML_CmdStopDCDriveMut = "StopDCDriveMut";
        public const string STRXML_CmdSetSpeedACDrive = "SetSpeedACDrive";
        public const string STRXML_CmdSetSpeedDCDriveMut = "SetSpeedDCDriveMut";
        public const string STRXML_CmdSetTorqueDCDriveMut = "SetTorqueDCDriveMut";
        public const string STRXML_CmdSetFieldDCDriveMut = "SetFieldDCDriveMut";
        public const string STRXML_CmdTakeMeasurement = "TakeMeasurement";

        //
        // XML elements for the command arguments in the equipment request strings
        //
        public const string STRXML_ReqACDriveConfig = "ACDriveConfig";
        public const string STRXML_ReqDCDriveMutConfig = "DCDriveMutConfig";
        public const string STRXML_ReqDCDriveMutMode = "DCDriveMutMode";
        public const string STRXML_ReqSpeedACDrive = "SpeedACDrive";
        public const string STRXML_ReqTorqueDCDriveMut = "TorqueDCDriveMut";
        public const string STRXML_ReqSpeedDCDriveMut = "SpeedDCDriveMut";
        public const string STRXML_ReqFieldDCDriveMut = "FieldDCDriveMut";

        //
        // Parameters for the command arguments in the equipment request strings
        //
        public const string STR_ACDriveConfig_Default = "Default";
        public const string STR_ACDriveConfig_LowerCurrent = "LowerCurrent";
        public const string STR_ACDriveConfig_MaximumCurrent = "MaximumCurrent";
        public const string STR_ACDriveConfig_MinimumTorque = "MinimumTorque";
        public const string STR_DCDriveMutConfig_Default = "Default";
        public const string STR_DCDriveMutConfig_MinimumTorque = "MinimumTorque";
        public const string STR_DCDriveMutMode_EnableOnly = "EnableOnly";
        public const string STR_DCDriveMutMode_Speed = "Speed";
        public const string STR_DCDriveMutMode_Torque = "Torque";

        //
        // XML elements in the equipment response strings
        //
        public const string STRXML_RspResetACDriveTime = "ResetACDriveTime";
        public const string STRXML_RspConfigureACDriveTime = "ConfigureACDriveTime";
        public const string STRXML_RspStartACDriveTime = "StartACDriveTime";
        public const string STRXML_RspStopACDriveTime = "StopACDriveTime";
        public const string STRXML_RspResetDCDriveMutTime = "ResetDCDriveMutTime";
        public const string STRXML_RspConfigureDCDriveMutTime = "ConfigureDCDriveMutTime";
        public const string STRXML_RspStartDCDriveMutTime = "StartDCDriveMutTime";
        public const string STRXML_RspStopDCDriveMutTime = "StopDCDriveMutTime";
        public const string STRXML_RspSetSpeedACDriveTime = "SetSpeedACDriveTime";
        public const string STRXML_RspSetTorqueDCDriveMutTime = "SetTorqueDCDriveMutTime";
        public const string STRXML_RspSetSpeedDCDriveMutTime = "SetSpeedDCDriveMutTime";
        public const string STRXML_RspSetFieldDCDriveMutTime = "SetFieldDCDriveMutTime";
        public const string STRXML_RspTakeMeasurementTime = "TakeMeasurementTime";
        public const string STRXML_RspMinSpeed = "MinSpeed";
        public const string STRXML_RspMaxSpeed = "MaxSpeed";
        public const string STRXML_RspDefaultField = "DefaultField";
        public const string STRXML_RspDefaultTorque = "DefaultTorque";
        public const string STRXML_RspSpeed = "Speed";
        public const string STRXML_RspVoltage = "Voltage";
        public const string STRXML_RspFieldCurrent = "FieldCurrent";
        public const string STRXML_RspLoad = "Load";

        //
        // XML ExperimentResult
        //
        public const string STRXML_speedVector = "speedVector";
        public const string STRXML_fieldVector = "fieldVector";
        public const string STRXML_voltageVector = "voltageVector";
        public const string STRXML_loadVector = "loadVector";
        public const char CHR_Splitter = ',';

        //
        // String constants
        //

    }
}
