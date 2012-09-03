using System;
using System.Xml;
using Library.Lab;
using Library.LabServerEngine;

namespace Library.LabServer
{
    public class Configuration : LabConfiguration
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "Configuration";

        //
        // String constants for the XML lab configuration
        //

        //
        // String constants for logfile messages
        //
        private const string STRLOG_MeasurementCount = " MeasurementCount: ";

        //
        // String constants for error messages
        //
        private const string STRERR_NumberIsNegative = "Number is negative!";

        #endregion

        #region Properties

        private int measurementCount;

        public int MeasurementCount
        {
            get { return this.measurementCount; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public Configuration(string rootFilePath)
            : this(rootFilePath, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public Configuration(string rootFilePath, string xmlLabConfiguration)
            : base(rootFilePath, xmlLabConfiguration)
        {
            const string STRLOG_MethodName = "Configuration";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            try
            {
                //
                // Get the number of measurements to take
                //
                this.measurementCount = Utilities.GetIntAppSetting(Consts.STRCFG_MeasurementCount);
                if (this.measurementCount < 0)
                {
                    throw new Exception(STRERR_NumberIsNegative);
                }
                Logfile.Write(STRLOG_MeasurementCount + this.measurementCount.ToString());
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

    }
}
