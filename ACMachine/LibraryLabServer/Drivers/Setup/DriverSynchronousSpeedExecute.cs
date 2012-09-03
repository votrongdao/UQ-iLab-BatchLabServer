using System;
using System.Diagnostics;
using System.Xml;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServerEngine.Drivers.Equipment;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverSynchronousSpeed : DriverEquipmentGeneric
    {
        #region Class Constants and Variables

        //
        // String constants for logfile messages
        //

        //
        // String constants for error messages
        //

        //
        // Data driven state machine for Execute()
        //
        private enum States_Execute
        {
            sSuspendPowerdown,
            sCreateConnection,
            sResetACDrive,
            sConfigureACDrive,
            sStartACDrive,
            sTakeMeasurement,
            sStopACDrive,
            sReconfigureACDrive,
            sCloseConnection,
            sResumePowerdown,
            sCompleted,
        }
        private struct SMTableEntry_Execute
        {
            public States_Execute currentState;
            public States_Execute nextState;
            public States_Execute exitState;
            public string equipmentCommand;
            public string[,] commandArguments;

            public SMTableEntry_Execute(States_Execute currentState, States_Execute nextState, States_Execute exitState,
                string equipmentCommand, string[,] commandArguments)
            {
                this.currentState = currentState;
                this.nextState = nextState;
                this.exitState = exitState;
                this.equipmentCommand = equipmentCommand;
                this.commandArguments = commandArguments;
            }
        }
        private SMTableEntry_Execute[] smTable_Execute = new SMTableEntry_Execute[] {
            //
            // Suspend powerdown
            //
            new SMTableEntry_Execute(States_Execute.sSuspendPowerdown, States_Execute.sCreateConnection, States_Execute.sResumePowerdown,
                null, null),

            //
            // CreateConnection
            //
            new SMTableEntry_Execute(States_Execute.sCreateConnection, States_Execute.sResetACDrive, States_Execute.sResumePowerdown,
                Consts.STRXML_CmdCreateConnection, null),

            //
            // ResetACDrive
            //
            new SMTableEntry_Execute(States_Execute.sResetACDrive, States_Execute.sConfigureACDrive, States_Execute.sCloseConnection,
                Consts.STRXML_CmdResetACDrive, null),

            //
            // ConfigureACDrive
            //
            new SMTableEntry_Execute(States_Execute.sConfigureACDrive, States_Execute.sStartACDrive, States_Execute.sCloseConnection,
                Consts.STRXML_CmdConfigureACDrive, new string[,] {
                    { Consts.STRXML_ReqACDriveConfig, Consts.STR_ACDriveConfig_MaximumCurrent }
                } ),

            //
            // StartACDrive
            //
            new SMTableEntry_Execute(States_Execute.sStartACDrive, States_Execute.sTakeMeasurement, States_Execute.sStopACDrive,
                Consts.STRXML_CmdStartACDrive, new string[,] {
                    { Consts.STRXML_ReqACDriveMode, Consts.STR_ACDriveMode_SynchronousSpeed }
                } ),

            //
            // TakeMeasurement
            //
            new SMTableEntry_Execute(States_Execute.sTakeMeasurement, States_Execute.sStopACDrive, States_Execute.sStopACDrive,
                Consts.STRXML_CmdTakeMeasurement, null),

            //
            // StopACDrive
            //
            new SMTableEntry_Execute(States_Execute.sStopACDrive, States_Execute.sReconfigureACDrive, States_Execute.sCloseConnection,
                Consts.STRXML_CmdStopACDrive, new string[,] {
                    { Consts.STRXML_ReqACDriveMode, Consts.STR_ACDriveMode_SynchronousSpeed }
                } ),

            //
            // ReconfigureACDrive
            //
            new SMTableEntry_Execute(States_Execute.sReconfigureACDrive, States_Execute.sCloseConnection, States_Execute.sCloseConnection,
                Consts.STRXML_CmdConfigureACDrive, new string[,] {
                    { Consts.STRXML_ReqACDriveConfig, Consts.STR_ACDriveConfig_Default }
                } ),

            //
            // CloseConnection
            //
            new SMTableEntry_Execute(States_Execute.sCloseConnection, States_Execute.sResumePowerdown, States_Execute.sResumePowerdown,
                Consts.STRXML_CmdCloseConnection, null),

            //
            // Resume powerdown
            //
            new SMTableEntry_Execute(States_Execute.sResumePowerdown, States_Execute.sCompleted, States_Execute.sCompleted,
                null, null),
        };

        #endregion

        //---------------------------------------------------------------------------------------//

        public override ExperimentResultInfo Execute(ExperimentSpecification experimentSpecification)
        {
            const string STRLOG_MethodName = "Execute";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Determine how long it actually take to execute
            //
            DateTime startDateTime = DateTime.Now;

            // Typecast the specification so that it can be used here
            Specification specification = (Specification)experimentSpecification;

            //
            // Log the specification
            //
            string logMessage = string.Empty;
            //Logfile.Write(logMessage);

            //
            // Create an instance of the result info ready to fill in
            //
            ResultInfo resultInfo = new ResultInfo();
            resultInfo.statusCode = StatusCodes.Running;

            //
            // Initialise variables used in the state machine
            //
            int repeatCount = 0;

            try
            {
                //
                // First, check to see if the LabEquipment is online
                //
                LabEquipmentStatus labEquipmentStatus = this.equipmentServiceProxy.GetLabEquipmentStatus();
                if (labEquipmentStatus.online == false)
                {
                    throw new Exception(labEquipmentStatus.statusMessage);
                }

                //
                // Run the state machine to execute the experiment specification
                //
                States_Execute state = States_Execute.sCompleted;
                if (smTable_Execute.Length > 0)
                {
                    state = smTable_Execute[0].currentState;
                }
                while (state != States_Execute.sCompleted)
                {
                    //
                    // Find table entry
                    //
                    int index = -1;
                    for (int i = 0; i < smTable_Execute.Length; i++)
                    {
                        if (smTable_Execute[i].currentState == state)
                        {
                            // Entry found
                            index = i;
                            break;
                        }
                    }
                    if (index == -1)
                    {
                        throw new ArgumentOutOfRangeException(state.ToString(), STRERR_StateNotFound);
                    }

                    //
                    // Get table entry and save next state
                    //
                    SMTableEntry_Execute entry = smTable_Execute[index];
                    States_Execute nextState = entry.nextState;

                    logMessage = " [ " + STRLOG_MethodName + ": " + entry.currentState.ToString() + " ]";
                    Logfile.Write(logMessage);

                    Trace.WriteLine(logMessage);

                    //
                    // Check if experiment was cancelled
                    //
                    if (this.cancelExperiment != null && this.cancelExperiment.IsCancelled == true &&
                        resultInfo.statusCode == StatusCodes.Running)
                    {
                        //
                        // Experiment was cancelled
                        //
                        resultInfo.statusCode = StatusCodes.Cancelled;
                        state = entry.exitState;
                        continue;
                    }

                    //
                    // Process non-XML commands
                    //
                    switch (entry.currentState)
                    {
                        case States_Execute.sSuspendPowerdown:
                            if (this.equipmentServiceProxy.SuspendPowerdown() == false)
                            {
                                //
                                // Command execution failed
                                //
                                resultInfo.statusCode = StatusCodes.Failed;
                                resultInfo.errorMessage = STRERR_SuspendPowerdown;
                                state = entry.exitState;
                            }
                            else
                            {
                                state = nextState;
                            }
                            continue;

                        case States_Execute.sResumePowerdown:
                            if (this.equipmentServiceProxy.ResumePowerdown() == false)
                            {
                                //
                                // Command execution failed
                                //
                                resultInfo.statusCode = StatusCodes.Failed;
                                resultInfo.errorMessage = STRERR_ResumePowerdown;
                                state = entry.exitState;
                            }
                            else
                            {
                                state = nextState;
                            }
                            continue;

                        default:
                            break;
                    }

                    //
                    // Add command arguments where required
                    //
                    switch (entry.currentState)
                    {
                        //
                        // Nothing to do here
                        //

                        default:
                            break;
                    }

                    //
                    // Execute command and check response success
                    //
                    XmlDocument xmlRequestDocument = CreateXmlRequestDocument(entry.equipmentCommand, entry.commandArguments);
                    string xmlResponse = this.equipmentServiceProxy.ExecuteRequest(xmlRequestDocument.InnerXml);
                    XmlNode xmlResponseNode = CreateXmlResponseNode(xmlResponse);
                    if (XmlUtilities.GetBoolValue(xmlResponseNode, LabServerEngine.Consts.STRXML_RspSuccess, false) == false)
                    {
                        //
                        // Command execution failed
                        //
                        resultInfo.statusCode = StatusCodes.Failed;
                        resultInfo.errorMessage = XmlUtilities.GetXmlValue(xmlResponseNode, LabServerEngine.Consts.STRXML_RspErrorMessage, true);
                        state = entry.exitState;
                        continue;
                    }

                    //
                    // Extract response values where required
                    //
                    switch (entry.currentState)
                    {
                        case States_Execute.sTakeMeasurement:

                            //
                            // Add in the values
                            //
                            resultInfo.voltage += (float)XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspVoltageVsd, 0.0);
                            resultInfo.current += (float)XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspCurrentVsd, 0.0);
                            resultInfo.powerFactor += (float)XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspPowerFactorVsd, 0.0);
                            resultInfo.speed += XmlUtilities.GetIntValue(xmlResponseNode, Consts.STRXML_RspSpeed, 0);
                            resultInfo.torque += XmlUtilities.GetIntValue(xmlResponseNode, Consts.STRXML_RspTorque, 0);

                            //
                            // Check if all measurements have been taken
                            //
                            if (++repeatCount == this.measurementCount)
                            {
                                //
                                // All measurements taken, average the values
                                //
                                resultInfo.voltage /= this.measurementCount;
                                resultInfo.current /= this.measurementCount;
                                resultInfo.powerFactor /= this.measurementCount;
                                resultInfo.speed /= this.measurementCount;
                                resultInfo.torque /= this.measurementCount;
                                break;
                            }

                            // Next measurement
                            nextState = States_Execute.sTakeMeasurement;
                            break;

                        default:
                            break;
                    }

                    //
                    // Next state
                    //
                    state = nextState;
                }

                //
                // Update status code
                //
                if (resultInfo.statusCode == StatusCodes.Running)
                {
                    resultInfo.statusCode = StatusCodes.Completed;
                }
            }
            catch (Exception ex)
            {
                resultInfo.statusCode = StatusCodes.Failed;
                resultInfo.errorMessage = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            //
            // Calculate actual execution time
            //
            TimeSpan timeSpan = DateTime.Now - startDateTime;

            logMessage = STRLOG_StatusCode + resultInfo.statusCode
                + Logfile.STRLOG_Spacer + STRLOG_ExecutionTime + timeSpan.TotalSeconds.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return resultInfo;
        }

    }
}
