using System;
using System.Diagnostics;
using System.Threading;
using System.Xml;
using Library.Lab;
using Library.LabServerEngine.Drivers.Equipment;

namespace Library.LabServerEngine.Drivers.Setup
{
    public class DriverEquipmentGeneric
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "DriverEquipmentGeneric";

        //
        // Constants
        //
        private const int EXECUTION_TIME = 7;

        //
        // String constants for logfile messages
        //
        private const string STRLOG_online = " online: ";
        private const string STRLOG_labStatusMessage = " labStatusMessage: ";

        protected const string STRLOG_State = " State: ";
        protected const string STRLOG_ExecutionTime = " ExecutionTime: ";
        protected const string STRLOG_StatusCode = " StatusCode: ";

        //
        // String constants for error messages
        //
        protected const string STRERR_StateNotFound = "State not found!";
        protected const string STRERR_SuspendPowerdown = "Suspend powerdown failed!";
        protected const string STRERR_ResumePowerdown = "Resume powerdown failed!";

        //
        // Local variables available to a derived class
        //
        protected EquipmentService equipmentServiceProxy;
        protected LabConfiguration labConfiguration;
        protected CancelExperiment cancelExperiment;

        #endregion

        //---------------------------------------------------------------------------------------//

        public DriverEquipmentGeneric(EquipmentService equipmentServiceProxy, LabConfiguration labConfiguration)
            : this(equipmentServiceProxy, labConfiguration, null)
        {
        }

        //-------------------------------------------------------------------------------------------------//

        public DriverEquipmentGeneric(EquipmentService equipmentServiceProxy, LabConfiguration labConfiguration, CancelExperiment cancelExperiment)
        {
            this.equipmentServiceProxy = equipmentServiceProxy;
            this.labConfiguration = labConfiguration;
            this.cancelExperiment = cancelExperiment;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual int GetExecutionTime(ExperimentSpecification experimentSpecification)
        {
            const string STRLOG_MethodName = "GetExecutionTime";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            int executionTime = EXECUTION_TIME;

            string logMessage = STRLOG_ExecutionTime + executionTime.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return executionTime;
        }

        //-------------------------------------------------------------------------------------------------//

        public virtual ExperimentResultInfo Execute(ExperimentSpecification experimentSpecification)
        {
            const string STRLOG_MethodName = "Execute";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Determine how long it actually take to execute
            //
            DateTime startDateTime = DateTime.Now;

            //
            // Create an instance of the experiment result info ready to fill in
            //
            ExperimentResultInfo experimentResultInfo = new ExperimentResultInfo();

            try
            {
                // Determine time to finish execution
                DateTime dateTimeEnd = startDateTime + new TimeSpan(0, 0, EXECUTION_TIME);

                //
                // Delay for the full execution time, unless cancelled
                //
                while (DateTime.Now < dateTimeEnd)
                {
                    Trace.Write("E");
                    Thread.Sleep(1000);

                    //
                    // Check if the experiment is being cancelled
                    //
                    if (this.cancelExperiment != null &&
                        this.cancelExperiment.IsCancelled == true)
                    {
                        // Experiment is cancelled
                        experimentResultInfo.statusCode = StatusCodes.Cancelled;
                        break;
                    }
                }
                Trace.WriteLine("");

                //
                // Check if the experiment was cancelled
                //
                if (experimentResultInfo.statusCode != StatusCodes.Cancelled)
                {
                    // Successful execution
                    experimentResultInfo.statusCode = StatusCodes.Completed;
                }
            }
            catch (Exception ex)
            {
                experimentResultInfo.statusCode = StatusCodes.Failed;
                experimentResultInfo.errorMessage = ex.Message;
                Logfile.WriteError(ex.Message);
            }

            //
            // Calculate actual execution time
            //
            TimeSpan timeSpan = DateTime.Now - startDateTime;
            int executionTime = (int)timeSpan.TotalSeconds;

            string logMessage = STRLOG_StatusCode + experimentResultInfo.statusCode
                + Logfile.STRLOG_Spacer + STRLOG_ExecutionTime + executionTime.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return experimentResultInfo;
        }

        //-------------------------------------------------------------------------------------------------//

        protected XmlDocument CreateXmlRequestDocument(string command)
        {
            return CreateXmlRequestDocument(command, null);
        }

        //-------------------------------------------------------------------------------------------------//

        protected XmlDocument CreateXmlRequestDocument(string command, string[,] args)
        {
            XmlDocument xmlDocument = null;

            try
            {
                xmlDocument = new XmlDocument();
                XmlElement xmlElement = xmlDocument.CreateElement(LabServerEngine.Consts.STRXML_Request);
                xmlDocument.AppendChild(xmlElement);

                //
                // Add command
                //
                xmlElement = xmlDocument.CreateElement(LabServerEngine.Consts.STRXML_Command);
                xmlElement.InnerText = command;
                xmlDocument.DocumentElement.AppendChild(xmlElement);

                //
                // Add arguments which could be XML strings
                //
                if (args != null)
                {
                    for (int i = 0; i < args.GetLength(0); i++)
                    {
                        xmlElement = xmlDocument.CreateElement(args[i, 0]);
                        xmlElement.InnerXml = args[i, 1];
                        xmlDocument.DocumentElement.AppendChild(xmlElement);
                    }
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return xmlDocument;
        }

        //-------------------------------------------------------------------------------------------------//

        protected XmlNode CreateXmlResponseNode(string xmlResponse)
        {
            XmlDocument xmlDocument = null;
            XmlNode xmlResponseNode = null;

            try
            {
                xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlResponse);
                xmlResponseNode = XmlUtilities.GetXmlRootNode(xmlDocument, LabServerEngine.Consts.STRXML_Response);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return xmlResponseNode.Clone();
        }

    }
}
