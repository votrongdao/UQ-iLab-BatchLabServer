using System;
using System.Web;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Drivers;

namespace Library.LabEquipment
{
    public class EquipmentEngine : LabEquipmentEngine
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "EquipmentEngine";

        //
        // Constants
        //

        //
        // String constants
        //

        //
        // String constants for logfile messages
        //
        private const string STRLOG_MeasurementDelay = " MeasurementDelay: ";
        private const string STRLOG_RedLionPresentInitialised = " RedLion is present and initialised";
        private const string STRLOG_ResetACDriveTime = " ResetACDriveTime: ";
        private const string STRLOG_ACDriveMode = " ACDriveMode: ";
        private const string STRLOG_StartACDriveTime = " StartACDriveTime: ";
        private const string STRLOG_StopACDriveTime = " StopACDriveTime: ";
        private const string STRLOG_TakeMeasurementTime = " TakeMeasurementTime: ";

        //
        // String constants for error messages
        //
        private const string STRERR_NumberIsNegative = "Number is negative!";
        private const string STRERR_NumberIsInvalid = "Number is invalid!";
        private const string STRERR_FailedToInitialiseRedLion = "Failed to initialise RedLion! ";

        //
        // Local variables
        //
        private RedLion redLion;

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public EquipmentEngine(string rootFilePath)
            : base(rootFilePath)
        {
            const string STRLOG_MethodName = "EquipmentEngine";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            try
            {
                //
                // Create an instance of the RedLion class
                //
                this.redLion = new RedLion(this.xmlNodeEquipmentConfig);

                //
                // Check the minimum initialise delay and update if necessary
                //
                //int initialiseDelay = this.redLion.InitialiseDelay;
                //if (initialiseDelay > this.InitialiseDelay)
                //{
                //    this.InitialiseDelay = initialiseDelay;
                //    Logfile.Write(STRLOG_InitialiseDelay + initialiseDelay.ToString() + STRLOG_Seconds);
                //}

                //
                // Get the delay in seconds to wait before taking a measurement
                //
                int measurementDelay = XmlUtilities.GetIntValue(this.xmlNodeEquipmentConfig, Consts.STRXML_measurementDelay);
                if (measurementDelay < 1)
                {
                    throw new Exception(STRERR_NumberIsInvalid);
                }
                Logfile.Write(STRLOG_MeasurementDelay + measurementDelay.ToString());

                this.redLion.MeasurementDelay = measurementDelay;
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

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Enable power to the external equipment.
        /// </summary>
        /// <returns>True if successful.</returns>
        public override bool PowerupEquipment()
        {
            const string STRLOG_MethodName = "PowerupEquipment";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            //
            // Nothing to do here
            //

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Initialise the equipment after it has been powered up.
        /// </summary>
        /// <returns>True if successful.</returns>
        public override bool InitialiseEquipment()
        {
            const string STRLOG_MethodName = "InitialiseEquipment";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            try
            {
                //
                // Initialise the RedLion class
                //
                if (this.redLion.Initialise() == false)
                {
                    string lastError = this.redLion.GetLastError();
                    throw new Exception(STRERR_FailedToInitialiseRedLion + lastError);
                }
                Logfile.Write(STRLOG_RedLionPresentInitialised);

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

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Disable power to the external equipment.
        /// </summary>
        /// <returns>True if successful.</returns>
        public override bool PowerdownEquipment()
        {
            const string STRLOG_MethodName = "PowerdownEquipment";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            //
            // Nothing to do here
            //

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public override LabStatus GetLabEquipmentStatus()
        {
            const string STRLOG_MethodName = "GetLabEquipmentStatus";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Get the status of the RedLion controller
            //
            LabStatus labStatus = new LabStatus(this.redLion.Online, this.redLion.StatusMessage);

            string logMessage = STRLOG_Online + labStatus.online.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_StatusMessage + Logfile.STRLOG_Quote + labStatus.labStatusMessage + Logfile.STRLOG_Quote;

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return labStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        public override ExecuteCommandInfo ProcessCommand(ExecuteCommandInfo executeCommandInfo)
        {
            const string STRLOG_MethodName = "ProcessCommand";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            CommandInfo commandInfo = (CommandInfo)executeCommandInfo;

            bool success = true;
            string errorMessage = null;

            try
            {
                //
                // Process the execute command
                //
                ExecuteCommands executeCommand = (ExecuteCommands)commandInfo.command;

                switch (executeCommand)
                {
                    case ExecuteCommands.CreateConnection:

                        //
                        // Create a connection to the RedLion controller
                        //
                        if ((success = this.redLion.CreateConnection()) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.CloseConnection:

                        //
                        // Close the connection to the RedLion controller
                        //
                        if ((success = this.redLion.CloseConnection()) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.ResetACDrive:

                        //
                        // Reset the AC drive controller
                        //
                        if ((success = this.redLion.ResetACDrive()) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.ConfigureACDrive:

                        //
                        // Get AC drive configuration from parameters
                        //
                        RedLion.ACDriveConfigs acDriveConfig = (RedLion.ACDriveConfigs)commandInfo.parameters[0];

                        //
                        // Configure the AC drive
                        //
                        if ((success = this.redLion.ConfigureACDrive(acDriveConfig)) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.StartACDrive:

                        //
                        // Get AC drive mode from parameters
                        //
                        RedLion.ACDriveModes acDriveMode = (RedLion.ACDriveModes)commandInfo.parameters[0];

                        //
                        // Start the AC drive
                        //
                        if ((success = this.redLion.StartACDrive(acDriveMode)) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.StopACDrive:

                        //
                        // Get AC drive mode from parameters
                        //
                        acDriveMode = (RedLion.ACDriveModes)commandInfo.parameters[0];

                        //
                        // Stop the AC drive
                        //
                        if ((success = this.redLion.StopACDrive(acDriveMode)) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.TakeMeasurement:

                        //
                        // Take a measurement
                        //
                        RedLion.Measurements measurement = new RedLion.Measurements();
                        if ((success = this.redLion.TakeMeasurement(ref measurement)) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        else
                        {
                            //
                            // Add the measurement values to results - ensure the order of the measurement values is the
                            // same as in EquipmentManager.cs
                            //
                            commandInfo.results = new object[] {
                                measurement.voltageMut,
                                measurement.currentMut,
                                measurement.powerFactorMut,
                                measurement.voltageVsd,
                                measurement.currentVsd,
                                measurement.powerFactorVsd,
                                measurement.speed,
                                measurement.torque
                            };
                        }
                        break;

                    default:

                        //
                        // Unknown command
                        //
                        errorMessage = STRERR_UnknownCommand + executeCommand.ToString();
                        success = false;
                        break;
                }
            }
            catch (Exception ex)
            {
                success = false;
                errorMessage = ex.Message;
            }

            //
            // Update success of command execution
            //
            executeCommandInfo.success = success;

            string logMessage = STRLOG_Success + success.ToString();
            if (success == false)
            {
                executeCommandInfo.errorMessage = errorMessage;
                logMessage += Logfile.STRLOG_Spacer + STRLOG_ErrorMessage + errorMessage;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return executeCommandInfo;
        }

        //-------------------------------------------------------------------------------------------------//

        protected override void Dispose(bool disposing)
        {
            if (disposing == true)
            {
                //
                // Dispose managed resources here. Anything that has a Dispose() method.
                //
                //
                // YOUR CODE HERE
                //
            }

            //
            // Release unmanaged resources here. Set large fields to null.
            //

            // Call Dispose on your base class.
            base.Dispose(disposing);
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetResetACDriveTime()
        {
            return this.redLion.GetResetACDriveTime();
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetConfigureACDriveTime()
        {
            return this.redLion.GetConfigureACDriveTime();
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetStartACDriveTime(RedLion.ACDriveModes acDriveMode)
        {
            return this.redLion.GetStartACDriveTime(acDriveMode);
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetStopACDriveTime(RedLion.ACDriveModes acDriveMode)
        {
            return this.redLion.GetStopACDriveTime(acDriveMode);
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetTakeMeasurementTime()
        {
            return this.redLion.GetTakeMeasurementTime();
        }

        //-------------------------------------------------------------------------------------------------//

    }
}
