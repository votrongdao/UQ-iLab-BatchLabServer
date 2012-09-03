using System;
using Library.LabServerEngine;

namespace Library.LabServer
{
    public class ResultInfo : ExperimentResultInfo
    {
        public float voltage;
        public float current;
        public float powerFactor;
        public int speed;
        public int torque;

        //-------------------------------------------------------------------------------------------------//

        public ResultInfo()
            : base()
        {
        }
    }

}
