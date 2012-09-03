using System;

namespace Library.LabEquipment
{
    public class Consts
    {
        //
        // XML elements in the equipment request strings
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
        // XML elements in the EquipmentConfig.xml file
        //
        public const string STRXML_hardwarePresent = "hardwarePresent";
        public const string STRXML_flexMotion = "flexMotion";
        public const string STRXML_boardID = "boardID";
        public const string STRXML_axisId = "axisId";
        public const string STRXML_tube = "tube";
        public const string STRXML_offsetDistance = "offsetDistance";
        public const string STRXML_homeDistance = "homeDistance";
        public const string STRXML_moveRate = "moveRate";
        public const string STRXML_initAxis = "initAxis";
        public const string STRXML_sources = "sources";
        public const string STRXML_absorbers = "absorbers";
        public const string STRXML_encoderPositions = "encoderPositions";
        public const string STRXML_selectTimes = "selectTimes";
        public const string STRXML_returnTimes = "returnTimes";
        public const string STRXML_firstLocation = "firstLocation";
        public const string STRXML_homeLocation = "homeLocation";
        public const string STRXML_initialiseDelay = "initialiseDelay";
        public const string STRXML_serialLcd = "serialLcd";
        public const string STRXML_type = "type";
        public const string STRXML_network = "network";
        public const string STRXML_ipaddr = "ipaddr";
        public const string STRXML_port = "port";
        public const string STRXML_serial = "serial";
        public const string STRXML_baud = "baud";
        public const string STRXML_writeLineTime = "writeLineTime";
        public const string STRXML_radiationCounter = "radiationCounter";
        public const string STRXML_st360Counter = "st360Counter";
        public const string STRXML_voltage = "voltage";
        public const string STRXML_volume = "volume";
        public const string STRXML_physicsCounter = "physicsCounter";
        public const string STRXML_timeAdjustment = "timeAdjustment";
        public const string STRXML_capture = "capture";

    }

}
