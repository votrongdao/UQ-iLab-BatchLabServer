using System;
using Library.LabServerEngine;

namespace Library.LabServer
{
    public class ResultInfo : ExperimentResultInfo
    {
        public int[] speedVector;
        public int[] voltageVector;
        public int[] loadVector;
        public float[] fieldVector;

        //-------------------------------------------------------------------------------------------------//

        public ResultInfo()
            : base()
        {
        }
    }

}
