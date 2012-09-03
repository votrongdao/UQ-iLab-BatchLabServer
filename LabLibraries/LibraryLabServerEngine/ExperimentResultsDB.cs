using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using Library.Lab;

namespace Library.LabServerEngine
{
    public class ExperimentResults
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "ExperimentResults";

        //
        // String constants for SQL processing
        //
        private const string STRSQLCMD_StoreResults = "StoreResults";
        private const string STRSQLCMD_UpdateResultsNotified = "UpdateResultsNotified";
        private const string STRSQLCMD_RetrieveResults = "RetrieveResults";
        private const string STRSQLCMD_RetrieveResultsAllNotNotified = "RetrieveResultsAllNotNotified";
        private const string STRSQLCMD_RetrieveResultsAll = "RetrieveResultsAll";
        private const string STRSQLPRM_ExperimentId = "@ExperimentId";
        private const string STRSQLPRM_SbName = "@SbName";
        private const string STRSQLPRM_UserGroup = "@UserGroup";
        private const string STRSQLPRM_PriorityHint = "@PriorityHint";
        private const string STRSQLPRM_Status = "@Status";
        private const string STRSQLPRM_XmlExperimentResult = "@XmlExperimentResult";
        private const string STRSQLPRM_XmlResultExtension = "@XmlResultExtension";
        private const string STRSQLPRM_XmlBlobExtension = "@XmlBlobExtension";
        private const string STRSQLPRM_WarningMessages = "@WarningMessages";
        private const string STRSQLPRM_ErrorMessage = "@ErrorMessage";
        private const string STRSQL_ResultsId = "Id";
        private const string STRSQL_ExperimentId = "ExperimentId";
        private const string STRSQL_SbName = "SbName";
        private const string STRSQL_UserGroup = "UserGroup";
        private const string STRSQL_PriorityHint = "PriorityHint";
        private const string STRSQL_Status = "Status";
        private const string STRSQL_XmlExperimentResult = "XmlExperimentResult";
        private const string STRSQL_XmlResultExtension = "XmlResultExtension";
        private const string STRSQL_XmlBlobExtension = "XmlBlobExtension";
        private const string STRSQL_WarningMessages = "WarningMessages";
        private const string STRSQL_ErrorMessage = "ErrorMessage";

        //
        // XML experiment results template
        //
        private const string STRXMLDOC_XmlTemplate =
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
            "<experimentResults>\r\n" +
            "  <experimentResult>\r\n" +
            "    <experimentID />\r\n" +
            "    <sbName />\r\n" +
            "    <userGroup />\r\n" +
            "    <priorityHint />\r\n" +
            "    <statusCode />\r\n" +
            "    <xmlExperimentResult />\r\n" +
            "    <xmlResultExtension />\r\n" +
            "    <xmlBlobExtension />\r\n" +
            "    <warningMessages>\r\n" +
            "      <warningMessage />\r\n" +
            "    </warningMessages>\r\n" +
            "    <errorMessage />\r\n" +
            "  </experimentResult>\r\n" +
            "</experimentResults>\r\n";

        //
        // XML warning messages template
        //
        private const string STRXMLDOC_WarningMessagesTemplate =
            "<warningMessages>\r\n" +
            "  <warningMessage />\r\n" +
            "</warningMessages>\r\n";

        //
        // String constants for the XML elements
        //
        private const string STRXML_experimentResults = "experimentResults";
        private const string STRXML_experimentResult = "experimentResult";
        private const string STRXML_userGroup = "userGroup";
        private const string STRXML_priorityHint = "priorityHint";
        private const string STRXML_statusCode = "statusCode";
        private const string STRXML_xmlExperimentResult = "xmlExperimentResult";
        private const string STRXML_xmlResultExtension = "xmlResultExtension";
        private const string STRXML_xmlBlobExtension = "xmlBlobExtension";
        private const string STRXML_warningMessages = "warningMessages";
        private const string STRXML_warningMessage = "warningMessage";
        private const string STRXML_errorMessage = "errorMessage";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_experimentId = " experimentId: ";
        private const string STRLOG_sbName = " sbName: ";
        private const string STRLOG_statusCode = " statusCode: ";
        private const string STRLOG_success = "success: ";
        private const string STRLOG_count = "count: ";

        //
        // String constants for exception messages
        //
        private const string STRERR_resultsLock = "resultsLock";
        private const string STRERR_ExperimentInfoIsNull = "ExperimentInfo is null";
        private const string STRERR_ResultsInfoArrayIsNull = "ResultsInfo array is null";
        private const string STRERR_SqlException = "SqlException: ";
        private const string STRERR_Exception = "Exception: ";
        private const string STRERR_FailedToSaveResults = "Failed to save results!";

        //
        // Local variables
        //
        private SqlConnection sqlConnection;
        private Object resultsLock;

        #endregion

        //---------------------------------------------------------------------------------------//

        public ExperimentResults()
        {
            //
            // Get the SQL connection string from Application's configuration file
            //
            string sqlConnectionString = Utilities.GetAppSetting(Consts.STRCFG_SqlConnection);
            this.sqlConnection = new SqlConnection(sqlConnectionString);

            //
            // Create results lock
            //
            this.resultsLock = new Object();
            if (this.resultsLock == null)
            {
                throw new ArgumentNullException(STRERR_resultsLock);
            }
        }

        //---------------------------------------------------------------------------------------//

        public ResultReport Load(int experimentId, string sbName)
        {
            const string STRLOG_MethodName = "Load";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote;

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);


            //
            // Create a ResultReport to return (not nullable)
            //
            ResultReport resultReport = new ResultReport();
            string xmlWarningMessages = string.Empty;

            //
            // Catch all exceptions so that a valid result report can be returned
            //
            lock (this.resultsLock)
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_RetrieveResults, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_ExperimentId, experimentId));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_SbName, sbName));

                    try
                    {
                        this.sqlConnection.Open();

                        SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                        while (sqlDataReader.Read() == true)
                        {
                            object sdrObject = null;
                            if ((sdrObject = sqlDataReader[STRSQL_Status]) != System.DBNull.Value)
                            {
                                StatusCodes status = (StatusCodes)Enum.Parse(typeof(StatusCodes), (string)sdrObject);
                                resultReport.statusCode = (int)status;
                            }
                            if ((sdrObject = sqlDataReader[STRSQL_XmlExperimentResult]) != System.DBNull.Value)
                                resultReport.experimentResults = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_XmlResultExtension]) != System.DBNull.Value)
                                resultReport.xmlResultExtension = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_XmlBlobExtension]) != System.DBNull.Value)
                                resultReport.xmlBlobExtension = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_WarningMessages]) != System.DBNull.Value)
                                xmlWarningMessages = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_ErrorMessage]) != System.DBNull.Value)
                                resultReport.errorMessage = (string)sdrObject;
                        }
                        sqlDataReader.Close();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception(STRERR_SqlException + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(STRERR_Exception + ex.Message);
                    }
                    finally
                    {
                        this.sqlConnection.Close();
                    }

                    //
                    // Convert warning messages from XML format to string array
                    //
                    if (xmlWarningMessages != null && xmlWarningMessages.Length > 0)
                    {
                        try
                        {
                            XmlDocument xmlDocument = XmlUtilities.GetXmlDocument(xmlWarningMessages);
                            XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlDocument, STRXML_warningMessages);
                            resultReport.warningMessages = XmlUtilities.GetXmlValues(xmlRootNode, STRXML_warningMessage, true);
                        }
                        catch
                        {
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Place the exception message in the result report
                    resultReport.errorMessage = ex.Message;

                    Logfile.WriteError(ex.Message);
                }
            }

            logMessage = STRLOG_statusCode + ((StatusCodes)resultReport.statusCode).ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return resultReport;
        }

        //---------------------------------------------------------------------------------------//

        public bool Save(ExperimentInfo experimentInfo)
        {
            const string STRLOG_MethodName = "Save";

            string logMessage = null;

            if (experimentInfo != null)
            {
                logMessage = STRLOG_experimentId + experimentInfo.experimentId.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + experimentInfo.sbName + Logfile.STRLOG_Quote;
            }

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Catch all exceptions thrown and return false if an error occurred.
            //
            bool success = false;

            lock (this.resultsLock)
            {
                try
                {
                    //
                    // Check that the experiment info exists
                    //
                    if (experimentInfo == null)
                    {
                        throw new ArgumentNullException(STRERR_ExperimentInfoIsNull);
                    }

                    //
                    // Check for null strings and change to empty strings if necessary
                    //
                    string errorMessage = (experimentInfo.resultReport.errorMessage == null) ? string.Empty : experimentInfo.resultReport.errorMessage;
                    string xmlBlobExtension = (experimentInfo.resultReport.xmlBlobExtension == null) ? string.Empty : experimentInfo.resultReport.xmlBlobExtension;
                    string xmlResultExtension = (experimentInfo.resultReport.xmlResultExtension == null) ? string.Empty : experimentInfo.resultReport.xmlResultExtension;
                    string experimentResults = (experimentInfo.resultReport.experimentResults == null) ? string.Empty : experimentInfo.resultReport.experimentResults;
                    string userGroup = (experimentInfo.userGroup == null) ? string.Empty : experimentInfo.userGroup;

                    //
                    // Convert warning messages to XML format
                    //
                    string xmlWarningMessages = string.Empty;
                    if (experimentInfo.resultReport.warningMessages != null)
                    {
                        XmlDocument xmlDocument = XmlUtilities.GetXmlDocument(STRXMLDOC_WarningMessagesTemplate);
                        XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlDocument, STRXML_warningMessages);
                        XmlUtilities.SetXmlValues(xmlRootNode, STRXML_warningMessage, experimentInfo.resultReport.warningMessages, true);
                        xmlWarningMessages = xmlDocument.OuterXml;
                    }

                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_StoreResults, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_ExperimentId, experimentInfo.experimentId));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_SbName, experimentInfo.sbName));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_UserGroup, userGroup));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_PriorityHint, experimentInfo.priorityHint));
                    StatusCodes status = (StatusCodes)experimentInfo.resultReport.statusCode;
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Status, status.ToString()));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_XmlExperimentResult, experimentResults));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_XmlResultExtension, xmlResultExtension));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_XmlBlobExtension, xmlBlobExtension));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_WarningMessages, xmlWarningMessages));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_ErrorMessage, errorMessage));

                    try
                    {
                        this.sqlConnection.Open();

                        if (sqlCommand.ExecuteNonQuery() == 0)
                        {
                            throw new Exception(STRERR_FailedToSaveResults);
                        }
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception(STRERR_SqlException + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(STRERR_Exception + ex.Message);
                    }
                    finally
                    {
                        this.sqlConnection.Close();
                    }

                    // Information saved successfully
                    success = true;
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);
                }
            }

            logMessage = STRLOG_success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Cancel an experiment that is waiting on the queue.
        /// </summary>
        /// <param name="experimentId"></param>
        /// <param name="sbName"></param>
        /// <returns>True if experiment was successfully cancelled.</returns>
        public bool UpdateNotified(int experimentId, string sbName)
        {
            const string STRLOG_MethodName = "UpdateNotified";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote;

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Catch all exceptions thrown and return false if an error occurred.
            //
            bool success = false;

            lock (this.resultsLock)
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_UpdateResultsNotified, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_ExperimentId, experimentId));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_SbName, sbName));

                    try
                    {
                        this.sqlConnection.Open();

                        success = (sqlCommand.ExecuteNonQuery() != 0);
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception(STRERR_SqlException + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(STRERR_Exception + ex.Message);
                    }
                    finally
                    {
                        this.sqlConnection.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);
                }
            }

            logMessage = STRLOG_success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public ResultsIdInfo[] RetrieveAllNotNotified()
        {
            const string STRLOG_MethodName = "RetrieveAllNotNotified";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            List<ResultsIdInfo> resultsIdInfoList = new List<ResultsIdInfo>();

            lock (this.resultsLock)
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_RetrieveResultsAllNotNotified, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        this.sqlConnection.Open();

                        SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                        while (sqlDataReader.Read() == true)
                        {
                            ResultsIdInfo resultsIdInfo = new ResultsIdInfo();
                            string xmlWarningMessages = string.Empty;

                            object sdrObject = null;
                            if ((sdrObject = sqlDataReader[STRSQL_ResultsId]) != System.DBNull.Value)
                                resultsIdInfo.queueId = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_ExperimentId]) != System.DBNull.Value)
                                resultsIdInfo.experimentId = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_SbName]) != System.DBNull.Value)
                                resultsIdInfo.sbName = (string)sdrObject;

                            //
                            // Add the results info to the list
                            //
                            resultsIdInfoList.Add(resultsIdInfo);
                        }
                        sqlDataReader.Close();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception(STRERR_SqlException + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(STRERR_Exception + ex.Message);
                    }
                    finally
                    {
                        this.sqlConnection.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);
                }
            }

            string logMessage = STRLOG_count + resultsIdInfoList.Count.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return resultsIdInfoList.ToArray();
        }

        //---------------------------------------------------------------------------------------//

        public string RetrieveAllToXml()
        {
            const string STRLOG_MethodName = "RetrieveAllToXml";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            ResultsInfo[] resultsInfoArray = this.RetrieveAll();
            string xmlResults = ConvertToXml(resultsInfoArray);

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return xmlResults;
        }

        //=======================================================================================//

        private class ResultsInfo
        {
            public int experimentId;
            public string sbName;
            public string userGroup;
            public int priorityHint;
            public int statusCode;
            public string xmlExperimentResult;
            public string xmlResultExtension;
            public string xmlBlobExtension;
            public string[] warningMessages;
            public string errorMessage;
        }

        //---------------------------------------------------------------------------------------//

        private ResultsInfo[] RetrieveAll()
        {
            const string STRLOG_MethodName = "RetrieveAll";

            List<ResultsInfo> resultsInfoList = new List<ResultsInfo>();

            lock (this.resultsLock)
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_RetrieveResultsAll, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        this.sqlConnection.Open();

                        SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                        while (sqlDataReader.Read() == true)
                        {
                            ResultsInfo resultsInfo = new ResultsInfo();
                            string xmlWarningMessages = string.Empty;

                            object sdrObject = null;
                            if ((sdrObject = sqlDataReader[STRSQL_ExperimentId]) != System.DBNull.Value)
                                resultsInfo.experimentId = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_SbName]) != System.DBNull.Value)
                                resultsInfo.sbName = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_UserGroup]) != System.DBNull.Value)
                                resultsInfo.userGroup = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_PriorityHint]) != System.DBNull.Value)
                                resultsInfo.priorityHint = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_Status]) != System.DBNull.Value)
                            {
                                StatusCodes status = (StatusCodes)Enum.Parse(typeof(StatusCodes), (string)sdrObject);
                                resultsInfo.statusCode = (int)status;
                            }
                            if ((sdrObject = sqlDataReader[STRSQL_XmlExperimentResult]) != System.DBNull.Value)
                                resultsInfo.xmlExperimentResult = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_XmlResultExtension]) != System.DBNull.Value)
                                resultsInfo.xmlResultExtension = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_XmlBlobExtension]) != System.DBNull.Value)
                                resultsInfo.xmlBlobExtension = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_WarningMessages]) != System.DBNull.Value)
                                xmlWarningMessages = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_ErrorMessage]) != System.DBNull.Value)
                                resultsInfo.errorMessage = (string)sdrObject;

                            //
                            // Convert warning messages from XML format to string array
                            //
                            try
                            {
                                XmlDocument xmlDocument = XmlUtilities.GetXmlDocument(xmlWarningMessages);
                                XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlDocument, STRXML_warningMessages);
                                resultsInfo.warningMessages = XmlUtilities.GetXmlValues(xmlRootNode, STRXML_warningMessage, true);
                            }
                            catch
                            {
                            }

                            //
                            // Add the results info to the list
                            //
                            resultsInfoList.Add(resultsInfo);
                        }
                        sqlDataReader.Close();
                    }
                    catch (SqlException ex)
                    {
                        throw new Exception(STRERR_SqlException + ex.Message);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(STRERR_Exception + ex.Message);
                    }
                    finally
                    {
                        this.sqlConnection.Close();
                    }
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);
                }
            }

            string logMessage = STRLOG_count + resultsInfoList.Count.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return resultsInfoList.ToArray();
        }

        //---------------------------------------------------------------------------------------//

        private string ConvertToXml(ResultsInfo[] resultsInfoArray)
        {
            const string STRLOG_MethodName = "ConvertToXml";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Catch all exceptions thrown and return an empty string if an error occurred
            //
            XmlDocument xmlDocument = null;
            string xmlExperimentResults = string.Empty;
            try
            {
                //
                // Check that the experiment info array exists
                //
                if (resultsInfoArray == null)
                {
                    throw new ArgumentNullException(STRERR_ResultsInfoArrayIsNull);
                }

                //
                // Take the experiment info  and put into the XML document
                //
                for (int i = 0; i < resultsInfoArray.GetLength(0); i++)
                {
                    ResultsInfo resultsInfo = resultsInfoArray[i];

                    // Load experiment results XML template string
                    XmlDocument xmlTemplateDocument = XmlUtilities.GetXmlDocument(STRXMLDOC_XmlTemplate);

                    //
                    // Fill in the XML template with values from the experiment results information
                    //
                    XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlTemplateDocument, STRXML_experimentResults);
                    XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlRootNode, STRXML_experimentResult);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXML_experimentID, resultsInfo.experimentId);
                    XmlUtilities.SetXmlValue(xmlNode, Consts.STRXML_sbName, resultsInfo.sbName, false);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_userGroup, resultsInfo.userGroup, false);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_priorityHint, resultsInfo.priorityHint);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_statusCode, resultsInfo.statusCode);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_xmlExperimentResult, resultsInfo.xmlExperimentResult, true);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_xmlResultExtension, resultsInfo.xmlResultExtension, true);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_xmlBlobExtension, resultsInfo.xmlBlobExtension, true);
                    XmlNode xmlNodeTemp = XmlUtilities.GetXmlNode(xmlNode, STRXML_warningMessages);
                    XmlUtilities.SetXmlValues(xmlNodeTemp, STRXML_warningMessage, resultsInfo.warningMessages, true);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_errorMessage, resultsInfo.errorMessage, true);

                    if (xmlDocument == null)
                    {
                        xmlDocument = xmlTemplateDocument;
                    }
                    else
                    {
                        //
                        // Create an XML fragment from the XML template and append to the document
                        //
                        XmlDocumentFragment xmlFragment = xmlDocument.CreateDocumentFragment();
                        xmlFragment.InnerXml = xmlNode.OuterXml;
                        xmlDocument.DocumentElement.AppendChild(xmlFragment);
                    }
                }

                //
                // Check if there were any experiment statistics
                //
                if (xmlDocument == null)
                {
                    xmlDocument = XmlUtilities.GetXmlDocument(STRXMLDOC_XmlTemplate);
                    XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlDocument, STRXML_experimentResults);
                    XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlRootNode, STRXML_experimentResult);
                    xmlRootNode.RemoveChild(xmlNode);
                }

                //
                // Convert the XML document to a string
                //
                StringWriter sw = new StringWriter();
                XmlTextWriter xtw = new XmlTextWriter(sw);
                xtw.Formatting = Formatting.Indented;
                xmlDocument.WriteTo(xtw);
                xtw.Flush();
                xmlExperimentResults = sw.ToString();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return xmlExperimentResults;
        }

    }
}
