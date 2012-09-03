using System;

namespace LabClientHtml.LabControls
{
    public class LabConsts
    {
        //
        // XML string constants for setup IDs
        //
        public const string STRXML_SetupId_VoltageVsSpeed = "VoltageVsSpeed";
        public const string STRXML_SetupId_VoltageVsField = "VoltageVsField";
        public const string STRXML_SetupId_VoltageVsLoad = "VoltageVsLoad";
        public const string STRXML_SetupId_SpeedVsVoltage = "SpeedVsVoltage";
        public const string STRXML_SetupId_SpeedVsField = "SpeedVsField";

        //
        // XML Configuration
        //
        public const string STRXML_ParamsTitle = "paramsTitle";

        //
        // XML Configuration, Specification and ExperimentResult
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
        // XML ExperimentResult
        //
        public const string STRXML_speedVector = "speedVector";
        public const string STRXML_fieldVector = "fieldVector";
        public const string STRXML_voltageVector = "voltageVector";
        public const string STRXML_loadVector = "loadVector";
        public const char CHR_Splitter = ',';

        //
        // ExperimentSpecification strings
        //
        public const string STR_MinSpeed = "Min Speed (RPM)";
        public const string STR_MaxSpeed = "Max Speed (RPM)";
        public const string STR_SpeedStep = "Speed Step (RPM)";
        public const string STR_MinField = "Min Field (%)";
        public const string STR_MaxField = "Max Field (%)";
        public const string STR_FieldStep = "Field Step (%)";
        public const string STR_MinLoad = "Min Load (%)";
        public const string STR_MaxLoad = "Max Load (%)";
        public const string STR_LoadStep = "Load Step (%)";

        //
        // ExperimentSpecification ToolTip strings
        //
        public const string STR_Range = "Range: ";
        public const string STR_to = " to ";

        //
        // ExperimentResult strings
        //
        public const string STR_MotorSpeed = "Motor Speed (RPM)";
        public const string STR_FieldCurrent = "Field Current (A)";
        public const string STR_ArmatureVoltage = "Armature Voltage (V)";
        public const string STR_LoadTorque = "Load Torque (%)";
    }
}
