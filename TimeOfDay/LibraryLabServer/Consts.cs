using System;

namespace Library.LabServer
{
    public class Consts
    {
        //
        // Application configuration file key strings
        //
        public const string STRCFG_NtpServerTimeout = "NtpServerTimeout";

        //
        // XML Configuration
        //
        public const string STRXML_SetupId_NTPServer = "NTPServer";
        public const string STRXML_SetupId_LocalClock = "LocalClock";
        public const string STRXML_Format_12Hour = "12-Hour";
        public const string STRXML_Format_24Hour = "24-Hour";

        //
        // XML Specification and ExperimentResult
        //
        public const string STRXML_serverUrl = "serverUrl";
        public const string STRXML_formatName = "formatName";

        //
        // XML Validation
        //

        //
        // XML elements for the commands in the equipment request strings
        //
        public const string STRXML_CmdGetTimeOfDayTime = "GetTimeOfDayTime";
        public const string STRXML_CmdGetTimeOfDay = "GetTimeOfDay";

        //
        // XML elements for the command arguments in the equipment request strings
        //
        public const string STRXML_ReqServerUrl = "ServerUrl";

        //
        // XML elements in the equipment response strings
        //
        public const string STRXML_RspTimeOfDayTime = "TimeOfDayTime";
        public const string STRXML_RspTimeOfDay = "TimeOfDay";

        //
        // XML ExperimentResult
        //
        public const string STRXML_timeofday = "timeofday";
        public const string STRXML_dayofweek = "dayofweek";
        public const string STRXML_day = "day";
        public const string STRXML_month = "month";
        public const string STRXML_year = "year";
        public const string STRXML_hours = "hours";
        public const string STRXML_minutes = "minutes";
        public const string STRXML_seconds = "seconds";

        //
        // String constants
        //
        public const string STR_DateTimeFormat_12Hour = "F";
        public const string STR_DateTimeFormat_24Hour = "ddd dd MMM yyyy - HH:mm:ss";

    }
}
