using System;
using System.Diagnostics;
using System.Xml;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServerEngine.Drivers.Equipment;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverSpeedVsVoltage : DriverEquipmentGeneric
    {
        #region Class Constants and Variables

        //
        // Data driven state machine for Execute()
        //
        private enum States_Execute
        {
            sSuspendPowerdown,
            sCreateConnection,
            sResetACDrive,
            sResetDCDriveMut,
            sConfigureACDrive,
            sConfigureDCDriveMut,
            sStartACDrive,
            sStartDCDriveMut,
            sSetSpeedDCDriveMut,
            sTakeMeasurement,
            sResetSpeedDCDriveMut,
            sStopDCDriveMut,
            sStopACDrive,
            sReconfigureDCDriveMut,
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
            new SMTableEntry_Execute(States_Execute.sResetACDrive, States_Execute.sResetDCDriveMut, States_Execute.sCloseConnection,
                Consts.STRXML_CmdResetACDrive, null),
            //
            // ResetDCDriveMut
            //
            new SMTableEntry_Execute(States_Execute.sResetDCDriveMut, States_Execute.sConfigureACDrive, States_Execute.sCloseConnection,
                Consts.STRXML_CmdResetDCDriveMut, null),
            //
            // ConfigureACDrive
            //
            new SMTableEntry_Execute(States_Execute.sConfigureACDrive, States_Execute.sConfigureDCDriveMut, States_Execute.sReconfigureACDrive,
                Consts.STRXML_CmdConfigureACDrive, new string[,] {
                    { Consts.STRXML_ReqACDriveConfig, Consts.STR_ACDriveConfig_LowerCurrent }
                } ),
            //
            // ConfigureDCDriveMut
            //
            new SMTableEntry_Execute(States_Execute.sConfigureDCDriveMut, States_Execute.sStartACDrive, States_Execute.sReconfigureDCDriveMut,
                Consts.STRXML_CmdConfigureDCDriveMut, new string[,] {
                    { Consts.STRXML_ReqDCDriveMutConfig, Consts.STR_DCDriveMutConfig_Default }
                } ),
            //
            // StartACDrive
            //
            new SMTableEntry_Execute(States_Execute.sStartACDrive, States_Execute.sStartDCDriveMut, States_Execute.sStopACDrive,
                Consts.STRXML_CmdStartACDrive, null),
            //
            // StartDCDriveMut
            //
            new SMTableEntry_Execute(States_Execute.sStartDCDriveMut, States_Execute.sSetSpeedDCDriveMut, States_Execute.sStopDCDriveMut,
                Consts.STRXML_CmdStartDCDriveMut, new string[,] {
                    { Consts.STRXML_ReqDCDriveMutMode, Consts.STR_DCDriveMutMode_Speed }
                } ),
            //
            // SetSpeedDCDriveMut
            //
            new SMTableEntry_Execute(States_Execute.sSetSpeedDCDriveMut, States_Execute.sTakeMeasurement, States_Execute.sStopDCDriveMut,
                Consts.STRXML_CmdSetSpeedDCDriveMut, new string[,] {
                    { Consts.STRXML_ReqSpeedDCDriveMut, string.Empty }
                } ),
            //
            // TakeMeasurement
            //
            new SMTableEntry_Execute(States_Execute.sTakeMeasurement, States_Execute.sResetSpeedDCDriveMut, States_Execute.sStopDCDriveMut,
                Consts.STRXML_CmdTakeMeasurement, null),
            //
            // ResetSpeedDCDriveMut
            //
            new SMTableEntry_Execute(States_Execute.sResetSpeedDCDriveMut, States_Execute.sStopDCDriveMut, States_Execute.sStopDCDriveMut,
                Consts.STRXML_CmdSetSpeedDCDriveMut, new string[,] {
                    { Consts.STRXML_ReqSpeedDCDriveMut, string.Empty }
                } ),
            //
            // StopDCDriveMut
            //
            new SMTableEntry_Execute(States_Execute.sStopDCDriveMut, States_Execute.sStopACDrive, States_Execute.sStopACDrive,
                Consts.STRXML_CmdStopDCDriveMut, null),
            //
            // StopACDrive
            //
            new SMTableEntry_Execute(States_Execute.sStopACDrive, States_Execute.sReconfigureDCDriveMut, States_Execute.sReconfigureDCDriveMut,
                Consts.STRXML_CmdStopACDrive, null),
            //
            // ReconfigureDCDriveMut
            //
            new SMTableEntry_Execute(States_Execute.sReconfigureDCDriveMut, States_Execute.sReconfigureACDrive, States_Execute.sReconfigureACDrive,
                Consts.STRXML_CmdConfigureDCDriveMut, new string[,] {
                    { Consts.STRXML_ReqDCDriveMutConfig, Consts.STR_DCDriveMutConfig_Default }
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
            string logMessage = STRLOG_SpeedMin + specification.Speed.min.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_SpeedMax + specification.Speed.max.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_SpeedStep + specification.Speed.step.ToString();
            Logfile.Write(logMessage);

            //
            // Create an instance of the result info ready to fill in
            //
            ResultInfo resultInfo = new ResultInfo();
            resultInfo.statusCode = StatusCodes.Running;

            //
            // Create data structures to hold the results
            //
            int vectorLength = ((specification.Speed.max - specification.Speed.min) / specification.Speed.step) + 1;
            resultInfo.speedVector = new int[vectorLength];
            resultInfo.voltageVector = new int[vectorLength];
            resultInfo.loadVector = new int[vectorLength];
            resultInfo.fieldVector = new float[vectorLength];

            //
            // Initialise variables used in the state machine
            //
            int vectorIndex = 0;
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
                        case States_Execute.sSetSpeedDCDriveMut:
                            int speedDCDriveMut = specification.Speed.min + (vectorIndex * specification.Speed.step);
                            entry.commandArguments[0, 1] = speedDCDriveMut.ToString();
                            break;

                        case States_Execute.sResetSpeedDCDriveMut:
                            speedDCDriveMut = 0;
                            entry.commandArguments[0, 1] = speedDCDriveMut.ToString();
                            break;

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
                            resultInfo.speedVector[vectorIndex] += XmlUtilities.GetIntValue(xmlResponseNode, Consts.STRXML_RspSpeed, 0);
                            resultInfo.voltageVector[vectorIndex] += XmlUtilities.GetIntValue(xmlResponseNode, Consts.STRXML_RspVoltage, 0);
                            resultInfo.loadVector[vectorIndex] += XmlUtilities.GetIntValue(xmlResponseNode, Consts.STRXML_RspLoad, 0);
                            resultInfo.fieldVector[vectorIndex] += (float)XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspFieldCurrent, 0.0);

                            //
                            // Check if all measurements have been taken for this field value
                            //
                            if (++repeatCount == this.measurementCount)
                            {
                                //
                                // All measurements have been taken for this field value, average the values
                                //
                                resultInfo.speedVector[vectorIndex] /= measurementCount;
                                resultInfo.voltageVector[vectorIndex] /= measurementCount;
                                resultInfo.fieldVector[vectorIndex] /= measurementCount;
                                resultInfo.loadVector[vectorIndex] /= measurementCount;

                                //
                                // Check if field values have been completed
                                //
                                if (++vectorIndex == vectorLength)
                                {
                                    //
                                    // All measurements have been taken
                                    //
                                    break;
                                }

                                //
                                // Next field value
                                //
                                repeatCount = 0;
                                nextState = States_Execute.sSetSpeedDCDriveMut;
                                break;
                            }

                            // Next measurement at the same field value
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
