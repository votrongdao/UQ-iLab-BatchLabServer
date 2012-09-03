using System;
using System.Diagnostics;
using System.Xml;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServerEngine.Drivers.Equipment;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverRadioactivity : DriverEquipmentGeneric
    {
        #region Class Constants and Variables

        //
        // State machine states
        //
        private enum States_GetExecutionTime
        {
            sGetLcdWriteLineTime,
            sGetAbsorberSelectTime,
            sGetSourceSelectTime,
            sGetTubeHomeDistance, sGetTubeMoveTime,
            sGetCaptureDataTime,
            sGetSourceReturnTime,
            sGetAbsorberReturnTime,
            sGetTubeReturnTime,
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
            // Get LCD writeline time
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetLcdWriteLineTime, States_GetExecutionTime.sGetAbsorberSelectTime,
                Consts.STRXML_CmdGetLcdWriteLineTime, null),
            //
            // Get absorber select time
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetAbsorberSelectTime, States_GetExecutionTime.sGetSourceSelectTime,
                Consts.STRXML_CmdGetAbsorberSelectTime, new string[,] {
                    { Consts.STRXML_ReqAbsorberLocation, string.Empty},
                } ),
            //
            // Get source select time
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetSourceSelectTime, States_GetExecutionTime.sGetTubeHomeDistance,
                Consts.STRXML_CmdGetSourceSelectTime, new string[,] {
                    { Consts.STRXML_ReqSourceLocation, string.Empty},
                } ),
            //
            // Get tube move time
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetTubeHomeDistance, States_GetExecutionTime.sGetTubeMoveTime,
                Consts.STRXML_CmdGetTubeHomeDistance, null),
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetTubeMoveTime, States_GetExecutionTime.sGetCaptureDataTime,
                Consts.STRXML_CmdGetTubeMoveTime, new string[,] {
                    { Consts.STRXML_ReqTubeDistanceFrom, string.Empty},
                    { Consts.STRXML_ReqTubeDistanceTo, string.Empty}
                } ),
            //
            // Get capture data time
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetCaptureDataTime, States_GetExecutionTime.sGetSourceReturnTime,
                Consts.STRXML_CmdGetCaptureDataTime, new string[,] {
                    { Consts.STRXML_ReqDuration, string.Empty},
                } ),
            //
            // Get source return time
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetSourceReturnTime, States_GetExecutionTime.sGetAbsorberReturnTime,
                Consts.STRXML_CmdGetSourceReturnTime, new string[,] {
                    { Consts.STRXML_ReqSourceLocation, string.Empty},
                } ),
            //
            // Get absorber return time
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetAbsorberReturnTime, States_GetExecutionTime.sGetTubeReturnTime,
                Consts.STRXML_CmdGetAbsorberReturnTime, new string[,] {
                    { Consts.STRXML_ReqAbsorberLocation, string.Empty},
                } ),
            //
            // Get tube return to home time
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetTubeReturnTime, States_GetExecutionTime.sCompleted,
                Consts.STRXML_CmdGetTubeMoveTime, new string[,] {
                    { Consts.STRXML_ReqTubeDistanceFrom, string.Empty},
                    { Consts.STRXML_ReqTubeDistanceTo, string.Empty}
                } ),
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
            string strDistanceList = null;
            for (int i = 0; i < specification.DistanceList.Length; i++)
            {
                if (i > 0)
                {
                    strDistanceList += Consts.CHR_CsvSplitter.ToString();
                }
                strDistanceList += specification.DistanceList[i].ToString();
            }
            string logMessage = STRLOG_Distance + strDistanceList;
            logMessage += Logfile.STRLOG_Spacer + STRLOG_Duration + specification.Duration.ToString();
            logMessage += Logfile.STRLOG_Spacer + STRLOG_Repeat + specification.Repeat.ToString();
            Logfile.Write(logMessage);

            //
            // Initialise variables used in the state machine
            //
            double executionTime = 0.0;
            double lcdWriteLineTime = 0.0;
            int tubeHomeDistance = 0;
            int distanceIndex = 0;

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
                        throw new ArgumentOutOfRangeException(state.ToString(), "State not found!");
                    }

                    //
                    // Get table entry and save next state
                    //
                    SMTableEntry_GetExecutionTime entry = smTable_GetExecutionTime[index];
                    States_GetExecutionTime nextState = entry.nextState;

                    Trace.Write(" [ " + entry.currentState.ToString() + ": " + entry.currentState.ToString());

                    //
                    // Add command arguments where required
                    //
                    switch (entry.currentState)
                    {
                        case States_GetExecutionTime.sGetAbsorberSelectTime:
                            entry.commandArguments[0, 1] = specification.AbsorberList[0].location.ToString();
                            break;

                        case States_GetExecutionTime.sGetSourceSelectTime:
                            entry.commandArguments[0, 1] = specification.SourceLocation.ToString();
                            break;

                        case States_GetExecutionTime.sGetTubeMoveTime:
                            if (distanceIndex == 0)
                            {
                                // From home to first distance
                                entry.commandArguments[0, 1] = tubeHomeDistance.ToString();
                                entry.commandArguments[1, 1] = specification.DistanceList[distanceIndex].ToString();
                            }
                            else if (distanceIndex < specification.DistanceList.Length)
                            {
                                // Everything in between
                                entry.commandArguments[0, 1] = specification.DistanceList[distanceIndex - 1].ToString();
                                entry.commandArguments[1, 1] = specification.DistanceList[distanceIndex].ToString();
                            }
                            break;

                        case States_GetExecutionTime.sGetCaptureDataTime:
                            entry.commandArguments[0, 1] = specification.Duration.ToString();
                            break;

                        case States_GetExecutionTime.sGetSourceReturnTime:
                            entry.commandArguments[0, 1] = specification.SourceLocation.ToString();
                            break;

                        case States_GetExecutionTime.sGetAbsorberReturnTime:
                            entry.commandArguments[0, 1] = specification.AbsorberList[0].location.ToString();
                            break;

                        case States_GetExecutionTime.sGetTubeReturnTime:
                            entry.commandArguments[0, 1] = specification.DistanceList[specification.DistanceList.Length - 1].ToString();
                            entry.commandArguments[1, 1] = tubeHomeDistance.ToString();
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
                        case States_GetExecutionTime.sGetLcdWriteLineTime:
                            lcdWriteLineTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspLcdWriteLineTime, 0);

                            // Time to ready LCD when completed
                            stateExecutionTime = lcdWriteLineTime * 2;
                            break;

                        case States_GetExecutionTime.sGetAbsorberSelectTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspAbsorberSelectTime, 0.0);
                            stateExecutionTime += lcdWriteLineTime * 2;
                            break;

                        case States_GetExecutionTime.sGetSourceSelectTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspSourceSelectTime, 0.0);
                            stateExecutionTime += lcdWriteLineTime * 2;
                            break;

                        case States_GetExecutionTime.sGetTubeHomeDistance:
                            tubeHomeDistance = XmlUtilities.GetIntValue(xmlResponseNode, Consts.STRXML_RspTubeHomeDistance, 0);
                            break;

                        case States_GetExecutionTime.sGetTubeMoveTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspTubeMoveTime, 0.0);
                            stateExecutionTime += lcdWriteLineTime * 2;
                            break;

                        case States_GetExecutionTime.sGetCaptureDataTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspCaptureDataTime, 0.0);
                            stateExecutionTime += lcdWriteLineTime * 2;
                            stateExecutionTime *= specification.Repeat;
                            if (++distanceIndex < specification.DistanceList.Length)
                            {
                                // Next distance
                                nextState = States_GetExecutionTime.sGetTubeMoveTime;
                            }
                            break;

                        case States_GetExecutionTime.sGetSourceReturnTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspSourceReturnTime, 0.0);
                            stateExecutionTime += lcdWriteLineTime * 2;
                            break;

                        case States_GetExecutionTime.sGetAbsorberReturnTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspAbsorberReturnTime, 0.0);
                            stateExecutionTime += lcdWriteLineTime * 2;
                            break;

                        case States_GetExecutionTime.sGetTubeReturnTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspTubeMoveTime, 0.0);
                            stateExecutionTime += lcdWriteLineTime * 2;
                            break;

                        default:
                            break;
                    }

                    Trace.WriteLine("  nextState: " + entry.nextState.ToString() + " ]");
                    Trace.WriteLine(" stateExecutionTime: " + stateExecutionTime.ToString());

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

            //
            // Round execution time to the nearest integer
            //
            int execTime = (int)(executionTime + 0.5);

            logMessage = STRLOG_ExecutionTime + execTime.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return execTime;
        }

    }
}
