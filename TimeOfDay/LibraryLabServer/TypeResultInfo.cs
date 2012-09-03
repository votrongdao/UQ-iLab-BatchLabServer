using System;
using Library.LabServerEngine;

namespace Library.LabServer
{
    public class ResultInfo : ExperimentResultInfo
    {
        public DateTime dateTime;
        public string timeofday;

        //-------------------------------------------------------------------------------------------------//

        public ResultInfo()
            : base()
        {
        }
    }

}
