using System;

namespace Library.LabServer
{
    public class Consts
    {
        //
        // XML Configuration
        //
        public const string STRXML_SetupId_OpenCircuitVaryField = "OpenCircuitVaryField";
        public const string STRXML_SetupId_OpenCircuitVarySpeed = "OpenCircuitVarySpeed";
        public const string STRXML_SetupId_ShortCircuitVaryField = "ShortCircuitVaryField";
        public const string STRXML_SetupId_PreSynchronisation = "PreSynchronisation";
        public const string STRXML_SetupId_Synchronisation = "Synchronisation";

        //
        // XML elements for the commands in the equipment request strings
        //
        public const string STRXML_CmdGetExecutionTime = "GetExecutionTime";
        public const string STRXML_CmdGetExecutionTimeRemaining = "GetExecutionTimeRemaining";
        public const string STRXML_CmdGetExecutionStatus = "GetExecutionStatus";
        public const string STRXML_CmdGetExecutionResultStatus = "GetExecutionResultStatus";
        public const string STRXML_CmdGetExecutionResults = "GetExecutionResults";
        public const string STRXML_CmdStartExecution = "StartExecution";

        //
        // XML elements for the command arguments in the equipment request strings
        //
        public const string STRXML_ReqSpecification = "Specification";

        //
        // XML elements in the equipment response strings
        //
        public const string STRXML_RspExecutionTime = "ExecutionTime";
        public const string STRXML_RspExecutionStatus = "ExecutionStatus";
        public const string STRXML_RspExecutionTimeRemaining = "ExecutionTimeRemaining";
        public const string STRXML_RspExecutionResultStatus = "ExecutionResultStatus";
        public const string STRXML_RspExecutionResults = "ExecutionResults";

        //
        // XML ExperimentResult
        //
        public const string STRXML_measurements = "measurements";
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
        public const string STRXMLPARAM_name = "@name";
        public const string STRXMLPARAM_units = "@units";
        public const string STRXMLPARAM_format = "@format";

    }
}
