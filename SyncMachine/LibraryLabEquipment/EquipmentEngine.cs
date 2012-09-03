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

        //
        // String constants for error messages
        //

        //
        // Local variables
        //
        private DriverMachine driverMachine;

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
                // Create an instance of the Machine class
                //
                this.driverMachine = new DriverMachine(this.xmlNodeEquipmentConfig, null);

                //
                // Update the initialisation delay
                //
                this.powerupInitialiseDelay = this.driverMachine.InitialiseDelay;
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
                success = this.driverMachine.Initialise();
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
            // Get the machine status
            //
            LabStatus labStatus = new LabStatus(this.driverMachine.Online, this.driverMachine.StatusMessage);

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

            bool success = false;
            string errorMessage = null;

            try
            {
                //
                // Process the execute command
                //
                ExecuteCommands executeCommand = (ExecuteCommands)commandInfo.command;

                switch (executeCommand)
                {
                    case ExecuteCommands.StartExecution:
                        //
                        // Get the specification in XML format from the parameters and parse
                        //
                        string xmlSpecification = (string)commandInfo.parameters[0];
                        Specification specification = new Specification(this.xmlNodeEquipmentConfig);
                        ValidationReport validationReport = specification.Parse(xmlSpecification);
                        if (validationReport.accepted == false)
                        {
                            errorMessage = validationReport.errorMessage;
                            break;
                        }

                        //
                        // Create an instance of the driver for the specified setup
                        // and then start the driver with the specification
                        //
                        if (specification.SetupId.Equals(Consts.STRXML_SetupId_OpenCircuitVaryField) == true)
                        {
                            this.driverMachine = new DriverMachine_OCVF(this.xmlNodeEquipmentConfig, specification);
                        }
                        else if (specification.SetupId.Equals(Consts.STRXML_SetupId_OpenCircuitVarySpeed) == true)
                        {
                            this.driverMachine = new DriverMachine_OCVS(this.xmlNodeEquipmentConfig, specification);
                        }
                        else if (specification.SetupId.Equals(Consts.STRXML_SetupId_ShortCircuitVaryField) == true)
                        {
                            this.driverMachine = new DriverMachine_SCVF(this.xmlNodeEquipmentConfig, specification);
                        }
                        else if (specification.SetupId.Equals(Consts.STRXML_SetupId_PreSynchronisation) == true)
                        {
                            this.driverMachine = new DriverMachine_PreSync(this.xmlNodeEquipmentConfig, specification);
                        }
                        else if (specification.SetupId.Equals(Consts.STRXML_SetupId_Synchronisation) == true)
                        {
                            this.driverMachine = new DriverMachine_Sync(this.xmlNodeEquipmentConfig, specification);
                        }
                        else
                        {
                            //
                            // Unknown SetupId
                            //
                            throw new Exception(STRERR_UnknownSetupId + specification.SetupId);
                        }

                        //
                        // Start execution of the specified setup
                        //
                        if ((success = this.driverMachine.Start()) == false)
                        {
                            errorMessage = this.driverMachine.LastError;
                        }
                        break;

                    default:
                        //
                        // Unknown command
                        //
                        throw new Exception(STRERR_UnknownCommand + executeCommand.ToString());
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

        public int GetExecutionTime(string xmlSpecification)
        {
            int executionTime = -1;

            try
            {
                //
                // Parse the XML specification string
                //
                Specification specification = new Specification(this.xmlNodeEquipmentConfig);
                ValidationReport validationReport = specification.Parse(xmlSpecification);
                if (validationReport.accepted == true)
                {
                    executionTime = (int)validationReport.estRuntime;
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetExecutionTimeRemaining()
        {
            return this.driverMachine.GetExecutionTimeRemaining();
        }

        //-------------------------------------------------------------------------------------------------//

        public DriverMachine.ExecutionStatus GetExecutionStatus()
        {
            return this.driverMachine.GetExecutionStatus();
        }

        //-------------------------------------------------------------------------------------------------//

        public DriverMachine.ExecutionStatus GetExecutionResultStatus()
        {
            return this.driverMachine.GetExecutionResultStatus();
        }

        //-------------------------------------------------------------------------------------------------//

        public string GetExecutionResults()
        {
            return this.driverMachine.GetExecutionResults();
        }

        //-------------------------------------------------------------------------------------------------//

        public string GetExecutionErrorMessage()
        {
            return this.driverMachine.LastError;
        }

    }
}
