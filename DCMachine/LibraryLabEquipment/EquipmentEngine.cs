using System;
using System.Net;
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
        private const string STRLOG_InitialiseEquipment = " InitialiseEquipment: ";
        private const string STRLOG_MeasurementDelay = " MeasurementDelay: ";
        private const string STRLOG_RedLionPresentInitialised = " RedLion is present and initialised";

        //
        // String constants for error messages
        //
        private const string STRERR_NumberIsNegative = "Number is negative!";
        private const string STRERR_NumberIsInvalid = "Number is invalid!";
        private const string STRERR_FailedToInitialiseRedLion = "Failed to initialise RedLion! ";

        //
        // Local variables
        //
        private Logfile.LoggingLevels logLevel;
        private RedLion redLion;

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public EquipmentEngine(string rootFilePath)
            : base(rootFilePath)
        {
            const string STRLOG_MethodName = "EquipmentEngine";

            Logfile.WriteCalled(null, STRLOG_MethodName);

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
                // Get the intialise equipment flag
                //
                bool initialiseEquipment = XmlUtilities.GetBoolValue(this.xmlNodeEquipmentConfig, Consts.STRXML_initialiseEquipment, false);
                Logfile.Write(STRLOG_InitialiseEquipment + initialiseEquipment.ToString());

                this.redLion.InitialiseEquipment = initialiseEquipment;

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

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            //
            // Nothing to do here
            //

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

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

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

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
                Logfile.Write(this.logLevel, STRLOG_RedLionPresentInitialised);

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

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

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            bool success = true;

            //
            // Nothing to do here
            //

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public override LabStatus GetLabEquipmentStatus()
        {
            const string STRLOG_MethodName = "GetLabEquipmentStatus";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            //
            // Get the status of the RedLion controller
            //
            LabStatus labStatus = new LabStatus(this.redLion.Online, this.redLion.StatusMessage);

            string logMessage = STRLOG_Online + labStatus.online.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_StatusMessage + Logfile.STRLOG_Quote + labStatus.labStatusMessage + Logfile.STRLOG_Quote;

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return labStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        public override ExecuteCommandInfo ProcessCommand(ExecuteCommandInfo executeCommandInfo)
        {
            const string STRLOG_MethodName = "ProcessCommand";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

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
                        // Reset the AC drive
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
                        RedLion.ACDriveConfigs acCDriveConfig = (RedLion.ACDriveConfigs)commandInfo.parameters[0];

                        //
                        // Configure the AC drive with the specified configuration
                        //
                        if ((success = this.redLion.ConfigureACDrive(acCDriveConfig)) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.StartACDrive:

                        //
                        // Start the AC drive
                        //
                        if ((success = this.redLion.StartACDrive()) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.StopACDrive:

                        //
                        // Stop the AC drive
                        //
                        if ((success = this.redLion.StopACDrive()) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.ResetDCDriveMut:

                        //
                        // Reset the DC drive
                        //
                        if ((success = this.redLion.ResetDCDriveMut()) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.ConfigureDCDriveMut:

                        //
                        // Get DC drive configuration from parameters
                        //
                        RedLion.DCDriveMutConfigs dcDriveMutConfig = (RedLion.DCDriveMutConfigs)commandInfo.parameters[0];

                        //
                        // Configure the AC drive with the specified configuration
                        //
                        if ((success = this.redLion.ConfigureDCDriveMut(dcDriveMutConfig)) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.StartDCDriveMut:

                        //
                        // Get DC drive mode from parameters
                        //
                        RedLion.DCDriveMutModes dcDriveMutMode = (RedLion.DCDriveMutModes)commandInfo.parameters[0];

                        //
                        // Start the DC drive
                        //
                        if ((success = this.redLion.StartDCDriveMut(dcDriveMutMode)) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.StopDCDriveMut:

                        //
                        // Stop the DC drive
                        //
                        if ((success = this.redLion.StopDCDriveMut()) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.SetSpeedACDrive:

                        //
                        // Get speed from parameters
                        //
                        int speedACDrive = (int)commandInfo.parameters[0];

                        //
                        // Set the speed of the AC drive
                        //
                        if ((success = this.redLion.SetSpeedACDrive(speedACDrive)) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.SetSpeedDCDriveMut:

                        //
                        // Get speed from parameters
                        //
                        int speedDCDriveMut = (int)commandInfo.parameters[0];

                        //
                        // Set the speed of the DC drive
                        //
                        if ((success = this.redLion.SetSpeedDCDriveMut(speedDCDriveMut)) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.SetTorqueDCDriveMut:

                        //
                        // Get torque from parameters
                        //
                        int torqueDCDriveMut = (int)commandInfo.parameters[0];

                        //
                        // Set the torque of the DC drive
                        //
                        if ((success = this.redLion.SetTorqueDCDriveMut(torqueDCDriveMut)) == false)
                        {
                            errorMessage = this.redLion.GetLastError();
                        }
                        break;

                    case ExecuteCommands.SetFieldDCDriveMut:

                        //
                        // Get torque from parameters
                        //
                        int fieldDCDriveMut = (int)commandInfo.parameters[0];

                        //
                        // Set the field of the DC drive
                        //
                        if ((success = this.redLion.SetFieldDCDriveMut(fieldDCDriveMut)) == false)
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
                            // Add the measurement values to results
                            //
                            commandInfo.results = new object[] {
                                measurement.speed,
                                measurement.voltage,
                                measurement.fieldCurrent,
                                measurement.load
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

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

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

        public int GetStartACDriveTime()
        {
            return this.redLion.GetStartACDriveTime();
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetStopACDriveTime()
        {
            return this.redLion.GetStopACDriveTime();
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetResetDCDriveMutTime()
        {
            return this.redLion.GetResetDCDriveMutTime();
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetConfigureDCDriveMutTime()
        {
            return this.redLion.GetConfigureDCDriveMutTime();
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetStartDCDriveMutTime(RedLion.DCDriveMutModes dcDriveMutMode)
        {
            return this.redLion.GetStartDCDriveMutTime(dcDriveMutMode);
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetStopDCDriveMutTime()
        {
            return this.redLion.GetStopDCDriveMutTime();
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetSetSpeedACDriveTime()
        {
            return this.redLion.GetSetSpeedACDriveTime();
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetSetSpeedDCDriveMutTime()
        {
            return this.redLion.GetSetSpeedDCDriveMutTime();
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetSetTorqueDCDriveMutTime()
        {
            return this.redLion.GetSetTorqueDCDriveMutTime();
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetSetFieldDCDriveMutTime()
        {
            return this.redLion.GetSetFieldDCDriveMutTime();
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetTakeMeasurementTime()
        {
            return this.redLion.GetTakeMeasurementTime();
        }

        //-------------------------------------------------------------------------------------------------//

        public RedLion.ACDriveInfo GetACDriveInfo()
        {
            return this.redLion.GetACDriveInfo();
        }

        //-------------------------------------------------------------------------------------------------//

        public RedLion.DCDriveMutInfo GetDCDriveMutInfo()
        {
            return this.redLion.GetDCDriveMutInfo();
        }

    }
}
