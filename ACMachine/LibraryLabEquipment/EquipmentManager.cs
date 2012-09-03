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
                        case NonExecuteCommands.GetResetACDriveTime:

                            //
                            // Get the time to reset the AC drive
                            //
                            int resetACDriveTime = equipmentEngine.GetResetACDriveTime();

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspResetACDriveTime);
                            xmlElement.InnerText = resetACDriveTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetConfigureACDriveTime:

                            //
                            // Get the time to configure the AC drive
                            //
                            int configureACDriveTime = equipmentEngine.GetConfigureACDriveTime();

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspConfigureACDriveTime);
                            xmlElement.InnerText = configureACDriveTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetStartACDriveTime:

                            //
                            // Get AC drive mode from request
                            //
                            string strACDriveMode = XmlUtilities.GetXmlValue(xmlRequestNode, Consts.STRXML_ReqACDriveMode, false);
                            RedLion.ACDriveModes acDriveMode = (RedLion.ACDriveModes)Enum.Parse(typeof(RedLion.ACDriveModes), strACDriveMode);

                            //
                            // Get the time to start the AC drive for the specified mode
                            //
                            int startACDriveTime = equipmentEngine.GetStartACDriveTime(acDriveMode);

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspStartACDriveTime);
                            xmlElement.InnerText = startACDriveTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetStopACDriveTime:

                            //
                            // Get AC drive mode from request
                            //
                            strACDriveMode = XmlUtilities.GetXmlValue(xmlRequestNode, Consts.STRXML_ReqACDriveMode, false);
                            acDriveMode = (RedLion.ACDriveModes)Enum.Parse(typeof(RedLion.ACDriveModes), strACDriveMode);

                            //
                            // Get the time to stop the AC drive for the specified mode
                            //
                            int stopACDriveTime = equipmentEngine.GetStopACDriveTime(acDriveMode);

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspStopACDriveTime);
                            xmlElement.InnerText = stopACDriveTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetTakeMeasurementTime:

                            //
                            // Get the time to take a measurement
                            //
                            int takeMeasurementTime = equipmentEngine.GetTakeMeasurementTime();

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspTakeMeasurementTime);
                            xmlElement.InnerText = takeMeasurementTime.ToString();
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
                            case ExecuteCommands.CreateConnection:

                                //
                                // Create a connection to the RedLion controller
                                //
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.CloseConnection:

                                //
                                // Close the connection to the RedLion controller
                                //
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.ResetACDrive:

                                //
                                // Reset the AC drive controller
                                //
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.ConfigureACDrive:

                                //
                                // Get AC drive configuration from request
                                //
                                string strACDriveConfig = XmlUtilities.GetXmlValue(xmlRequestNode, Consts.STRXML_ReqACDriveConfig, false);
                                RedLion.ACDriveConfigs acDriveConfig = (RedLion.ACDriveConfigs)Enum.Parse(typeof(RedLion.ACDriveConfigs), strACDriveConfig);

                                //
                                // Configure the AC drive
                                //
                                commandInfo.parameters = new object[] { acDriveConfig };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.StartACDrive:

                                //
                                // Get AC drive mode from request
                                //
                                string strACDriveMode = XmlUtilities.GetXmlValue(xmlRequestNode, Consts.STRXML_ReqACDriveMode, false);
                                RedLion.ACDriveModes acDriveMode = (RedLion.ACDriveModes)Enum.Parse(typeof(RedLion.ACDriveModes), strACDriveMode);

                                //
                                // Start the AC drive
                                //
                                commandInfo.parameters = new object[] { acDriveMode };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.StopACDrive:

                                //
                                // Get AC drive mode from request
                                //
                                strACDriveMode = XmlUtilities.GetXmlValue(xmlRequestNode, Consts.STRXML_ReqACDriveMode, false);
                                acDriveMode = (RedLion.ACDriveModes)Enum.Parse(typeof(RedLion.ACDriveModes), strACDriveMode);

                                //
                                // Stop the AC drive
                                //
                                commandInfo.parameters = new object[] { acDriveMode };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.TakeMeasurement:

                                //
                                // Take a measurement
                                //
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                if (commandInfo.success == true)
                                {
                                    //
                                    // Get measurement values from the result - ensure the order of the measurement values is the
                                    // same as in EquipmentEngine.cs
                                    //
                                    float voltageMut = (float)commandInfo.results[0];
                                    float currentMut = (float)commandInfo.results[1];
                                    float powerFactorMut = (float)commandInfo.results[2];
                                    float voltageVsd = (float)commandInfo.results[3];
                                    float currentVsd = (float)commandInfo.results[4];
                                    float powerFactorVsd = (float)commandInfo.results[5];
                                    int speed = (int)commandInfo.results[6];
                                    int torque = (int)commandInfo.results[7];

                                    //
                                    // Add the measurement values to the response
                                    //
                                    xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspVoltageMut);
                                    xmlElement.InnerText = voltageMut.ToString();
                                    xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                                    xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspCurrentMut);
                                    xmlElement.InnerText = currentMut.ToString();
                                    xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                                    xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspPowerFactorMut);
                                    xmlElement.InnerText = powerFactorMut.ToString();
                                    xmlResponseDocument.DocumentElement.AppendChild(xmlElement);

                                    xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspVoltageVsd);
                                    xmlElement.InnerText = voltageVsd.ToString();
                                    xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                                    xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspCurrentVsd);
                                    xmlElement.InnerText = currentVsd.ToString();
                                    xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                                    xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspPowerFactorVsd);
                                    xmlElement.InnerText = powerFactorVsd.ToString();
                                    xmlResponseDocument.DocumentElement.AppendChild(xmlElement);

                                    xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspSpeed);
                                    xmlElement.InnerText = speed.ToString();
                                    xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                                    xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspTorque);
                                    xmlElement.InnerText = torque.ToString();
                                    xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                                }
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
