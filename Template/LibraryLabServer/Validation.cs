using System;
using System.Xml;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServer.Drivers;

namespace Library.LabServer
{
    public class Validation : ExperimentValidation
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "Validation";

        //
        // String constants for error messages
        //

        //
        // Local variables
        //

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public Validation(Configuration configuration)
            : base(configuration)
        {
            const string STRLOG_MethodName = "Validation";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Get information from the validation XML node
            //
            try
            {
                //
                // YOUR CODE HERE
                //
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        //
        // YOUR CODE HERE
        //

    }
}
