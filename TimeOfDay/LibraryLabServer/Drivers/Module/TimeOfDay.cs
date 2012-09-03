using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Xml;
using Library.Lab;

namespace Library.LabServer.Drivers.Module
{
    public class TimeOfDay
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "TimeOfDay";

        //
        // Constants
        //

        //
        // String constants for logfile messages
        //
        private const string STRLOG_TimeOfDay = " TimeOfDay: ";

        //
        // Local variables
        //

        #endregion

        //---------------------------------------------------------------------------------------//

        public TimeOfDay(Configuration configuration)
        {
            const string STRLOG_MethodName = "TimeOfDay";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Nothing to do here
            //

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public DateTime GetTimeOfDay(int executionTime)
        {
            const string STRLOG_MethodName = "GetTimeOfDay";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Need to run for the full execution time, so get the time now
            //
            DateTime dateTimeStart = DateTime.Now;

            //
            // Get the LabServer's local clock time
            //
            DateTime dateTime = DateTime.Now;

            //
            // Delay for the full execution time
            //
            DateTime dateTimeEnd = dateTimeStart + new TimeSpan(0, 0, executionTime);
            while (DateTime.Now < dateTimeEnd)
            {
                Trace.Write("L");
                Thread.Sleep(1000);
            }
            Trace.WriteLine("");

            string logMessage = STRLOG_TimeOfDay + dateTime.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return dateTime;
        }

    }
}
