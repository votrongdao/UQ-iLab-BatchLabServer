using System;
using Library.LabServerEngine;

namespace Library.LabServer
{
    public enum DataTypes
    {
        Unknown, Real, Simulated, Calculated
    };

    public class ResultInfo : ExperimentResultInfo
    {
        public DataTypes dataType;
        public int[,] dataVectors;

        //-------------------------------------------------------------------------------------------------//

        public ResultInfo()
            : base()
        {
            this.dataType = DataTypes.Unknown;
        }
    }

}
