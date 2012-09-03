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
        // State machine states
        //
        private enum States_GetExecutionTime
        {
            sGetResetACDriveTime,
            sGetConfigureACDriveTime,
            sGetStartACDriveTime,
            sGetTakeMeasurementTime,
            sGetStopACDriveTime,
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
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetResetACDriveTime, States_GetExecutionTime.sGetConfigureACDriveTime,
                Consts.STRXML_CmdGetResetACDriveTime, null
                ),
            //
            // GetConfigureACDriveTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetConfigureACDriveTime, States_GetExecutionTime.sGetStartACDriveTime,
                Consts.STRXML_CmdGetConfigureACDriveTime, null
                ),
            //
            // GetStartACDriveTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetStartACDriveTime, States_GetExecutionTime.sGetTakeMeasurementTime,
                Consts.STRXML_CmdGetStartACDriveTime, new string[,] {
                    { Consts.STRXML_ReqACDriveMode, string.Empty},
                }
                ),
            //
            // GetTakeMeasurementTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetTakeMeasurementTime, States_GetExecutionTime.sGetStopACDriveTime,
                Consts.STRXML_CmdGetTakeMeasurementTime, null
                ),
            //
            // GetStopACDriveTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetStopACDriveTime, States_GetExecutionTime.sGetReconfigureACDriveTime,
                Consts.STRXML_CmdGetStopACDriveTime, new string[,] {
                    { Consts.STRXML_ReqACDriveMode, string.Empty},
                }
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
            string logMessage = string.Empty;
            //Logfile.Write(logMessage);

            //
            // Initialise variables used in the state machine
            //
            double executionTime = 0.0;

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
                        case States_GetExecutionTime.sGetStartACDriveTime:
                            entry.commandArguments[0, 1] = specification.SetupId;
                            break;

                        case States_GetExecutionTime.sGetStopACDriveTime:
                            entry.commandArguments[0, 1] = specification.SetupId;
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

                        case States_GetExecutionTime.sGetConfigureACDriveTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspConfigureACDriveTime, 0);
                            break;

                        case States_GetExecutionTime.sGetStartACDriveTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspStartACDriveTime, 0);
                            break;

                        case States_GetExecutionTime.sGetTakeMeasurementTime:
                            double measurementTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspTakeMeasurementTime, 0);
                            stateExecutionTime = this.measurementCount * measurementTime;
                            break;

                        case States_GetExecutionTime.sGetStopACDriveTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspStopACDriveTime, 0);
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
