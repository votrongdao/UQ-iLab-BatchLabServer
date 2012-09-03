using System;
using Library.LabClient;

namespace LabClientHtml.LabControls
{
    public class ResultInfo : ExperimentResultInfo
    {
        public string sourceName;
        public string[] absorbers;
        public int[] distances;
        public int duration;
        public int repeat;
        public int[,] datavectors;
    }
}
