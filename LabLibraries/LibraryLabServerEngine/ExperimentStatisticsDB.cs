using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using Library.Lab;

namespace Library.LabServerEngine
{
    public class ExperimentStatistics
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "ExperimentStatistics";

        //
        // String constants for SQL processing
        //
        private const string STRSQLCMD_StoreStatisticsSubmitted = "StoreStatisticsSubmitted";
        private const string STRSQLCMD_UpdateStatisticsStarted = "UpdateStatisticsStarted";
        private const string STRSQLCMD_UpdateStatisticsCompleted = "UpdateStatisticsCompleted";
        private const string STRSQLCMD_UpdateStatisticsCancelled = "UpdateStatisticsCancelled";
        private const string STRSQLCMD_RetrieveAllStatistics = "RetrieveAllStatistics";
        private const string STRSQLPRM_ExperimentId = "@ExperimentId";
        private const string STRSQLPRM_SbName = "@SbName";
        private const string STRSQLPRM_UserGroup = "@UserGroup";
        private const string STRSQLPRM_PriorityHint = "@PriorityHint";
        private const string STRSQLPRM_EstimatedExecTime = "@EstimatedExecTime";
        private const string STRSQLPRM_TimeSubmitted = "@TimeSubmitted";
        private const string STRSQLPRM_QueueLength = "@QueueLength";
        private const string STRSQLPRM_EstimatedWaitTime = "@EstimatedWaitTime";
        private const string STRSQLPRM_UnitId = "@UnitId";
        private const string STRSQLPRM_TimeStarted = "@TimeStarted";
        private const string STRSQLPRM_TimeCompleted = "@TimeCompleted";
        private const string STRSQL_ExperimentId = "ExperimentId";
        private const string STRSQL_SbName = "SbName";
        private const string STRSQL_UserGroup = "UserGroup";
        private const string STRSQL_PriorityHint = "PriorityHint";
        private const string STRSQL_EstimatedExecTime = "EstimatedExecTime";
        private const string STRSQL_TimeSubmitted = "TimeSubmitted";
        private const string STRSQL_QueueLength = "QueueLength";
        private const string STRSQL_EstimatedWaitTime = "EstimatedWaitTime";
        private const string STRSQL_TimeStarted = "TimeStarted";
        private const string STRSQL_UnitId = "UnitId";
        private const string STRSQL_TimeCompleted = "TimeCompleted";
        private const string STRSQL_Cancelled = "Cancelled";

        //
        // XML statistics template
        //
        private const string STRXMLDOC_XmlTemplate =
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
            "<statistics>\r\n" +
            "  <experiment>\r\n" +
            "    <experimentId />\r\n" +
            "    <sbName />\r\n" +
            "    <userGroup />\r\n" +
            "    <priorityHint />\r\n" +
            "    <estimatedExecTime />\r\n" +
            "    <timeSubmitted />\r\n" +
            "    <queueLength />\r\n" +
            "    <estimatedWaitTime />\r\n" +
            "    <timeStarted />\r\n" +
            "    <unitId />\r\n" +
            "    <timeCompleted />\r\n" +
            "    <actualExecTime />\r\n" +
            "    <cancelled />\r\n" +
            "  </experiment>\r\n" +
            "</statistics>\r\n";

        //
        // String constants for XML elements
        //
        private const string STRXML_statistics = "statistics";
        private const string STRXML_experiment = "experiment";
        private const string STRXML_experimentId = "experimentId";
        private const string STRXML_sbName = "sbName";
        private const string STRXML_userGroup = "userGroup";
        private const string STRXML_priorityHint = "priorityHint";
        private const string STRXML_estimatedExecTime = "estimatedExecTime";
        private const string STRXML_timeSubmitted = "timeSubmitted";
        private const string STRXML_queueLength = "queueLength";
        private const string STRXML_estimatedWaitTime = "estimatedWaitTime";
        private const string STRXML_timeStarted = "timeStarted";
        private const string STRXML_unitId = "unitId";
        private const string STRXML_timeCompleted = "timeCompleted";
        private const string STRXML_actualExecTime = "actualExecTime";
        private const string STRXML_cancelled = "cancelled";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_experimentId = "experimentId: ";
        private const string STRLOG_sbName = "sbName: ";
        private const string STRLOG_unitId = "unitId: ";
        private const string STRLOG_success = "success: ";
        private const string STRLOG_count = "count: ";

        //
        // String constants for exception messages
        //
        private const string STRERR_statisticsLock = "statisticsLock";
        private const string STRERR_QueuedExperimentInfoIsNull = "QueuedExperimentInfo is null";
        private const string STRERR_StatisticsInfoArrayIsNull = "StatisticsInfo array is null";
        private const string STRERR_ExperimentNotFound = "Experiment not found!";
        private const string STRERR_SqlException = "SqlException: ";
        private const string STRERR_Exception = "Exception: ";
        private const string STRERR_FailedToSubmitStatistics = "Failed to submit statistics!";

        //
        // Local variables
        //
        private SqlConnection sqlConnection;
        private Object statisticsLock;

        #endregion

        //---------------------------------------------------------------------------------------//

        public ExperimentStatistics()
        {
            //
            // Get the SQL connection string from Application's configuration file
            //
            string sqlConnectionString = Utilities.GetAppSetting(Consts.STRCFG_SqlConnection);
            this.sqlConnection = new SqlConnection(sqlConnectionString);

            //
            // Create statistics lock
            //
            this.statisticsLock = new Object();
            if (this.statisticsLock == null)
            {
                throw new ArgumentNullException(STRERR_statisticsLock);
            }
        }

        //---------------------------------------------------------------------------------------//

        public bool Submitted(QueuedExperimentInfo queuedExperimentInfo, DateTime timeSubmitted)
        {
            const string STRLOG_MethodName = "Submitted";

            string logMessage = null;
            if (queuedExperimentInfo != null)
            {
                logMessage = STRLOG_experimentId + queuedExperimentInfo.experimentId.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + queuedExperimentInfo.sbName + Logfile.STRLOG_Quote;
            }

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Catch all exceptions thrown and return false if an error occurred.
            //
            bool success = false;

            lock (this.statisticsLock)
            {
                try
                {
                    //
                    // Check that the queued experiment info exists
                    //
                    if (queuedExperimentInfo == null)
                    {
                        throw new ArgumentNullException(STRERR_QueuedExperimentInfoIsNull);
                    }

                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_StoreStatisticsSubmitted, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_ExperimentId, queuedExperimentInfo.experimentId));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_SbName, queuedExperimentInfo.sbName));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_UserGroup, queuedExperimentInfo.userGroup));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_PriorityHint, queuedExperimentInfo.priorityHint));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_EstimatedExecTime, queuedExperimentInfo.estExecutionTime));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_TimeSubmitted, timeSubmitted));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_QueueLength, queuedExperimentInfo.position - 1));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_EstimatedWaitTime, queuedExperimentInfo.waitTime));

                    try
                    {
                        this.sqlConnection.Open();

                        if (sqlCommand.ExecuteNonQuery() == 0)
                        {
                            throw new Exception(STRERR_FailedToSubmitStatistics);
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

        public bool Started(int experimentId, string sbName, int unitId, DateTime timeStarted)
        {
            const string STRLOG_MethodName = "Started";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote +
                Logfile.STRLOG_Spacer + STRLOG_unitId + unitId.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Catch all exceptions thrown and return false if an error occurred.
            //
            bool success = false;

            lock (this.statisticsLock)
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_UpdateStatisticsStarted, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_ExperimentId, experimentId));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_SbName, sbName));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_UnitId, unitId));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_TimeStarted, timeStarted));

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

        public bool Completed(int experimentId, string sbName, DateTime timeCompleted)
        {
            const string STRLOG_MethodName = "Completed";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote;

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Catch all exceptions thrown and return false if an error occurred.
            //
            bool success = false;

            lock (this.statisticsLock)
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_UpdateStatisticsCompleted, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_ExperimentId, experimentId));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_SbName, sbName));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_TimeCompleted, timeCompleted));

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

        public bool Cancelled(int experimentId, string sbName, DateTime timeCancelled)
        {
            const string STRLOG_MethodName = "Cancelled";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote;

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Catch all exceptions thrown and return false if an error occurred.
            //
            bool success = false;

            lock (this.statisticsLock)
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_UpdateStatisticsCancelled, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_ExperimentId, experimentId));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_SbName, sbName));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQL_TimeCompleted, timeCancelled));

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

        private class StatisticsInfo
        {
            public int experimentId;
            public string sbName;
            public string userGroup;
            public int priorityHint;
            public int estimatedExecTime;
            public DateTime timeSubmitted;
            public int queueLength;
            public int estimatedWaitTime;
            public DateTime timeStarted;
            public int unitId;
            public DateTime timeCompleted;
            public int actualExecTime;
            public bool cancelled;
        }

        //---------------------------------------------------------------------------------------//

        public string RetrieveAllToXml()
        {
            const string STRLOG_MethodName = "RetrieveAllToXml";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            StatisticsInfo[] statisticsInfoArray = this.RetrieveAll();
            string xmlStatistics = ConvertToXml(statisticsInfoArray);

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return xmlStatistics;
        }
        
        //---------------------------------------------------------------------------------------//

        private StatisticsInfo[] RetrieveAll()
        {
            const string STRLOG_MethodName = "RetrieveAll";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            List<StatisticsInfo> statisticsInfoList = new List<StatisticsInfo>();

            lock (this.statisticsLock)
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_RetrieveAllStatistics, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    try
                    {
                        this.sqlConnection.Open();

                        SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                        while (sqlDataReader.Read() == true)
                        {
                            StatisticsInfo statisticsInfo = new StatisticsInfo();

                            //
                            // Put the data into the statistics info object
                            //
                            object sdrObject = null;
                            if ((sdrObject = sqlDataReader[STRSQL_ExperimentId]) != System.DBNull.Value)
                                statisticsInfo.experimentId = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_SbName]) != System.DBNull.Value)
                                statisticsInfo.sbName = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_UserGroup]) != System.DBNull.Value)
                                statisticsInfo.userGroup = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_PriorityHint]) != System.DBNull.Value)
                                statisticsInfo.priorityHint = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_EstimatedExecTime]) != System.DBNull.Value)
                                statisticsInfo.estimatedExecTime = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_TimeSubmitted]) != System.DBNull.Value)
                                statisticsInfo.timeSubmitted = (DateTime)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_QueueLength]) != System.DBNull.Value)
                                statisticsInfo.queueLength = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_EstimatedWaitTime]) != System.DBNull.Value)
                                statisticsInfo.estimatedWaitTime = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_TimeStarted]) != System.DBNull.Value)
                                statisticsInfo.timeStarted = (DateTime)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_UnitId]) != System.DBNull.Value)
                                statisticsInfo.unitId = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_TimeCompleted]) != System.DBNull.Value)
                                statisticsInfo.timeCompleted = (DateTime)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_Cancelled]) != System.DBNull.Value)
                                statisticsInfo.cancelled = (bool)sdrObject;

                            //
                            // Calculate the actual execution time
                            //
                            if (statisticsInfo.timeStarted != DateTime.MinValue)
                            {
                                TimeSpan timeSpan = statisticsInfo.timeCompleted - statisticsInfo.timeStarted;
                                statisticsInfo.actualExecTime = (int)timeSpan.TotalSeconds;
                            }

                            //
                            // Add the statistics info to the list
                            //
                            statisticsInfoList.Add(statisticsInfo);
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

            string logMessage = STRLOG_count + statisticsInfoList.Count.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return statisticsInfoList.ToArray();
        }

        //---------------------------------------------------------------------------------------//

        private string ConvertToXml(StatisticsInfo[] statisticsInfoArray)
        {
            const string STRLOG_MethodName = "ConvertToXml";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Catch all exceptions thrown and return an empty string if an error occurred
            //
            XmlDocument xmlDocument = null;
            string xmlExperimentStatistics = string.Empty;
            try
            {
                //
                // Check that the experiment info array exists
                //
                if (statisticsInfoArray == null)
                {
                    throw new ArgumentNullException(STRERR_StatisticsInfoArrayIsNull);
                }

                //
                // Take the experiment info and put into the XML document
                //
                for (int i = 0; i < statisticsInfoArray.GetLength(0); i++)
                {
                    StatisticsInfo statisticsInfo = statisticsInfoArray[i];

                    // Load experiment statistics XML template string
                    XmlDocument xmlTemplateDocument = XmlUtilities.GetXmlDocument(STRXMLDOC_XmlTemplate);

                    //
                    // Fill in the XML template with values from the experiment statistics information
                    //
                    XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlTemplateDocument, STRXML_statistics);
                    XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlRootNode, STRXML_experiment);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_experimentId, statisticsInfo.experimentId);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_sbName, statisticsInfo.sbName, false);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_userGroup, statisticsInfo.userGroup, false);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_priorityHint, statisticsInfo.priorityHint);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_estimatedExecTime, statisticsInfo.estimatedExecTime);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_timeSubmitted, statisticsInfo.timeSubmitted.ToString(), false);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_queueLength, statisticsInfo.queueLength);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_estimatedWaitTime, statisticsInfo.estimatedWaitTime);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_cancelled, statisticsInfo.cancelled);

                    //
                    // Check if experiment has started before filling in these
                    //
                    if (statisticsInfo.timeStarted > DateTime.MinValue)
                    {
                        XmlUtilities.SetXmlValue(xmlNode, STRXML_timeStarted, statisticsInfo.timeStarted.ToString(), false);
                        XmlUtilities.SetXmlValue(xmlNode, STRXML_unitId, statisticsInfo.unitId);
                        if (statisticsInfo.timeCompleted > DateTime.MinValue)
                        {
                            //
                            // Check if experiment has completed/cancelled before filling in these
                            //
                            XmlUtilities.SetXmlValue(xmlNode, STRXML_actualExecTime, statisticsInfo.actualExecTime);
                            XmlUtilities.SetXmlValue(xmlNode, STRXML_timeCompleted, statisticsInfo.timeCompleted.ToString(), false);
                        }
                    }

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
                    //
                    // Remove the template content
                    //
                    xmlDocument = XmlUtilities.GetXmlDocument(STRXMLDOC_XmlTemplate);
                    XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlDocument, STRXML_statistics);
                    XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlRootNode, STRXML_experiment);
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
                xmlExperimentStatistics = sw.ToString();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return xmlExperimentStatistics;
        }

    }
}
