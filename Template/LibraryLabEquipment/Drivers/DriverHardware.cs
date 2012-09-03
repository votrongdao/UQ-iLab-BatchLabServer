using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Threading;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class DriverHardware : IDisposable
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "DriverHardware";

        //
        // String constants for logfile messages
        //
        protected const string STRLOG_NotInitialised = " Not Initialised!";
        protected const string STRLOG_Initialising = " Initialising...";
        protected const string STRLOG_Online = " Online: ";
        protected const string STRLOG_disposing = " disposing: ";
        protected const string STRLOG_disposed = " disposed: ";
        protected const string STRLOG_Success = " Success: ";

        //
        // String constants for error messages
        //

        //
        // Local variables
        //
        protected Logfile.LoggingLevels logLevel;
        protected bool initialised;
        protected bool configured;
        private string lastError;
        private bool disposed;

        #endregion

        #region Public Properties

        //
        // Minimum power-up and initialise delays in seconds
        //
        public const int DELAY_POWERUP = 5;
        public const int DELAY_INITIALISE = 1;

        protected int initialiseDelay;
        protected bool online;
        protected string statusMessage;

        /// <summary>
        /// Returns the time (in seconds) that it takes for the equipment to initialise.
        /// </summary>
        public int InitialiseDelay
        {
            get { return this.initialiseDelay; }
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

        public DriverHardware(XmlNode xmlNodeEquipmentConfig)
        {
            const string STRLOG_MethodName = "DriverHardware";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise local variables
            //
            this.disposed = true;
            this.initialised = false;
            this.lastError = null;
            this.configured = false;

            //
            // Initialise properties
            //
            this.initialiseDelay = DELAY_INITIALISE;
            this.online = false;
            this.statusMessage = STRLOG_NotInitialised;

            //
            // Determine the logging level for this class
            //
            try
            {
                this.logLevel = (Logfile.LoggingLevels)Utilities.GetIntAppSetting(STRLOG_ClassName);
            }
            catch
            {
                this.logLevel = Logfile.LoggingLevels.Minimum;
            }
            Logfile.Write(Logfile.STRLOG_LogLevel + this.logLevel.ToString());

            //
            // YOUR CODE HERE
            //

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public string GetLastError()
        {
            string lastError = this.lastError;
            this.lastError = null;
            return lastError;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual bool Initialise(bool configure)
        {
            //
            // There is now some disposing to do
            //
            this.disposed = false;

            return true;
        }

        //-------------------------------------------------------------------------------------------------//

        protected bool Configure()
        {
            const string STRLOG_MethodName = "Configure";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            try
            {
                //
                // YOUR CODE HERE
                //

                this.configured = true;
                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
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
            Dispose(true);

            // Take yourself off the Finalization queue to prevent finalization code for this object
            // from executing a second time.
            GC.SuppressFinalize(this);
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Use C# destructor syntax for finalization code. This destructor will run only if the Dispose
        /// method does not get called. It gives your base class the opportunity to finalize. Do not provide
        /// destructors in types derived from this class.
        /// </summary>
        ~DriverHardware()
        {
            Trace.WriteLine("~DriverHardware():");

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

            string logMessage = STRLOG_disposing + disposing.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_disposed + this.disposed.ToString();

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
                if (this.configured == true)
                {
                    //
                    // YOUR CODE HERE
                    //
                }

                if (this.initialised == true)
                {
                    //
                    // YOUR CODE HERE
                    //
                }

                this.disposed = true;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        #endregion

        //=================================================================================================//


    }
}
