using System;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine.Drivers;

namespace Library.LabEquipment.Drivers
{
    public class DriverMachine : DriverGeneric
    {
        #region Constants

        private const string STRLOG_ClassName = "DriverMachine";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_PLC = "PLC:";
        private const string STRLOG_DCDrive = "DCDrive:";

        //
        // String constants for error messages
        //
        private const string STRERR_FailedToInitialise = "Failed to initialise! ";
        private const string STRERR_EmergencyStopActive = "Emergency stop is active!";
        private const string STRERR_ProtectionCircuitBreakerTripped = "Protection circuit breaker has tripped!";

        #endregion

        #region Types

        public delegate void KeepAliveCallback();

        #endregion

        #region Variables

        private static bool firstInstance = true;
        private static DCDrive dcDriveInstance;
        private static PLC plcInstance;
        protected DCDrive dcDrive;
        protected PLC plc;

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public DriverMachine(XmlNode xmlNodeEquipmentConfig, Specification specification)
            : base(xmlNodeEquipmentConfig, specification)
        {
            const string STRLOG_MethodName = "DriverMachine";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            try
            {
                //
                // Initialise static variables
                //
                if (DriverMachine.firstInstance == true)
                {
                    DriverMachine.dcDriveInstance = new DCDrive(xmlNodeEquipmentConfig, KeepAlive);
                    DriverMachine.plcInstance = new PLC(xmlNodeEquipmentConfig, KeepAlive);
                    DriverMachine.initialiseDelay = DriverMachine.dcDriveInstance.InitialiseDelay + DriverMachine.plcInstance.InitialiseDelay;
                    DriverMachine.statusMessage = STRLOG_PLC + STRLOG_NotInitialised + STRLOG_DCDrive + STRLOG_NotInitialised;
                    DriverMachine.firstInstance = false;
                }

                //
                // Provide access to the instances
                //
                this.dcDrive = dcDriveInstance;
                this.plc = plcInstance;
            }
            catch (Exception ex)
            {
                //
                // Log the message and throw the exception back to the caller
                //
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public DCDrive GetDCDrive()
        {
            return dcDrive;
        }

        //-------------------------------------------------------------------------------------------------//

        public PLC GetPLC()
        {
            return plc;
        }

        //-------------------------------------------------------------------------------------------------//

        public void KeepAlive()
        {
            if (this.dcDrive != null)
            {
                this.dcDrive.KeepAlive();
            }
            if (this.plc != null)
            {
                this.plc.KeepAlive();
            }
        }

        //-------------------------------------------------------------------------------------------------//

        protected bool OpenConnection()
        {
            bool success = false;

            if ((success = this.dcDrive.OpenConnection()) == true)
            {
                success = this.plc.OpenConnection();
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected bool CloseConnection()
        {
            bool success = false;

            if ((success = this.dcDrive.CloseConnection()) == true)
            {
                success = this.plc.CloseConnection();
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Check status of the emergency stop button and protection circuit breaker. If either is active,
        /// a message is placed in 'lastError' and false is returned.
        /// </summary>
        /// <returns>True if the emergency stop button is not active and the protection circuit breaker
        /// has not tripped.</returns>
        protected bool CheckStatus()
        {
            bool success;
            bool status = false;

            //
            // Check if emergency shutdown button is active
            //
            success = this.plc.GetEmergencyStoppedStatus(ref status);
            if (success == true)
            {
                if (status == true)
                {
                    this.lastError = STRERR_EmergencyStopActive;
                    Logfile.WriteError(this.lastError);
                    Trace.WriteLine(this.lastError);
                    success = false;
                }
                else
                {
                    //
                    // Check if protection circuit breaker has tripped
                    //
                    success = this.plc.GetProtectionCBTrippedStatus(ref status);
                    if (success == true)
                    {
                        if (status == true)
                        {
                            this.lastError = STRERR_ProtectionCircuitBreakerTripped;
                            Logfile.WriteError(this.lastError);
                            Trace.WriteLine(this.lastError);
                            success = false;
                        }
                    }
                }
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        protected void WaitDelay(int seconds)
        {
            for (int i = 0; i < seconds; i++)
            {
                Trace.Write(".");
                Thread.Sleep(1000);

                this.KeepAlive();
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public override bool Initialise()
        {
            const string STRLOG_MethodName = "Initialise";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            if (DriverMachine.initialised == false)
            {
                try
                {
                    //
                    // Initialise the PLC
                    //
                    DriverMachine.statusMessage = STRLOG_PLC + STRLOG_Initialising + STRLOG_DCDrive + STRLOG_NotInitialised;

                    if (this.plc.OpenConnection() == false ||
                        this.plc.Initialise() == false)
                    {
                        throw new Exception(this.plc.LastError);
                    }

                    //
                    // Initialise the DC drive
                    //
                    DriverMachine.statusMessage = STRLOG_PLC + STRLOG_Initialised + STRLOG_DCDrive + STRLOG_Initialising;

                    if (this.dcDrive.OpenConnection() == false ||
                        this.dcDrive.Initialise() == false)
                    {
                        throw new Exception(this.dcDrive.LastError);
                    }

                    //
                    // Initialisation is complete
                    //
                    DriverMachine.initialised = true;
                    DriverMachine.online = true;
                    DriverMachine.statusMessage = StatusCodes.Ready.ToString();
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);
                    this.lastError = ex.Message;
                    DriverMachine.statusMessage = STRERR_FailedToInitialise + ex.Message;
                }
                finally
                {
                    this.plc.CloseConnection();
                    this.dcDrive.CloseConnection();
                }
            }

            string logMessage = STRLOG_Online + DriverMachine.online.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return DriverMachine.online;
        }

    }
}
