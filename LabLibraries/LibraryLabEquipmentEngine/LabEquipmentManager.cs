using System;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Engine
{
    public class LabEquipmentManager
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "LabEquipmentManager";

        //
        // String constants
        //
        protected const string STR_NotInitialised = "Not Initialised!";

        //
        // String constants for log messages
        //
        protected const string STRLOG_Command = " Command: ";
        protected const string STRLOG_Result = " Result: ";

        //
        // String constants for error messages
        //
        private const string STRERR_managerLock = "managerLock";
        protected const string STRERR_UnknownCommand = "Unknown Command: ";
        protected const string STRERR_PowerdownMustBeSuspended = "Powerdown must be suspended to execute command: ";

        //
        // Local variables
        //
        private Logfile.LoggingLevels logLevel;
        protected Object managerLock;
        protected string rootFilePath;
        protected LabEquipmentEngine labEquipmentEngine;

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public LabEquipmentManager(string rootFilePath)
        {
            const string STRLOG_MethodName = "LabEquipmentManager";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            this.rootFilePath = rootFilePath;

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

            //
            // Create thread objects
            //
            this.managerLock = new Object();
            if (this.managerLock == null)
            {
                throw new ArgumentNullException(STRERR_managerLock);
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual void Create()
        {
            const string STRLOG_MethodName = "Create";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            //
            // Create an instance of lab equipment engine
            //
            this.labEquipmentEngine = new LabEquipmentEngine(this.rootFilePath);

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public bool Start()
        {
            bool success = false;

            if (this.labEquipmentEngine != null)
            {
                success = this.labEquipmentEngine.Start();
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetTimeUntilReady()
        {
            int timeUntilReady = -1;

            if (this.labEquipmentEngine != null)
            {
                timeUntilReady = this.labEquipmentEngine.GetTimeUntilReady();
            }

            return timeUntilReady;
        }

        //-------------------------------------------------------------------------------------------------//

        public LabStatus GetLabEquipmentStatus()
        {
            LabStatus labStatus = null;

            lock (this.managerLock)
            {
                if (this.labEquipmentEngine != null)
                {
                    labStatus = this.labEquipmentEngine.GetLabEquipmentStatus();
                }
                else
                {
                    labStatus = new LabStatus(false, STR_NotInitialised);
                }
            }

            return labStatus;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SuspendPowerdown()
        {
            bool success = false;

            lock (this.managerLock)
            {
                if (this.labEquipmentEngine != null)
                {
                    success = this.labEquipmentEngine.SuspendPowerdown();
                }
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool ResumePowerdown()
        {
            bool success = false;

            lock (this.managerLock)
            {
                if (this.labEquipmentEngine != null)
                {
                    success = this.labEquipmentEngine.ResumePowerdown();
                }
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        public void Close()
        {
            if (this.labEquipmentEngine != null)
            {
                this.labEquipmentEngine.Close();
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual string ExecuteXmlRequest(string xmlRequest)
        {
            string strXmlResponse = string.Empty;

            const string STRLOG_MethodName = "ExecuteXmlRequest";

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);

            try
            {
                bool success = true;
                string errorMessage = string.Empty;
                ExecuteCommandInfo executeCommandInfo = null;
                ExecuteCommandInfo executeResultInfo = null;

                //
                // Create the XML response
                //
                XmlDocument xmlResponseDocument = new XmlDocument();
                XmlElement xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_Response);
                xmlResponseDocument.AppendChild(xmlElement);

                //
                // Add success of command execution and update later
                //
                xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspSuccess);
                xmlElement.InnerText = success.ToString();
                xmlResponseDocument.DocumentElement.AppendChild(xmlElement);

                //
                // Parse XML request for the command
                //
                XmlDocument xmlRequestDocument = new XmlDocument();
                xmlRequestDocument.LoadXml(xmlRequest);
                XmlNode xmlRequestNode = XmlUtilities.GetXmlRootNode(xmlRequestDocument, Consts.STRXML_Request);
                string strCommand = XmlUtilities.GetXmlValue(xmlRequestNode, Consts.STRXML_Command, false);

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
                            // Unable to execute the command
                            //
                            success = false;
                            errorMessage = STRERR_PowerdownMustBeSuspended + executeCommand.ToString();
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
                        case NonExecuteCommands.GetTime:

                            //
                            // Add time to response
                            //
                            xmlElement = xmlResponseDocument.CreateElement("Time");
                            xmlElement.InnerText = DateTime.Now.ToShortTimeString();
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
                        executeCommandInfo = new ExecuteCommandInfo(executeCommand);

                        //
                        // Process the execute command
                        //
                        switch (executeCommand)
                        {
                            case ExecuteCommands.SetTime:

                                //
                                // Pretend to change the time
                                //
                                executeCommandInfo.parameters = new object[] { DateTime.Now };
                                executeCommandInfo.timeout = 20;
                                executeResultInfo = this.labEquipmentEngine.ExecuteCommand(executeCommandInfo);
                                if (executeResultInfo.success == true)
                                {
                                    DateTime dateTime = (DateTime)executeCommandInfo.results[0];
                                }
                                break;
                        }
                    }
                }

                //
                // Update success of command execution
                //
                XmlNode xmlResponseNode = XmlUtilities.GetXmlRootNode(xmlResponseDocument, Engine.Consts.STRXML_Response);
                XmlUtilities.SetXmlValue(xmlResponseNode, Engine.Consts.STRXML_RspSuccess, executeResultInfo.success.ToString(), false);
                if (success == false)
                {
                    //
                    // Create error response
                    //
                    xmlElement = xmlResponseDocument.CreateElement(Engine.Consts.STRXML_RspErrorMessage);
                    xmlElement.InnerText = executeResultInfo.errorMessage;
                    xmlResponseDocument.DocumentElement.AppendChild(xmlElement);
                }
                strXmlResponse = xmlResponseDocument.InnerXml;

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
