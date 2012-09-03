using System;
using Library.Lab;

namespace Library.LabServerEngine
{
    public class ExperimentResultInfo
    {
        public StatusCodes statusCode;
        public string errorMessage;

        //-------------------------------------------------------------------------------------------------//

        public ExperimentResultInfo()
        {
            this.statusCode = StatusCodes.Unknown;
            this.errorMessage = string.Empty;
        }

        //-------------------------------------------------------------------------------------------------//

    }
}
