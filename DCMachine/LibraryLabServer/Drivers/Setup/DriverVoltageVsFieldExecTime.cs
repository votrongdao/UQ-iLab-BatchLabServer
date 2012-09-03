using System;
using System.Diagnostics;
using System.Xml;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServerEngine.Drivers.Equipment;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverVoltageVsField : DriverEquipmentGeneric
    {
        #region Class Constants and Variables

        //
        // State machine states
        //
        private enum States_GetExecutionTime
        {
            sGetResetACDriveTime,
            sGetResetDCDriveMutTime,
            sGetConfigureACDriveTime,
            sGetSetSpeedACDriveTime,
            sGetConfigureDCDriveMutTime,
            sGetStartACDriveTime,
            sGetStartDCDriveMutTime,
            sGetSetFieldDCDriveMutTime,
            sGetTakeMeasurementTime,
            sGetResetFieldDCDriveMutTime,
            sGetStopDCDriveMutTime,
            sGetStopACDriveTime,
            sGetReconfigureDCDriveMutTime,
            sGetReconfigureACDriveTime,
            sCompleted
        }

        //
        // State machine table entry
        //
        private struct SMTableEntry_GetExecutionTime
        {
            public States_GetExecutionTime currentState;
            public States_GetExecutionTime nextState;
            public string equipmentCommand;
            public string[,] commandArguments;

            public SMTableEntry_GetExecutionTime(States_GetExecutionTime currentState, States_GetExecutionTime nextState,
                string equipmentCommand, string[,] commandArguments)
            {
                this.currentState = currentState;
                this.nextState = nextState;
                this.equipmentCommand = equipmentCommand;
                this.commandArguments = commandArguments;
            }
        }

        //
        // State machine table
        //
        private SMTableEntry_GetExecutionTime[] smTable_GetExecutionTime = new SMTableEntry_GetExecutionTime[] {
            //
            // GetResetACDriveTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetResetACDriveTime, States_GetExecutionTime.sGetResetDCDriveMutTime,
                Consts.STRXML_CmdGetResetACDriveTime, null
                ),
            //
            // GetResetDCDriveMutTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetResetDCDriveMutTime, States_GetExecutionTime.sGetConfigureACDriveTime,
                Consts.STRXML_CmdGetResetDCDriveMutTime, null
                ),
            //
            // GetConfigureACDriveTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetConfigureACDriveTime, States_GetExecutionTime.sGetSetSpeedACDriveTime,
                Consts.STRXML_CmdGetConfigureACDriveTime, null
                ),
            //
            // GetSetSpeedACDriveTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetSetSpeedACDriveTime, States_GetExecutionTime.sGetConfigureDCDriveMutTime,
                Consts.STRXML_CmdGetSetSpeedACDriveTime, null
                ),
            //
            // GetConfigureDCDriveMutTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetConfigureDCDriveMutTime, States_GetExecutionTime.sGetStartACDriveTime,
                Consts.STRXML_CmdGetConfigureDCDriveMutTime, null
                ),
            //
            // GetStartACDriveTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetStartACDriveTime, States_GetExecutionTime.sGetStartDCDriveMutTime,
                Consts.STRXML_CmdGetStartACDriveTime, null
                ),
            //
            // GetStartDCDriveMutTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetStartDCDriveMutTime, States_GetExecutionTime.sGetSetFieldDCDriveMutTime,
                Consts.STRXML_CmdGetStartDCDriveMutTime, new string[,] {
                    { Consts.STRXML_ReqDCDriveMutMode, Consts.STR_DCDriveMutMode_EnableOnly },
                }
                ),
            //
            // GetSetFieldDCDriveMutTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetSetFieldDCDriveMutTime, States_GetExecutionTime.sGetTakeMeasurementTime,
                Consts.STRXML_CmdGetSetFieldDCDriveMutTime, null
                ),
            //
            // GetTakeMeasurementTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetTakeMeasurementTime, States_GetExecutionTime.sGetResetFieldDCDriveMutTime,
                Consts.STRXML_CmdGetTakeMeasurementTime, null
                ),
            //
            // GetResetFieldDCDriveMutTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetResetFieldDCDriveMutTime, States_GetExecutionTime.sGetStopDCDriveMutTime,
                Consts.STRXML_CmdGetSetFieldDCDriveMutTime, null
                ),
            //
            // GetStopDCDriveMutTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetStopDCDriveMutTime, States_GetExecutionTime.sGetStopACDriveTime,
                Consts.STRXML_CmdGetStopDCDriveMutTime, null
                ),
            //
            // GetStopACDriveTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetStopACDriveTime, States_GetExecutionTime.sGetReconfigureDCDriveMutTime,
                Consts.STRXML_CmdGetStopACDriveTime, null
                ),
            //
            // GetReconfigureDCDriveMutTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetReconfigureDCDriveMutTime, States_GetExecutionTime.sGetReconfigureACDriveTime,
                Consts.STRXML_CmdGetConfigureDCDriveMutTime, null
                ),
            //
            // GetReconfigureACDriveTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetReconfigureACDriveTime, States_GetExecutionTime.sCompleted,
                Consts.STRXML_CmdGetConfigureACDriveTime, null
                ),
        };

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public override int GetExecutionTime(ExperimentSpecification experimentSpecification)
        {
            const string STRLOG_MethodName = "GetExecutionTime";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            // Typecast the specification so that it can be used here
            Specification specification = (Specification)experimentSpecification;

            //
            // Log the specification
            //
            string logMessage = STRLOG_FieldMin + specification.Field.min.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_FieldMax + specification.Field.max.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_FieldStep + specification.Field.step.ToString();
            Logfile.Write(logMessage);

            //
            // Initialise variables used in the state machine
            //
            double executionTime = 0.0;
            int vectorLength = ((specification.Field.max - specification.Field.min) / specification.Field.step) + 1;

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
                // Get the time until the LabEquipment is ready to use
                //
                executionTime = this.equipmentServiceProxy.GetTimeUntilReady();

                //
                // Run the state machine to determine the execution time for the experiment specification
                //
                States_GetExecutionTime state = States_GetExecutionTime.sCompleted;
                if (smTable_GetExecutionTime.Length > 0)
                {
                    state = smTable_GetExecutionTime[0].currentState;
                }
                while (state != States_GetExecutionTime.sCompleted)
                {
                    //
                    // Find table entry
                    //
                    int index = -1;
                    for (int i = 0; i < smTable_GetExecutionTime.Length; i++)
                    {
                        if (smTable_GetExecutionTime[i].currentState == state)
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
                    SMTableEntry_GetExecutionTime entry = smTable_GetExecutionTime[index];
                    States_GetExecutionTime nextState = entry.nextState;

                    logMessage = " [ " + STRLOG_MethodName + ": " + entry.currentState.ToString() + " ]";
                    Logfile.Write(logMessage);
                    Trace.WriteLine(logMessage);

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
                        string errorMessage = XmlUtilities.GetXmlValue(xmlResponseNode, LabServerEngine.Consts.STRXML_RspErrorMessage, true);
                        throw new ArgumentException(errorMessage);
                    }

                    //
                    // Extract response values where required
                    //
                    double stateExecutionTime = 0.0;
                    switch (entry.currentState)
                    {
                        case States_GetExecutionTime.sGetResetACDriveTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspResetACDriveTime, 0);
                            break;

                        case States_GetExecutionTime.sGetResetDCDriveMutTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspResetDCDriveMutTime, 0);
                            break;

                        case States_GetExecutionTime.sGetConfigureACDriveTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspConfigureACDriveTime, 0);
                            break;

                        case States_GetExecutionTime.sGetSetSpeedACDriveTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspSetSpeedACDriveTime, 0);
                            break;

                        case States_GetExecutionTime.sGetConfigureDCDriveMutTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspConfigureDCDriveMutTime, 0);
                            break;

                        case States_GetExecutionTime.sGetStartACDriveTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspStartACDriveTime, 0);
                            break;

                        case States_GetExecutionTime.sGetStartDCDriveMutTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspStartDCDriveMutTime, 0);
                            break;

                        case States_GetExecutionTime.sGetSetFieldDCDriveMutTime:
                            double setFieldDCDriveMutTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspSetFieldDCDriveMutTime, 0);
                            stateExecutionTime = vectorLength * setFieldDCDriveMutTime;
                            break;

                        case States_GetExecutionTime.sGetTakeMeasurementTime:
                            double takeMeasurementTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspTakeMeasurementTime, 0);
                            stateExecutionTime = vectorLength * this.measurementCount * takeMeasurementTime;
                            break;

                        case States_GetExecutionTime.sGetResetFieldDCDriveMutTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspSetFieldDCDriveMutTime, 0);
                            break;

                        case States_GetExecutionTime.sGetStopDCDriveMutTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspStopDCDriveMutTime, 0);
                            break;

                        case States_GetExecutionTime.sGetStopACDriveTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspStopACDriveTime, 0);
                            break;

                        case States_GetExecutionTime.sGetReconfigureDCDriveMutTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspConfigureDCDriveMutTime, 0);
                            break;

                        case States_GetExecutionTime.sGetReconfigureACDriveTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspConfigureACDriveTime, 0);
                            break;

                        default:
                            break;
                    }

                    Trace.WriteLine("stateExecutionTime: " + stateExecutionTime.ToString());

                    //
                    // Update the execution time so far
                    //
                    executionTime += stateExecutionTime;

                    //
                    // Next state
                    //
                    state = nextState;
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            logMessage = STRLOG_ExecutionTime + executionTime.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return (int)executionTime;
        }

    }
}
