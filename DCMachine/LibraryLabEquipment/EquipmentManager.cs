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

        //
        // Local variables
        //
        private Logfile.LoggingLevels logLevel;

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public EquipmentManager(string rootFilePath)
            : base(rootFilePath)
        {
            //
            // Determine the logging level for this class
            //
            try
            {
                this.logLevel = (Logfile.LoggingLevels)Utilities.GetIntAppSetting(STRLOG_ClassName);
            }
            catch
            {
                this.logLevel = Logfile.LoggingLevels.Minimum;
            }
            Logfile.Write(Logfile.STRLOG_LogLevel + this.logLevel.ToString());
        }

        //-------------------------------------------------------------------------------------------------//

        public override void Create()
        {
            const string STRLOG_MethodName = "Create";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            //
            // Create an instance of lab equipment engine
            //
            this.labEquipmentEngine = new EquipmentEngine(this.rootFilePath);

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public override string ExecuteXmlRequest(string xmlRequest)
        {
            string strXmlResponse = string.Empty;

            const string STRLOG_MethodName = "ExecuteXmlRequest";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

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
                    Logfile.Write(this.logLevel, STRLOG_Command + nonExecuteCommand.ToString());
                }
                catch
                {
                    try
                    {
                        //
                        // Try to convert to an execute command type that does require powerdown to be suspended
                        //
                        executeCommand = (ExecuteCommands)Enum.Parse(typeof(ExecuteCommands), strCommand);
                        Logfile.Write(this.logLevel, STRLOG_Command + executeCommand.ToString());

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
                            // Get the time to start the AC drive
                            //
                            int startACDriveTime = equipmentEngine.GetStartACDriveTime();

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspStartACDriveTime);
                            xmlElement.InnerText = startACDriveTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetStopACDriveTime:

                            //
                            // Get the time to stop the AC drive
                            //
                            int stopACDriveTime = equipmentEngine.GetStopACDriveTime();

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspStopACDriveTime);
                            xmlElement.InnerText = stopACDriveTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetResetDCDriveMutTime:

                            //
                            // Get the time to reset the DC drive
                            //
                            int resetDCDriveMutTime = equipmentEngine.GetResetDCDriveMutTime();

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspResetDCDriveMutTime);
                            xmlElement.InnerText = resetDCDriveMutTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetConfigureDCDriveMutTime:

                            //
                            // Get the time to configure the DC drive
                            //
                            int configureDCDriveMutTime = equipmentEngine.GetConfigureDCDriveMutTime();

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspConfigureDCDriveMutTime);
                            xmlElement.InnerText = configureDCDriveMutTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetStartDCDriveMutTime:

                            //
                            // Get DC drive mode from request
                            //
                            string strDCDriveMutMode = XmlUtilities.GetXmlValue(xmlRequestNode, Consts.STRXML_ReqDCDriveMutMode, false);
                            RedLion.DCDriveMutModes dcDriveMutMode = (RedLion.DCDriveMutModes)Enum.Parse(typeof(RedLion.DCDriveMutModes), strDCDriveMutMode);

                            //
                            // Get the time to start the DC drive for the specified mode
                            //
                            int startDCDriveMutTime = equipmentEngine.GetStartDCDriveMutTime(dcDriveMutMode);

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspStartDCDriveMutTime);
                            xmlElement.InnerText = startDCDriveMutTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetStopDCDriveMutTime:

                            //
                            // Get the time to stop the DC drive
                            //
                            int stopDCDriveMutTime = equipmentEngine.GetStopDCDriveMutTime();

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspStopDCDriveMutTime);
                            xmlElement.InnerText = stopDCDriveMutTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetSetSpeedACDriveTime:

                            //
                            // Get the time to set the speed of the AC drive
                            //
                            int setSpeedACDriveTime = equipmentEngine.GetSetSpeedACDriveTime();

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspSetSpeedACDriveTime);
                            xmlElement.InnerText = setSpeedACDriveTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetSetSpeedDCDriveMutTime:

                            //
                            // Get the time to set the speed of the DC drive
                            //
                            int setSpeedDCDriveMutTime = equipmentEngine.GetSetSpeedDCDriveMutTime();

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspSetSpeedDCDriveMutTime);
                            xmlElement.InnerText = setSpeedDCDriveMutTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetSetTorqueDCDriveMutTime:

                            //
                            // Get the time to set the torque of the DC drive
                            //
                            int setTorqueDCDriveMutTime = equipmentEngine.GetSetTorqueDCDriveMutTime();

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspSetTorqueDCDriveMutTime);
                            xmlElement.InnerText = setTorqueDCDriveMutTime.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetSetFieldDCDriveMutTime:

                            //
                            // Get the time to set the field of the DC drive
                            //
                            int setFieldDCDriveMutTime = equipmentEngine.GetSetFieldDCDriveMutTime();

                            //
                            // Add the time to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspSetFieldDCDriveMutTime);
                            xmlElement.InnerText = setFieldDCDriveMutTime.ToString();
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

                        case NonExecuteCommands.GetACDriveInfo:

                            //
                            // Get the info about the AC drive
                            //
                            RedLion.ACDriveInfo aCDriveInfo = equipmentEngine.GetACDriveInfo();

                            //
                            // Add the info to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspMinSpeed);
                            xmlElement.InnerText = aCDriveInfo.minSpeed.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspMaxSpeed);
                            xmlElement.InnerText = aCDriveInfo.maxSpeed.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            break;

                        case NonExecuteCommands.GetDCDriveMutInfo:

                            //
                            // Get the info about the AC drive
                            //
                            RedLion.DCDriveMutInfo dCDriveMutInfo = equipmentEngine.GetDCDriveMutInfo();

                            //
                            // Add the info to the response
                            //
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspMinSpeed);
                            xmlElement.InnerText = dCDriveMutInfo.minSpeed.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspMaxSpeed);
                            xmlElement.InnerText = dCDriveMutInfo.maxSpeed.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspDefaultField);
                            xmlElement.InnerText = dCDriveMutInfo.defaultField.ToString();
                            xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                            xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspDefaultTorque);
                            xmlElement.InnerText = dCDriveMutInfo.defaultTorque.ToString();
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
                                RedLion.ACDriveConfigs aCDriveConfig = (RedLion.ACDriveConfigs)Enum.Parse(typeof(RedLion.ACDriveConfigs), strACDriveConfig);

                                //
                                // Configure the AC drive with the specified configuration
                                //
                                commandInfo.parameters = new object[] { aCDriveConfig };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.StartACDrive:

                                //
                                // Start the AC drive
                                //
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.StopACDrive:

                                //
                                // Stop the AC drive
                                //
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.ResetDCDriveMut:

                                //
                                // Reset the DC drive
                                //
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.ConfigureDCDriveMut:

                                //
                                // Get DC drive configuration from request
                                //
                                string strDCDriveMutConfig = XmlUtilities.GetXmlValue(xmlRequestNode, Consts.STRXML_ReqDCDriveMutConfig, false);
                                RedLion.DCDriveMutConfigs dcDriveMutConfig = (RedLion.DCDriveMutConfigs)Enum.Parse(typeof(RedLion.DCDriveMutConfigs), strDCDriveMutConfig);

                                //
                                // Configure the AC drive with the specified configuration
                                //
                                commandInfo.parameters = new object[] { dcDriveMutConfig };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.StartDCDriveMut:

                                //
                                // Get DC drive mode from request
                                //
                                string strDCDriveMutMode = XmlUtilities.GetXmlValue(xmlRequestNode, Consts.STRXML_ReqDCDriveMutMode, false);
                                RedLion.DCDriveMutModes dcDriveMutMode = (RedLion.DCDriveMutModes)Enum.Parse(typeof(RedLion.DCDriveMutModes), strDCDriveMutMode);

                                //
                                // Start the DC drive
                                //
                                commandInfo.parameters = new object[] { dcDriveMutMode };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.StopDCDriveMut:

                                //
                                // Stop the DC drive
                                //
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.SetSpeedACDrive:

                                //
                                // Get speed from request
                                //
                                int speedACDrive = XmlUtilities.GetIntValue(xmlRequestNode, Consts.STRXML_ReqSpeedACDrive, 0);

                                //
                                // Set the speed of the AC drive
                                //
                                commandInfo.parameters = new object[] { speedACDrive };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.SetSpeedDCDriveMut:

                                //
                                // Get speed from request
                                //
                                int speedDCDriveMut = XmlUtilities.GetIntValue(xmlRequestNode, Consts.STRXML_ReqSpeedDCDriveMut, 0);

                                //
                                // Set the speed of the DC drive
                                //
                                commandInfo.parameters = new object[] { speedDCDriveMut };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.SetTorqueDCDriveMut:

                                //
                                // Get torque from request
                                //
                                int torqueDCDriveMut = XmlUtilities.GetIntValue(xmlRequestNode, Consts.STRXML_ReqTorqueDCDriveMut, 0);

                                //
                                // Set the torque of the DC drive
                                //
                                commandInfo.parameters = new object[] { torqueDCDriveMut };
                                resultInfo = (CommandInfo)equipmentEngine.ExecuteCommand((ExecuteCommandInfo)commandInfo);
                                break;

                            case ExecuteCommands.SetFieldDCDriveMut:

                                //
                                // Get torque from request
                                //
                                int fieldDCDriveMut = XmlUtilities.GetIntValue(xmlRequestNode, Consts.STRXML_ReqFieldDCDriveMut, 100);

                                //
                                // Set the field of the DC drive
                                //
                                commandInfo.parameters = new object[] { fieldDCDriveMut };
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
                                    // Get measurement values from the result
                                    //
                                    int speed = (int)commandInfo.results[0];
                                    int voltage = (int)commandInfo.results[1];
                                    float fieldCurrent = (float)commandInfo.results[2];
                                    int load = (int)commandInfo.results[3];

                                    //
                                    // Add the measurement values to the response
                                    //
                                    xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspSpeed);
                                    xmlElement.InnerText = speed.ToString();
                                    xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                                    xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspVoltage);
                                    xmlElement.InnerText = voltage.ToString();
                                    xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                                    xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspFieldCurrent);
                                    xmlElement.InnerText = fieldCurrent.ToString();
                                    xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                                    xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspLoad);
                                    xmlElement.InnerText = load.ToString();
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

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            return strXmlResponse;
        }

    }
}
