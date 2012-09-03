using System;

namespace Library.LabEquipment
{
    public class Consts
    {
        //
        // XML elements in the equipment request strings
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
        // XML elements in the EquipmentConfig.xml file
        //
        public const string STRXML_machineIP = "machineIP";
        public const string STRXML_machinePort = "machinePort";
        public const string STRXML_initialiseEquipment = "initialiseEquipment";
        public const string STRXML_measurementDelay = "measurementDelay";

    }
}
