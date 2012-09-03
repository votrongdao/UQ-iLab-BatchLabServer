using System;
using System.Diagnostics;
using System.Xml;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServerEngine.Drivers.Setup;
using Library.LabServerEngine.Drivers.Equipment;

namespace Library.LabServer.Drivers.Setup
{
    public partial class DriverNetwork : DriverEquipmentGeneric
    {
        #region Class Constants and Variables

        //
        // State machine states
        //
        private enum States_GetExecutionTime
        {
            sGetTimeOfDayTime,
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
            // GetTimeOfDayTime
            //
            new SMTableEntry_GetExecutionTime(States_GetExecutionTime.sGetTimeOfDayTime, States_GetExecutionTime.sCompleted,
                Consts.STRXML_CmdGetTimeOfDayTime, null
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
                        throw new ArgumentOutOfRangeException(state.ToString(), "State not found!");
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
                        case States_GetExecutionTime.sGetTimeOfDayTime:
                            stateExecutionTime = XmlUtilities.GetRealValue(xmlResponseNode, Consts.STRXML_RspTimeOfDayTime, 0);
                            break;

                        default:
                            break;
                    }

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
