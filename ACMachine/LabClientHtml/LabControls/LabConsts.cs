using System;

namespace LabClientHtml.LabControls
{
    public class LabConsts
    {
        //
        // XML string constants for setup IDs
        //
        public const string STRXML_SetupId_LockedRotor = "LockedRotor";
        public const string STRXML_SetupId_NoLoad = "NoLoad";
        public const string STRXML_SetupId_SynchronousSpeed = "SynchronousSpeed";
        public const string STRXML_SetupId_FullLoad = "FullLoad";

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
        public const string STRXML_voltage = "voltage";
        public const string STRXML_current = "current";
        public const string STRXML_powerFactor = "powerFactor";
        public const string STRXML_speed = "speed";

        //
        // ExperimentResult strings
        //
        public const string STR_PhasePhaseVoltage = "Ph-Ph Voltage (Volts)";
        public const string STR_PhaseCurrent = "Phase Current (Amps)";
        public const string STR_PowerFactor = "Power Factor (Avg)";
        public const string STR_MotorSpeed = "Motor Speed (RPM)";

    }
}
