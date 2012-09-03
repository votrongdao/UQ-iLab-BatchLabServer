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
                        case NonExecuteCommands.GetTubeHomeDistance:

                            //
                            // Get tube home distance
                            //
                            int tubeHomeDistance = equipmentEngine.GetTubeHomeDistance();

                            //
                            // Add tube home distance to response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspTubeHomeDistance);
                            xmlElement.InnerText = tubeHomeDistance.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetTubeMoveTime:

                            //
                            // Get distance 'from' and 'to' from request
                            //
                            int distanceFrom = XmlUtilities.GetIntValue(xmlRequestNode, Consts.STRXML_ReqTubeDistanceFrom, 0);
                            int distanceTo = XmlUtilities.GetIntValue(xmlRequestNode, Consts.STRXML_ReqTubeDistanceTo, 0);

                            //
                            // Get tube move time
                            //
                            double tubeMoveTime = equipmentEngine.GetTubeMoveTime(distanceFrom, distanceTo);

                            //
                            // Add tube move time to response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspTubeMoveTime);
                            xmlElement.InnerText = tubeMoveTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetSourceHomeLocation:

                            //
                            // Get source home location
                            //
                            char sourceHomeLocation = equipmentEngine.GetSourceHomeLocation();

                            //
                            // Add source home location to response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspSourceHomeLocation);
                            xmlElement.InnerText = sourceHomeLocation.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetSourceSelectTime:

                            //
                            // Get source location from request
                            //
                            char sourceLocation = XmlUtilities.GetCharValue(xmlRequestNode, Consts.STRXML_ReqSourceLocation, Char.MinValue);

                            //
                            // Get source select time
                            //
                            double sourceSelectTime = equipmentEngine.GetSourceSelectTime(sourceLocation);

                            //
                            // Add source select time to response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspSourceSelectTime);
                            xmlElement.InnerText = sourceSelectTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetSourceReturnTime:

                            //
                            // Get source location from request
                            //
                            sourceLocation = XmlUtilities.GetCharValue(xmlRequestNode, Consts.STRXML_ReqSourceLocation, Char.MinValue);

                            //
                            // Get source return time
                            //
                            double sourceReturnTime = equipmentEngine.GetSourceReturnTime(sourceLocation);

                            //
                            // Add source return time to response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspSourceReturnTime);
                            xmlElement.InnerText = sourceReturnTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetAbsorberHomeLocation:

                            //
                            // Get absorber home location
                            //
                            char absorberHomeLocation = equipmentEngine.GetAbsorberHomeLocation();

                            //
                            // Add absorber home location to response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspAbsorberHomeLocation);
                            xmlElement.InnerText = absorberHomeLocation.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetAbsorberSelectTime:

                            //
                            // Get absorber location from request
                            //
                            char absorberLocation = XmlUtilities.GetCharValue(xmlRequestNode, Consts.STRXML_ReqAbsorberLocation, Char.MinValue);

                            //
                            // Get absorber select time
                            //
                            double absorberSelectTime = equipmentEngine.GetAbsorberSelectTime(absorberLocation);

                            //
                            // Add absorber select time to response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspAbsorberSelectTime);
                            xmlElement.InnerText = absorberSelectTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetAbsorberReturnTime:

                            //
                            // Get absorber location from request
                            //
                            absorberLocation = XmlUtilities.GetCharValue(xmlRequestNode, Consts.STRXML_ReqAbsorberLocation, Char.MinValue);

                            //
                            // Get absorber return time
                            //
                            double absorberReturnTime = equipmentEngine.GetAbsorberReturnTime(absorberLocation);

                            //
                            // Add absorber return time to response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspAbsorberReturnTime);
                            xmlElement.InnerText = absorberReturnTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetLcdWriteLineTime:
                            //
                            // Get LCD writeline time
                            //
                            double lcdWriteLineTime = equipmentEngine.GetLcdWriteLineTime();

                            //
                            // Add capture data time to response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspLcdWriteLineTime);
                            xmlElement.InnerText = lcdWriteLineTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetCaptureDataTime:
                            //
                            // Get duration from request
                            //
                            int duration = XmlUtilities.GetIntValue(xmlRequestNode, Consts.STRXML_ReqDuration, 0);

                            //
                            // Get capture data time
                            //
                            double captureDataTime = equipmentEngine.GetCaptureDataTime(duration);

                            //
                            // Add capture data time to response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspCaptureDataTime);
                            xmlElement.InnerText = captureDataTime.ToString();
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
                            case ExecuteCommands.SetTubeDistance:

                                //
                                // Get tube distance from request
                                //
                                int tubeDistance = XmlUtilities.GetIntValue(xmlRequestNode, Consts.STRXML_ReqTubeDistance, 0);

                                //
                                // Set tube distance
                                //
                                commandInfo.parameters = new object[] { tubeDistance };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.SetSourceLocation:

                                //
                                // Get source location from request
                                //
                                char sourceLocation = XmlUtilities.GetCharValue(xmlRequestNode, Consts.STRXML_ReqSourceLocation, Char.MinValue);

                                //
                                // Set source location
                                //
                                commandInfo.parameters = new object[] { sourceLocation };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.SetAbsorberLocation:

                                //
                                // Get absorber location from request
                                //
                                char absorberLocation = XmlUtilities.GetCharValue(xmlRequestNode, Consts.STRXML_ReqAbsorberLocation, Char.MinValue);

                                //
                                // Set absorber location
                                //
                                commandInfo.parameters = new object[] { absorberLocation };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.GetCaptureData:

                                //
                                // Get duration from request
                                //
                                int duration = XmlUtilities.GetIntValue(xmlRequestNode, Consts.STRXML_ReqDuration, 0);

                                //
                                // Get radiation count
                                //
                                int count = -1;
                                commandInfo.parameters = new object[] { duration };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                if (commandInfo.success == true)
                                {
                                    count = (int)commandInfo.results[0];
                                }

                                //
                                // Add radiation count to response
                                //
                                xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspCount);
                                xmlElement.InnerText = count.ToString();
                                xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                                break;

                            case ExecuteCommands.WriteLcdLine:

                                //
                                // Get LCD line number and message from request
                                //
                                int lcdLineNo = XmlUtilities.GetIntValue(xmlRequestNode, Consts.STRXML_ReqLcdLineNo, 0);
                                string lcdMessage = XmlUtilities.GetXmlValue(xmlRequestNode, Consts.STRXML_ReqLcdMessage, false);

                                //
                                // Write message to LCD
                                //
                                commandInfo.parameters = new object[] { lcdLineNo, lcdMessage };
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
                Logfile.Write(ex.Message);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return strXmlResponse;
        }

    }
}
