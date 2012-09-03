using System;
using System.Diagnostics;
using System.Web;
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
        // String constants for logfile messages
        //
        private const string STRLOG_Distance = " Distance: ";
        private const string STRLOG_Duration = " Duration: ";
        private const string STRLOG_Repeat = " Repeat: ";

        //
        // String constants for error messages
        //

        //
        // String constants for serial LCD messages
        //
        private const string STRLCD_Ready = "Ready.";
        private const string STRLCD_LineNoOne = "1";
        private const string STRLCD_LineNoTwo = "2";
        private const string STRLCD_SelectAbsorber = "Select absorber:";
        private const string STRLCD_SelectSource = "Select source:";
        private const string STRLCD_SetDistance = "Set distance:";
        private const string STRLCD_Millimetres = "mm";
        private const string STRLCD_CaptureCounts = "Capture counts:";
        private const string STRLCD_ReturnSource = "Return source";
        private const string STRLCD_ReturnAbsorber = "Return absorber";
        private const string STRLCD_ReturnTube = "Return tube";
        private const string STRLCD_Seconds = "sec";
        private const string STRLCD_Break = "-";
        private const string STRLCD_Of = "/";
        private const string STRLCD_EmptyString = "                ";

        //
        // Data driven state machine for Execute()
        //
        private enum States_Execute
        {
            sSuspendPowerdown,
            sSelectAbsorberMessageLine1, sSelectAbsorberMessageLine2, sSelectAbsorber,
            sSelectSourceMessageLine1, sSelectSourceMessageLine2, sSelectSource,
            sSetTubeDistanceMessageLine1, sSetTubeDistanceMessageLine2, sSetTubeDistance,
            sCaptureDataMessageLine1, sCaptureDataMessageLine2, sCaptureData,
            sReturnSourceMessageLine1, sReturnSourceMessageLine2, sGetSourceHomeLocation, sReturnSource,
            sReturnAbsorberMessageLine1, sReturnAbsorberMessageLine2, sGetAbsorberHomeLocation, sReturnAbsorber,
            sReturnTubeMessageLine1, sReturnTubeMessageLine2, sGetTubeHomeDistance, sReturnTube,
            sReturnToReadyMessageLine1, sReturnToReadyMessageLine2,
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
            new SMTableEntry_Execute(States_Execute.sSuspendPowerdown, States_Execute.sSelectAbsorberMessageLine1, States_Execute.sResumePowerdown,
                null, null),

            //
            // Select absorber
            //
            new SMTableEntry_Execute(States_Execute.sSelectAbsorberMessageLine1, States_Execute.sSelectAbsorberMessageLine2, States_Execute.sResumePowerdown,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoOne },
                    { Consts.STRXML_ReqLcdMessage, HttpUtility.UrlEncode(STRLCD_SelectAbsorber) }
                } ),
            new SMTableEntry_Execute(States_Execute.sSelectAbsorberMessageLine2, States_Execute.sSelectAbsorber, States_Execute.sResumePowerdown,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoTwo },
                    { Consts.STRXML_ReqLcdMessage, string.Empty }
                } ),
            new SMTableEntry_Execute(States_Execute.sSelectAbsorber, States_Execute.sSelectSourceMessageLine1, States_Execute.sResumePowerdown,
                Consts.STRXML_CmdSetAbsorberLocation, new string[,] {
                    { Consts.STRXML_ReqAbsorberLocation, string.Empty }
                } ),

            //
            // Select source
            //
            new SMTableEntry_Execute(States_Execute.sSelectSourceMessageLine1, States_Execute.sSelectSourceMessageLine2, States_Execute.sReturnAbsorberMessageLine1,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoOne },
                    { Consts.STRXML_ReqLcdMessage, HttpUtility.UrlEncode(STRLCD_SelectSource) }
                } ),
            new SMTableEntry_Execute(States_Execute.sSelectSourceMessageLine2, States_Execute.sSelectSource, States_Execute.sReturnAbsorberMessageLine1,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoTwo },
                    { Consts.STRXML_ReqLcdMessage, string.Empty }
                } ),
            new SMTableEntry_Execute(States_Execute.sSelectSource, States_Execute.sSetTubeDistanceMessageLine1, States_Execute.sReturnAbsorberMessageLine1,
                Consts.STRXML_CmdSetSourceLocation, new string[,] {
                    { Consts.STRXML_ReqSourceLocation, string.Empty }
                } ),

            //
            // Select tube distance
            //
            new SMTableEntry_Execute(States_Execute.sSetTubeDistanceMessageLine1, States_Execute.sSetTubeDistanceMessageLine2, States_Execute.sReturnSourceMessageLine1,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoOne },
                    { Consts.STRXML_ReqLcdMessage, HttpUtility.UrlEncode(STRLCD_SetDistance) }
                } ),
            new SMTableEntry_Execute(States_Execute.sSetTubeDistanceMessageLine2, States_Execute.sSetTubeDistance, States_Execute.sReturnSourceMessageLine1,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoTwo },
                    { Consts.STRXML_ReqLcdMessage, string.Empty }
                } ),
            new SMTableEntry_Execute(States_Execute.sSetTubeDistance, States_Execute.sCaptureDataMessageLine1, States_Execute.sReturnSourceMessageLine1,
                Consts.STRXML_CmdSetTubeDistance, new string[,] {
                    { Consts.STRXML_ReqTubeDistance, string.Empty }
                } ),

            //
            // Get capture data
            //
            new SMTableEntry_Execute(States_Execute.sCaptureDataMessageLine1, States_Execute.sCaptureDataMessageLine2, States_Execute.sReturnSourceMessageLine1,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoOne },
                    { Consts.STRXML_ReqLcdMessage, HttpUtility.UrlEncode(STRLCD_CaptureCounts) }
                } ),
            new SMTableEntry_Execute(States_Execute.sCaptureDataMessageLine2, States_Execute.sCaptureData, States_Execute.sReturnSourceMessageLine1,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoTwo },
                    { Consts.STRXML_ReqLcdMessage, string.Empty }
                } ),
            new SMTableEntry_Execute(States_Execute.sCaptureData, States_Execute.sReturnSourceMessageLine1, States_Execute.sReturnSourceMessageLine1,
                Consts.STRXML_CmdGetCaptureData, new string[,] {
                    { Consts.STRXML_ReqDuration, string.Empty }
                } ),

            //
            // Return source to home position
            //
            new SMTableEntry_Execute(States_Execute.sReturnSourceMessageLine1, States_Execute.sReturnSourceMessageLine2, States_Execute.sReturnAbsorberMessageLine1,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoOne },
                    { Consts.STRXML_ReqLcdMessage, HttpUtility.UrlEncode(STRLCD_ReturnSource) }
                } ),
            new SMTableEntry_Execute(States_Execute.sReturnSourceMessageLine2, States_Execute.sGetSourceHomeLocation, States_Execute.sReturnAbsorberMessageLine1,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoTwo },
                    { Consts.STRXML_ReqLcdMessage, HttpUtility.UrlEncode(STRLCD_EmptyString) }
                } ),
            new SMTableEntry_Execute(States_Execute.sGetSourceHomeLocation, States_Execute.sReturnSource, States_Execute.sReturnAbsorberMessageLine1,
                Consts.STRXML_CmdGetSourceHomeLocation, null),
            new SMTableEntry_Execute(States_Execute.sReturnSource, States_Execute.sReturnAbsorberMessageLine1, States_Execute.sReturnAbsorberMessageLine1,
                Consts.STRXML_CmdSetSourceLocation, new string[,] {
                    { Consts.STRXML_ReqSourceLocation, string.Empty }
                } ),

            //
            // Return absorber to home position
            //
            new SMTableEntry_Execute(States_Execute.sReturnAbsorberMessageLine1, States_Execute.sReturnAbsorberMessageLine2, States_Execute.sReturnTubeMessageLine1,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoOne },
                    { Consts.STRXML_ReqLcdMessage, HttpUtility.UrlEncode(STRLCD_ReturnAbsorber) }
                } ),
            new SMTableEntry_Execute(States_Execute.sReturnAbsorberMessageLine2, States_Execute.sGetAbsorberHomeLocation, States_Execute.sReturnTubeMessageLine1,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoTwo },
                    { Consts.STRXML_ReqLcdMessage, HttpUtility.UrlEncode(STRLCD_EmptyString) }
                } ),
            new SMTableEntry_Execute(States_Execute.sGetAbsorberHomeLocation, States_Execute.sReturnAbsorber, States_Execute.sReturnTubeMessageLine1,
                Consts.STRXML_CmdGetAbsorberHomeLocation, null),
            new SMTableEntry_Execute(States_Execute.sReturnAbsorber, States_Execute.sReturnTubeMessageLine1, States_Execute.sReturnTubeMessageLine1,
                Consts.STRXML_CmdSetAbsorberLocation, new string[,] {
                    { Consts.STRXML_ReqAbsorberLocation, string.Empty }
                } ),

            //
            // Return tube to home position
            //
            new SMTableEntry_Execute(States_Execute.sReturnTubeMessageLine1, States_Execute.sReturnTubeMessageLine2, States_Execute.sReturnToReadyMessageLine1,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoOne },
                    { Consts.STRXML_ReqLcdMessage, HttpUtility.UrlEncode(STRLCD_ReturnTube) }
                } ),
            new SMTableEntry_Execute(States_Execute.sReturnTubeMessageLine2, States_Execute.sGetTubeHomeDistance, States_Execute.sReturnToReadyMessageLine1,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoTwo },
                    { Consts.STRXML_ReqLcdMessage, HttpUtility.UrlEncode(STRLCD_EmptyString) }
                } ),
            new SMTableEntry_Execute(States_Execute.sGetTubeHomeDistance, States_Execute.sReturnTube, States_Execute.sReturnToReadyMessageLine1,
                Consts.STRXML_CmdGetTubeHomeDistance, null),
            new SMTableEntry_Execute(States_Execute.sReturnTube, States_Execute.sReturnToReadyMessageLine1, States_Execute.sReturnToReadyMessageLine1,
                Consts.STRXML_CmdSetTubeDistance, new string[,] {
                    { Consts.STRXML_ReqTubeDistance, string.Empty }
                } ),

            //
            // Display ready message
            //
            new SMTableEntry_Execute(States_Execute.sReturnToReadyMessageLine1, States_Execute.sReturnToReadyMessageLine2, States_Execute.sResumePowerdown,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoOne },
                    { Consts.STRXML_ReqLcdMessage, HttpUtility.UrlEncode(STRLCD_Ready) }
                } ),
            new SMTableEntry_Execute(States_Execute.sReturnToReadyMessageLine2, States_Execute.sResumePowerdown, States_Execute.sResumePowerdown,
                Consts.STRXML_CmdWriteLcdLine, new string[,] {
                    { Consts.STRXML_ReqLcdLineNo, STRLCD_LineNoTwo },
                    { Consts.STRXML_ReqLcdMessage, HttpUtility.UrlEncode(STRLCD_EmptyString) }
                } ),

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
            // Create an instance of the result info ready to fill in
            //
            ResultInfo resultInfo = new ResultInfo();
            resultInfo.statusCode = StatusCodes.Running;
            resultInfo.dataType = DataTypes.Real;

            //
            // Create data structures to hold the results
            //
            resultInfo.dataVectors = new int[specification.DistanceList.Length, specification.Repeat];

            //
            // Initialise variables used in the state machine
            //
            int distanceIndex = 0;
            int repeatIndex = 0;
            int tubeHomeDistance = 0;
            char sourceHomeLocation = (char)0;
            char absorberHomeLocation = (char)0;

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
                        case States_Execute.sSelectAbsorberMessageLine2:
                            entry.commandArguments[1, 1] = HttpUtility.UrlEncode(specification.AbsorberList[0].name);
                            break;

                        case States_Execute.sSelectAbsorber:
                            entry.commandArguments[0, 1] = specification.AbsorberList[0].location.ToString();
                            break;

                        case States_Execute.sSelectSourceMessageLine2:
                            entry.commandArguments[1, 1] = HttpUtility.UrlEncode(specification.SourceName);
                            break;

                        case States_Execute.sSelectSource:
                            entry.commandArguments[0, 1] = specification.SourceLocation.ToString();
                            break;

                        case States_Execute.sSetTubeDistanceMessageLine2:
                            entry.commandArguments[1, 1] = HttpUtility.UrlEncode(specification.DistanceList[distanceIndex].ToString() + STRLCD_Millimetres);
                            break;

                        case States_Execute.sSetTubeDistance:
                            entry.commandArguments[0, 1] = specification.DistanceList[distanceIndex].ToString();
                            break;

                        case States_Execute.sCaptureDataMessageLine2:
                            string lcdMessage = specification.DistanceList[distanceIndex].ToString() + STRLCD_Millimetres;
                            lcdMessage += STRLCD_Break + specification.Duration.ToString() + STRLCD_Seconds;
                            lcdMessage += STRLCD_Break + (repeatIndex + 1).ToString() + STRLCD_Of + specification.Repeat.ToString();
                            entry.commandArguments[1, 1] = HttpUtility.UrlEncode(lcdMessage);
                            break;

                        case States_Execute.sCaptureData:
                            entry.commandArguments[0, 1] = specification.Duration.ToString();
                            break;

                        case States_Execute.sReturnSource:
                            entry.commandArguments[0, 1] = sourceHomeLocation.ToString();
                            break;

                        case States_Execute.sReturnAbsorber:
                            entry.commandArguments[0, 1] = absorberHomeLocation.ToString();
                            break;

                        case States_Execute.sReturnTube:
                            entry.commandArguments[0, 1] = tubeHomeDistance.ToString();
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
                        case States_Execute.sCaptureData:
                            resultInfo.dataVectors[distanceIndex, repeatIndex] = XmlUtilities.GetIntValue(xmlResponseNode, Consts.STRXML_RspCount, 0);
                            if (++repeatIndex == specification.Repeat)
                            {
                                if (++distanceIndex == specification.DistanceList.Length)
                                {
                                    // All distances completed
                                    break;
                                }

                                // Next distance
                                repeatIndex = 0;
                                nextState = States_Execute.sSetTubeDistanceMessageLine1;
                                break;
                            }

                            // Next repeat
                            nextState = States_Execute.sCaptureDataMessageLine1;
                            break;

                        case States_Execute.sGetSourceHomeLocation:
                            sourceHomeLocation = XmlUtilities.GetCharValue(xmlResponseNode, Consts.STRXML_RspSourceHomeLocation, (char)0);
                            break;

                        case States_Execute.sGetAbsorberHomeLocation:
                            absorberHomeLocation = XmlUtilities.GetCharValue(xmlResponseNode, Consts.STRXML_RspAbsorberHomeLocation, (char)0);
                            break;

                        case States_Execute.sGetTubeHomeDistance:
                            tubeHomeDistance = XmlUtilities.GetIntValue(xmlResponseNode, Consts.STRXML_RspTubeHomeDistance, 0);
                            break;

                        default:
                            break;
                    }

                    Trace.WriteLine("nextState: " + entry.nextState.ToString());

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
            // Calculate actual execution time and round to the nearest integer
            //
            TimeSpan timeSpan = DateTime.Now - startDateTime;
            int execTime = (int)(timeSpan.TotalSeconds + 0.5);

            logMessage = STRLOG_StatusCode + resultInfo.statusCode
                + Logfile.STRLOG_Spacer + STRLOG_ExecutionTime + execTime.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return resultInfo;
        }

    }
}
