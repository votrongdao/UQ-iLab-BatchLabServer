using System;
using Library.LabClient;

namespace LabClientHtml.LabControls
{
    public class ResultInfo : ExperimentResultInfo
    {
        public string serverUrl;
        public string formatName;
        public string timeofday;
        public string dayofweek;
        public int day;
        public int month;
        public int year;
        public int hours;
        public int minutes;
        public int seconds;

        // Previous version
        public string serverName;
    }
}
