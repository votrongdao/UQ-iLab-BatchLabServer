using System;
using Library.LabClient;

namespace LabClientHtml.LabControls
{
    public class ResultInfo : ExperimentResultInfo
    {
        public int speedMin;
        public int speedMax;
        public int speedStep;
        public int fieldMin;
        public int fieldMax;
        public int fieldStep;
        public int loadMin;
        public int loadMax;
        public int loadStep;
        public string speedVector;
        public string fieldVector;
        public string voltageVector;
        public string loadVector;
    }
}
