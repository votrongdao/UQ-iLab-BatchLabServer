using System;
using Library.Lab;

namespace Library.LabClient
{
    public class ExperimentResultInfo
    {
        public string timestamp;
        public string title;
        public string version;
        public int experimentId;
        public int unitId;
        public string setupId;
        public string setupName;
        public string dataType;
        public StatusCodes statusCode;
        public string errorMessage;
    }
}
