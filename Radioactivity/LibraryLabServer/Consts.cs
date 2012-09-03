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
        public const string STRXML_SetupId_RadioactivityVsTime = "RadioactivityVsTime";
        public const string STRXML_SetupId_RadioactivityVsDistance = "RadioactivityVsDistance";
        public const string STRXML_SetupId_RadioactivityVsAbsorber = "RadioactivityVsAbsorber";
        public const string STRXML_SetupId_SimActivityVsTime = "SimActivityVsTime";
        public const string STRXML_SetupId_SimActivityVsDistance = "SimActivityVsDistance";
        public const string STRXML_SetupId_SimActivityVsTimeNoDelay = "SimActivityVsTimeNoDelay";
        public const string STRXML_SetupId_SimActivityVsDistanceNoDelay = "SimActivityVsDistanceNoDelay";

        //
        // XML Specification and ExperimentResult
        //
        public const string STRXML_sourceName = "sourceName";
        public const string STRXML_absorberName = "absorberName";
        public const string STRXML_distance = "distance";
        public const string STRXML_duration = "duration";
        public const string STRXML_repeat = "repeat";
        public const char CHR_CsvSplitter = ',';

        //
        // XML Validation
        //
        public const string STRXML_vdnDistance = "vdnDistance";
        public const string STRXML_vdnDuration = "vdnDuration";
        public const string STRXML_vdnRepeat = "vdnRepeat";
        public const string STRXML_vdnTotaltime = "vdnTotaltime";
        public const string STRXML_minimum = "minimum";
        public const string STRXML_maximum = "maximum";

        //
        // XML elements for the commands in the equipment request strings
        //
        public const string STRXML_CmdGetLcdWriteLineTime = "GetLcdWriteLineTime";
        public const string STRXML_CmdGetTubeHomeDistance = "GetTubeHomeDistance";
        public const string STRXML_CmdGetTubeMoveTime = "GetTubeMoveTime";
        public const string STRXML_CmdGetSourceHomeLocation = "GetSourceHomeLocation";
        public const string STRXML_CmdGetSourceSelectTime = "GetSourceSelectTime";
        public const string STRXML_CmdGetSourceReturnTime = "GetSourceReturnTime";
        public const string STRXML_CmdGetAbsorberHomeLocation = "GetAbsorberHomeLocation";
        public const string STRXML_CmdGetAbsorberSelectTime = "GetAbsorberSelectTime";
        public const string STRXML_CmdGetAbsorberReturnTime = "GetAbsorberReturnTime";
        public const string STRXML_CmdGetCaptureDataTime = "GetCaptureDataTime";
        public const string STRXML_CmdSetTubeDistance = "SetTubeDistance";
        public const string STRXML_CmdSetSourceLocation = "SetSourceLocation";
        public const string STRXML_CmdSetAbsorberLocation = "SetAbsorberLocation";
        public const string STRXML_CmdGetCaptureData = "GetCaptureData";
        public const string STRXML_CmdWriteLcdLine = "WriteLcdLine";

        //
        // XML elements for the command arguments in the equipment request strings
        //
        public const string STRXML_ReqTubeHomeDistance = "TubeHomeDistance";
        public const string STRXML_ReqTubeDistanceFrom = "TubeDistanceFrom";
        public const string STRXML_ReqTubeDistanceTo = "TubeDistanceTo";
        public const string STRXML_ReqTubeDistance = "TubeDistance";
        public const string STRXML_ReqSourceLocation = "SourceLocation";
        public const string STRXML_ReqAbsorberLocation = "AbsorberLocation";
        public const string STRXML_ReqDuration = "Duration";
        public const string STRXML_ReqLcdLineNo = "LcdLineNo";
        public const string STRXML_ReqLcdMessage = "LcdMessage";

        //
        // XML elements in the equipment response strings
        //
        public const string STRXML_RspLcdWriteLineTime = "LcdWriteLineTime";
        public const string STRXML_RspTubeHomeDistance = "TubeHomeDistance";
        public const string STRXML_RspTubeMoveTime = "TubeMoveTime";
        public const string STRXML_RspSourceHomeLocation = "SourceHomeLocation";
        public const string STRXML_RspSourceSelectTime = "SourceSelectTime";
        public const string STRXML_RspSourceReturnTime = "SourceReturnTime";
        public const string STRXML_RspAbsorberHomeLocation = "AbsorberHomeLocation";
        public const string STRXML_RspAbsorberSelectTime = "AbsorberSelectTime";
        public const string STRXML_RspAbsorberReturnTime = "AbsorberReturnTime";
        public const string STRXML_RspCaptureDataTime = "CaptureDataTime";
        public const string STRXML_RspCount = "Count";

        //
        // XML ExperimentResult
        //
        public const string STRXML_dataType = "dataType";
        public const string STRXML_dataVector = "dataVector";

        //
        // XML elements in the SimulationConfig.xml file
        //
        public const string STRXML_simulationConfig = "simulationConfig";
        public const string STRXMLPARAM_title = "@title";
        public const string STRXMLPARAM_version = "@version";
        public const string STRXML_simDistance = "distance";
        public const string STRXML_simDuration = "duration";
        public const string STRXML_simMean = "mean";
        public const string STRXML_simPower = "power";
        public const string STRXML_simDeviation = "deviation";
        public const string STRXML_tube = "tube";
        public const string STRXML_offsetDistance = "offsetDistance";
        public const string STRXML_homeDistance = "homeDistance";
        public const string STRXML_moveRate = "moveRate";
        public const string STRXML_sources = "sources";
        public const string STRXML_absorbers = "absorbers";
        public const string STRXML_selectTimes = "selectTimes";
        public const string STRXML_returnTimes = "returnTimes";
        public const string STRXML_firstLocation = "firstLocation";
        public const string STRXML_homeLocation = "homeLocation";

    }
}
