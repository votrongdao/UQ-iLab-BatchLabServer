using System;

namespace LabClientHtml.LabControls
{
    public class LabConsts
    {
        //
        // XML string constants for setup IDs
        //
        public const string STRXML_SetupId_OpenCircuitVaryField = "OpenCircuitVaryField";
        public const string STRXML_SetupId_OpenCircuitVarySpeed = "OpenCircuitVarySpeed";
        public const string STRXML_SetupId_ShortCircuitVaryField = "ShortCircuitVaryField";
        public const string STRXML_SetupId_PreSynchronisation = "PreSynchronisation";
        public const string STRXML_SetupId_Synchronisation = "Synchronisation";

        //
        // XML Configuration
        //

        //
        // XML Validation
        //

        //
        // XML Specification and ExperimentResult
        //

        //
        // XML ExperimentResult
        //
        public const string STRXML_measurements = "measurements";
        public const string STRXMLPARAM_name = "@name";
        public const string STRXMLPARAM_units = "@units";
        public const string STRXMLPARAM_format = "@format";
        public const string STRXML_fieldCurrent = "fieldCurrent";
        public const string STRXML_speed = "speed";
        public const string STRXML_voltage = "voltage";
        public const string STRXML_statorCurrent = "statorCurrent";
        public const string STRXML_speedSetpoint = "speedSetpoint";
        public const string STRXML_mainsVoltage = "mainsVoltage";
        public const string STRXML_mainsFrequency = "mainsFrequency";
        public const string STRXML_syncVoltage = "syncVoltage";
        public const string STRXML_syncFrequency = "syncFrequency";
        public const string STRXML_syncMainsPhase = "syncMainsPhase";
        public const string STRXML_synchronism = "synchronism";
        public const string STRXML_torqueSetpoint = "torqueSetpoint";
        public const string STRXML_powerFactor = "powerFactor";
        public const string STRXML_realPower = "realPower";
        public const string STRXML_reactivePower = "reactivePower";
        public const string STRXML_phaseCurrent = "phaseCurrent";

        //
        // ExperimentResult strings
        //
        public const string STR_FieldCurrent = "Field Current (Amps)";
        public const string STR_MotorSpeed = "Motor Speed (RPM)";
        public const string STR_ArmatureVoltage = "Armature Voltage (Volts)";
    }
}
