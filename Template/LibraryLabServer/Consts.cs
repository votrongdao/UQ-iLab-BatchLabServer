using System;

namespace Library.LabServer
{
    public class Consts
    {
        //
        // Application configuration file key strings
        //
        public const string STRCFG_XmlSimulationConfigFilename = "XmlSimulationConfigFilename";

        //
        // XML Configuration
        //
        public const string STRXML_SetupId_Equipment = "Equipment";
        public const string STRXML_SetupId_Simulation = "Simulation";

        //
        // XML Specification and ExperimentResult
        //

        //
        // XML Validation
        //

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

        //
        // XML elements in the SimulationConfig.xml file
        //
        public const string STRXML_simulationConfig = "simulationConfig";
        public const string STRXMLPARAM_title = "@title";
        public const string STRXMLPARAM_version = "@version";

    }
}
