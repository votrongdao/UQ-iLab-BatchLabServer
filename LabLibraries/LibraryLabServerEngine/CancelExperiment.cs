using System;
using Library.Lab;

namespace Library.LabServerEngine
{
    public class CancelExperiment
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "CancelExperiment";

        //
        // Local variables
        //
        private object cancelLock;

        #endregion

        #region Public Properties

        private bool cancelled;

        public bool IsCancelled
        {
            get
            {
                lock (this.cancelLock)
                {
                    return this.cancelled;
                }
            }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public CancelExperiment()
        {
            const string STRLOG_MethodName = "CancelExperiment";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            this.cancelLock = new object();
            this.cancelled = false;

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public void Cancel()
        {
            const string STRLOG_MethodName = "Cancel";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            lock (this.cancelLock)
            {
                this.cancelled = true;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

    }
}
