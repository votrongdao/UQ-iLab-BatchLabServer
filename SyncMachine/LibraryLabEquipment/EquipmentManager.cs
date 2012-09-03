using System;
using System.Xml;
using Library.Lab;
using Library.LabEquipment.Engine;
using Library.LabEquipment.Drivers;

namespace Library.LabEquipment
{
    public class EquipmentManager : LabEquipmentManager
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "EquipmentManager";

        public EquipmentEngine EquipmentEngine
        {
            get { return (EquipmentEngine)this.labEquipmentEngine; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public EquipmentManager(string rootFilePath)
            : base(rootFilePath)
        {
        }

        //-------------------------------------------------------------------------------------------------//

        public override void Create()
        {
            const string STRLOG_MethodName = "Create";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Create an instance of lab equipment engine
            //
            this.labEquipmentEngine = new EquipmentEngine(this.rootFilePath);

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public override string ExecuteXmlRequest(string xmlRequest)
        {
            string strXmlResponse = string.Empty;

            const string STRLOG_MethodName = "ExecuteXmlRequest";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            EquipmentEngine equipmentEngine = (EquipmentEngine)this.labEquipmentEngine;

            try
            {
                bool success = true;
                string errorMessage = string.Empty;
                CommandInfo commandInfo = null;
                CommandInfo resultInfo = null;

                //
                // Create the XML response
                //
                XmlDocument xmlResponseDocument = new XmlDocument();
                XmlElement xmlElement = xmlResponseDocument.CreateElement(Engine.Consts.STRXML_Response);
                xmlResponseDocument.AppendChild(xmlElement);

                //
                // Add success of command execution and update later
                //
                xmlElement = xmlResponseDocument.CreateElement(Engine.Consts.STRXML_RspSuccess);
                xmlElement.InnerText = success.ToString();
                xmlResponseDocument.DocumentElement.AppendChild(xmlElement);

                //
                // Parse XML request for the command
                //
                XmlDocument xmlRequestDocument = new XmlDocument();
                xmlRequestDocument.LoadXml(xmlRequest);
                XmlNode xmlRequestNode = XmlUtilities.GetXmlRootNode(xmlRequestDocument, Engine.Consts.STRXML_Request);
                string strCommand = XmlUtilities.GetXmlValue(xmlRequestNode, Engine.Consts.STRXML_Command, false);

                NonExecuteCommands nonExecuteCommand = (NonExecuteCommands)(-1);
                ExecuteCommands executeCommand = (ExecuteCommands)(-1);
                try
                {
                    //
                    // Try to convert to a non-execute command type that doesn't require powerdown to be suspended
                    //
                    nonExecuteCommand = (NonExecuteCommands)Enum.Parse(typeof(NonExecuteCommands), strCommand);
                    Logfile.Write(STRLOG_Command + nonExecuteCommand.ToString());
                }
                catch
                {
                    try
                    {
                        //
                        // Try to convert to an execute command type that does require powerdown to be suspended
                        //
                        executeCommand = (ExecuteCommands)Enum.Parse(typeof(ExecuteCommands), strCommand);
                        Logfile.Write(STRLOG_Command + executeCommand.ToString());

                        //
                        // Check that powerdown has been suspended before executing the command
                        //
                        if (this.labEquipmentEngine.IsPowerdownSuspended == false)
                        {
                            //
                            // Suspend the powerdown before executing the command
                            //
                            this.labEquipmentEngine.SuspendPowerdown();
                        }
                    }
                    catch
                    {
                        //
                        // Unknown command
                        //
                        success = false;
                        errorMessage = STRERR_UnknownCommand + strCommand;
                    }
                }

                if (success == true && nonExecuteCommand != (NonExecuteCommands)(-1))
                {
                    //
                    // Process the non-execute command
                    //
                    switch (nonExecuteCommand)
                    {
                        case NonExecuteCommands.GetExecutionTime:
                            //
                            // Get specification in XML format from request
                            //
                            string xmlSpecification = XmlUtilities.GetXmlValue(xmlRequestNode, Consts.STRXML_ReqSpecification, false);

                            //
                            // Get the time to execute the specified setup
                            //
                            int executionTime = equipmentEngine.GetExecutionTime(xmlSpecification);

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspExecutionTime);
                            xmlElement.InnerText = executionTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetExecutionStatus:
                            //
                            // Get the execution status
                            //
                            DriverMachine.ExecutionStatus executionStatus = equipmentEngine.GetExecutionStatus();

                            //
                            // Add the execution status to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspExecutionStatus);
                            xmlElement.InnerText = executionStatus.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);

                            //
                            // Get the execution time remaining
                            //
                            int executionTimeRemaining = equipmentEngine.GetExecutionTimeRemaining();

                            //
                            // Add the execution status to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspExecutionTimeRemaining);
                            xmlElement.InnerText = executionTimeRemaining.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetExecutionResultStatus:
                            //
                            // Get the execution result status
                            //
                            DriverMachine.ExecutionStatus executionResultStatus = equipmentEngine.GetExecutionResultStatus();

                            //
                            // Add the execution result status to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspExecutionResultStatus);
                            xmlElement.InnerText = executionResultStatus.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetExecutionResults:
                            //
                            // Get the execution results
                            //
                            string executionResults = equipmentEngine.GetExecutionResults();

                            //
                            // Add the execution status to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspExecutionResults);
                            xmlElement.InnerXml = executionResults;
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;
                    }
                }
                else if (success == true && executeCommand != (ExecuteCommands)(-1))
                {
                    //
                    // Only one execute command allowed at a time
                    //
                    lock (this.managerLock)
                    {
                        //
                        // Create an instance of the command info ready to fill in
                        //
                        commandInfo = new CommandInfo(executeCommand);

                        //
                        // Process the execute command
                        //
                        switch (executeCommand)
                        {
                            case ExecuteCommands.StartExecution:
                                //
                                // Get specification in XML format from request
                                //
                                string xmlSpecification = XmlUtilities.GetXmlValue(xmlRequestNode, Consts.STRXML_ReqSpecification, false);

                                //
                                // Start execution with the given specification
                                //
                                commandInfo.parameters = new object[] { xmlSpecification };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;
                        }

                        success = resultInfo.success;
                        errorMessage = resultInfo.errorMessage;
                    }
                }

                //
                // Update success of command execution
                //
                XmlNode xmlResponseNode = XmlUtilities.GetXmlRootNode(xmlResponseDocument, Engine.Consts.STRXML_Response);
                XmlUtilities.SetXmlValue(xmlResponseNode, Engine.Consts.STRXML_RspSuccess, success.ToString(), false);
                if (success == false)
                {
                    //
                    // Create error response
                    //
                    xmlElement = xmlResponseDocument.CreateElement(Engine.Consts.STRXML_RspErrorMessage);
                    xmlElement.InnerText = errorMessage;
                    xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                }
                strXmlResponse = xmlResponseDocument.InnerXml;
            }
            catch (ArgumentException)
            {
                //
                // Unknown command so pass to base class to process
                //
                strXmlResponse = base.ExecuteXmlRequest(xmlRequest);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return strXmlResponse;
        }

    }
}
