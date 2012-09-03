using System;
using System.Diagnostics;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class Hardware : IDisposable
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "Hardware";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_NotInitialised = " Not Initialised!";
        private const string STRLOG_Initialising = " Initialising...";
        private const string STRLOG_Online = " Online: ";

        //
        // String constants for error messages
        //

        //
        // Local variables
        //
        private bool disposed;
        private bool initialised;
        private string lastError;

        #endregion

        #region Properties

        //
        // Minimum power-up and initialise delays in seconds
        //
        public const int DELAY_POWERUP = 5;
        public const int DELAY_INITIALISE = 3;

        private bool online;
        private string statusMessage;

        /// <summary>
        /// Returns the time (in seconds) that it takes for the equipment to initialise.
        /// </summary>
        public int InitialiseDelay
        {
            get { return (this.initialised == false) ? DELAY_INITIALISE : 0; }
        }

        /// <summary>
        /// Returns true if the hardware has been initialised successfully and is ready for use.
        /// </summary>
        public bool Online
        {
            get { return this.online; }
        }

        public string StatusMessage
        {
            get { return this.statusMessage; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public Hardware()
        {
            const string STRLOG_MethodName = "Hardware";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise local variables
            //
            this.disposed = false;
            this.initialised = false;
            this.lastError = null;

            //
            // Initialise properties
            //
            this.online = false;
            this.statusMessage = STRLOG_NotInitialised;

            //
            // YOUR CODE HERE
            //
        }

        //-------------------------------------------------------------------------------------------------//

        public string GetLastError()
        {
            string lastError = this.lastError;
            this.lastError = null;
            return lastError;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool Initialise()
        {
            const string STRLOG_MethodName = "Initialise";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            if (this.initialised == false)
            {
                this.statusMessage = STRLOG_Initialising;

                try
                {
                    //
                    // YOUR CODE HERE
                    //

                    //
                    // Initialisation is complete
                    //
                    this.initialised = true;
                    this.online = true;
                    this.statusMessage = StatusCodes.Ready.ToString();
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);

                    //
                    // YOUR CODE HERE
                    //
                }
            }

            string logMessage = STRLOG_Online + this.online.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return this.online;
        }

        //-------------------------------------------------------------------------------------------------//

        #region Close and Dispose

        /// <summary>
        /// Do not make this method virtual. A derived class should not be allowed to override this method.
        /// </summary>
        public void Close()
        {
            const string STRLOG_MethodName = "Close";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            // Calls the Dispose method without parameters
            Dispose();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Implement IDisposable. Do not make this method virtual. A derived class should not be able
        /// to override this method.
        /// </summary>
        public void Dispose()
        {
            const string STRLOG_MethodName = "Dispose";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            Dispose(true);

            // Take yourself off the Finalization queue to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Use C# destructor syntax for finalization code. This destructor will run only if the Dispose
        /// method does not get called. It gives your base class the opportunity to finalize. Do not provide
        /// destructors in types derived from this class.
        /// </summary>
        ~Hardware()
        {
            Trace.WriteLine("~Hardware():");

            //
            // Do not re-create Dispose clean-up code here. Calling Dispose(false) is optimal in terms of
            // readability and maintainability.
            //
            Dispose(false);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios:
        /// 1. If disposing equals true, the method has been called directly or indirectly by a user's code.
        ///    Managed and unmanaged resources can be disposed.
        /// 2. If disposing equals false, the method has been called by the runtime from inside the finalizer
        ///    and you should not reference other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            const string STRLOG_MethodName = "Dispose";

            string logMessage = " disposing: " + this.disposed.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Check to see if Dispose has already been called
            //
            if (this.disposed == false)
            {
                //
                // If disposing equals true, dispose all managed and unmanaged resources.
                //
                if (disposing == true)
                {
                    // Dispose managed resources here. Anything that has a Dispose() method.
                }

                //
                // Release unmanaged resources here. If disposing is false, only the following
                // code is executed.
                //

                //
                // YOUR CODE HERE
                //

                this.disposed = true;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        #endregion

    }
}
