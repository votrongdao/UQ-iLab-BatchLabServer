using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Xml;
using Library.Lab;

namespace Library.LabServerEngine
{
    public class ExperimentQueueDB
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "ExperimentQueueDB";

        //
        // String constants for SQL processing
        //
        private const string STRSQLCMD_StoreQueue = "StoreQueue";
        private const string STRSQLCMD_UpdateQueueStatusToRunning = "UpdateQueueStatusToRunning";
        private const string STRSQLCMD_UpdateQueueStatus = "UpdateQueueStatus";
        private const string STRSQLCMD_UpdateQueueCancel = "UpdateQueueCancel";
        private const string STRSQLCMD_RetrieveQueue = "RetrieveQueue";
        private const string STRSQLCMD_RetrieveQueueAllWithStatus = "RetrieveQueueAllWithStatus";
        private const string STRSQLCMD_RetrieveQueueCountWithStatus = "RetrieveQueueCountWithStatus";

        private const string STRSQLPRM_ExperimentId = "@ExperimentId";
        private const string STRSQLPRM_SbName = "@SbName";
        private const string STRSQLPRM_UserGroup = "@UserGroup";
        private const string STRSQLPRM_PriorityHint = "@PriorityHint";
        private const string STRSQLPRM_XmlSpecification = "@XmlSpecification";
        private const string STRSQLPRM_EstimatedExecTime = "@EstimatedExecTime";
        private const string STRSQLPRM_Status = "@Status";
        private const string STRSQLPRM_UnitId = "@UnitId";

        private const string STRSQL_QueueId = "Id";
        private const string STRSQL_ExperimentId = "ExperimentId";
        private const string STRSQL_SbName = "SbName";
        private const string STRSQL_UserGroup = "UserGroup";
        private const string STRSQL_PriorityHint = "PriorityHint";
        private const string STRSQL_XmlSpecification = "XmlSpecification";
        private const string STRSQL_EstimatedExecTime = "EstimatedExecTime";
        private const string STRSQL_Status = "Status";
        private const string STRSQL_UnitId = "UnitId";
        private const string STRSQL_Cancelled = "Cancelled";

        //
        // XML experiment queue template
        //
        private const string STRXMLDOC_XmlTemplate =
            "<?xml version=\"1.0\" encoding=\"utf-8\" ?>\r\n" +
            "<experimentQueue>\r\n" +
            "  <experiment>\r\n" +
            "    <experimentId />\r\n" +
            "    <sbName />\r\n" +
            "    <userGroup />\r\n" +
            "    <priorityHint />\r\n" +
            "    <specification />\r\n" +
            "    <estExecutionTime />\r\n" +
            "    <cancelled />\r\n" +
            "  </experiment>\r\n" +
            "</experimentQueue>\r\n";

        //
        // String constants for the XML experiment queue template
        //
        private const string STRXML_experimentQueue = "experimentQueue";
        private const string STRXML_experiment = "experiment";
        private const string STRXML_experimentId = "experimentId";
        private const string STRXML_sbName = "sbName";
        private const string STRXML_userGroup = "userGroup";
        private const string STRXML_priorityHint = "priorityHint";
        private const string STRXML_specification = "specification";
        private const string STRXML_estExecutionTime = "estExecutionTime";
        private const string STRXML_cancelled = "cancelled";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_experimentId = " experimentId: ";
        private const string STRLOG_sbName = " sbName: ";
        private const string STRLOG_statusCode = " statusCode: ";
        private const string STRLOG_status = " status: ";
        private const string STRLOG_unitId = " unitId: ";
        private const string STRLOG_success = " success: ";
        private const string STRLOG_count = "count: ";
        private const string STRLOG_position = " position: ";
        private const string STRLOG_queueLength = " queueLength: ";
        private const string STRLOG_waitTime = " waitTime: ";
        private const string STRLOG_estExecutionTime = " estExecutionTime: ";
        private const string STRLOG_seconds = " seconds";
        private const string STRLOG_NoExperimentsWaiting = "No experiments waiting.";
        //
        // String constants for exception messages
        //
        private const string STRERR_queueLock = "queueLock";
        private const string STRERR_ExperimentInfoIsNull = "ExperimentInfo is null";
        private const string STRERR_ExperimentNotFound = "Experiment not found!";
        private const string STRERR_SqlException = "SqlException: ";
        private const string STRERR_Exception = "Exception: ";
        private const string STRERR_FailedToEnqueueExperiment = "Failed to enqueue experiment!";
        private const string STRERR_ExperimentInfoArrayIsNull = "ExperimentInfo array is null";

        //
        // Local variables
        //
        private SqlConnection sqlConnection;
        private Object queueLock;

        #endregion

        //---------------------------------------------------------------------------------------//

        public ExperimentQueueDB()
        {
            //
            // Get the SQL connection string from Application's configuration file
            //
            string sqlConnectionString = Utilities.GetAppSetting(Consts.STRCFG_SqlConnection);
            this.sqlConnection = new SqlConnection(sqlConnectionString);

            //
            // Create queue lock
            //
            this.queueLock = new Object();
            if (this.queueLock == null)
            {
                throw new ArgumentNullException(STRERR_queueLock);
            }
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Add an experiment to the end of the queue. Return queue information about the experiment.
        /// </summary>
        /// <param name="experimentInfo"></param>
        /// <returns>Queue information about the experiment.</returns>
        public QueuedExperimentInfo Enqueue(ExperimentInfo experimentInfo)
        {
            const string STRLOG_MethodName = "Enqueue";

            string logMessage = null;
            if (experimentInfo != null)
            {
                logMessage = STRLOG_experimentId + experimentInfo.experimentId.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + experimentInfo.sbName + Logfile.STRLOG_Quote;
            }

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            QueuedExperimentInfo queuedExperimentInfo = null;

            lock (this.queueLock)
            {
                try
                {
                    //
                    // Check that the queued experiment info exists
                    //
                    if (experimentInfo == null)
                    {
                        throw new ArgumentNullException(STRERR_ExperimentInfoIsNull);
                    }

                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_StoreQueue, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_ExperimentId, experimentInfo.experimentId));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_SbName, experimentInfo.sbName));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_UserGroup, experimentInfo.userGroup));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_PriorityHint, experimentInfo.priorityHint));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_XmlSpecification, experimentInfo.xmlSpecification));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_EstimatedExecTime, experimentInfo.estExecutionTime));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Status, StatusCodes.Waiting.ToString()));

                    try
                    {
                        this.sqlConnection.Open();

                        if (sqlCommand.ExecuteNonQuery() == 0)
                        {
                            throw new Exception(STRERR_FailedToEnqueueExperiment);
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

                    //
                    // Get the queued experiment information and update with queue length 
                    //
                    queuedExperimentInfo = GetQueuedExperimentInfo(experimentInfo.experimentId, experimentInfo.sbName);
                    queuedExperimentInfo.queueLength--;

                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);
                }
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return queuedExperimentInfo;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Get the experiment information for the experiment waiting at the head of the queue.
        /// </summary>
        /// <returns></returns>
        public ExperimentInfo Dequeue(int unitId)
        {
            const string STRLOG_MethodName = "Dequeue";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            ExperimentInfo experimentInfo = null;

            //
            // Lock the queue so that two processes don't dequeue the same experiment before status is updated
            //
            lock (this.queueLock)
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_RetrieveQueueAllWithStatus, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Status, StatusCodes.Waiting.ToString()));

                    try
                    {
                        this.sqlConnection.Open();

                        SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                        if (sqlDataReader.Read() == true)
                        {
                            experimentInfo = new ExperimentInfo(0, null);

                            //
                            // Get the experiment information from waiting experiment
                            //
                            object sdrObject = null;
                            if ((sdrObject = sqlDataReader[STRSQL_QueueId]) != System.DBNull.Value)
                                experimentInfo.queueId = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_ExperimentId]) != System.DBNull.Value)
                                experimentInfo.experimentId = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_SbName]) != System.DBNull.Value)
                                experimentInfo.sbName = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_UserGroup]) != System.DBNull.Value)
                                experimentInfo.userGroup = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_PriorityHint]) != System.DBNull.Value)
                                experimentInfo.priorityHint = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_XmlSpecification]) != System.DBNull.Value)
                                experimentInfo.xmlSpecification = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_Status]) != System.DBNull.Value)
                                experimentInfo.status = (StatusCodes)Enum.Parse(typeof(StatusCodes), (string)sdrObject);
                            if ((sdrObject = sqlDataReader[STRSQL_EstimatedExecTime]) != System.DBNull.Value)
                                experimentInfo.estExecutionTime = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_Cancelled]) != System.DBNull.Value)
                                experimentInfo.cancelled = (bool)sdrObject;
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

                    if (experimentInfo != null)
                    {
                        //
                        // Update experiment status to 'Running' and update the unit ID
                        //
                        if (this.UpdateStatusToRunning(experimentInfo.experimentId, experimentInfo.sbName, unitId) == true)
                        {
                            experimentInfo.status = StatusCodes.Running;
                            experimentInfo.unitId = unitId;
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);
                }
            }

            string logMessage = null;
            if (experimentInfo != null)
            {
                logMessage = STRLOG_experimentId + experimentInfo.experimentId.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_sbName + experimentInfo.sbName;
            }
            else
            {
                logMessage = STRLOG_NoExperimentsWaiting;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return experimentInfo;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Update the status of the specified experiment.
        /// </summary>
        /// <returns></returns>
        public bool UpdateStatus(int experimentId, string sbName, StatusCodes statusCode)
        {
            const string STRLOG_MethodName = "UpdateStatus";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote +
                Logfile.STRLOG_Spacer + STRLOG_statusCode + statusCode.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Catch all exceptions thrown and return false if an error occurred.
            //
            bool success = false;

            lock (this.queueLock)
            {
                try
                {
                    //
                    // Update queued experiment status
                    //
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_UpdateQueueStatus, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_ExperimentId, experimentId));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_SbName, sbName));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Status, statusCode.ToString()));

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

        /// <summary>
        /// Cancel an experiment that is waiting on the queue.
        /// </summary>
        /// <param name="experimentId"></param>
        /// <param name="sbName"></param>
        /// <returns>True if experiment was successfully cancelled.</returns>
        public bool Cancel(int experimentId, string sbName)
        {
            const string STRLOG_MethodName = "Cancel";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote;

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Catch all exceptions thrown and return false if an error occurred.
            //
            bool success = false;

            lock (this.queueLock)
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_UpdateQueueCancel, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_ExperimentId, experimentId));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_SbName, sbName));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Status, StatusCodes.Waiting.ToString()));

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

        /// <summary>
        /// Get the number of experiments that are waiting on the queue.
        /// </summary>
        /// <param name="experimentId"></param>
        /// <param name="sbName"></param>
        /// <returns></returns>
        public int GetWaitCount()
        {
            const string STRLOG_MethodName = "GetWaitCount";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            int count = -1;

            lock (this.queueLock)
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_RetrieveQueueCountWithStatus, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Status, StatusCodes.Waiting.ToString()));

                    try
                    {
                        this.sqlConnection.Open();

                        count = (int)sqlCommand.ExecuteScalar();
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

            string logMessage = STRLOG_count + count.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return count;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Get the length of the queue and estimated queue wait time.
        /// </summary>
        /// <returns></returns>
        public WaitEstimate GetWaitEstimate()
        {
            const string STRLOG_MethodName = "GetWaitEstimate";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            WaitEstimate waitEstimate;

            //
            // Get the queued experiment information for non-existent experiment
            //
            QueuedExperimentInfo queuedExperimentInfo = GetQueuedExperimentInfo(0, null);

            //
            // Save the wait estimate information
            //
            if (queuedExperimentInfo != null)
            {
                waitEstimate = new WaitEstimate(queuedExperimentInfo.queueLength, queuedExperimentInfo.waitTime);
            }
            else
            {
                // Should never occur, but anyway...
                waitEstimate = new WaitEstimate();
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return waitEstimate;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Get the queued experiment information for the specified experiment. If the experiment is
        /// not waiting on the queue, 'experimentId' is set to zero and 'sbName' is set to null.
        /// In either case, the queue length and estimated queue wait are returned.
        /// </summary>
        /// <param name="experimentId"></param>
        /// <param name="sbName"></param>
        /// <returns></returns>
        public QueuedExperimentInfo GetQueuedExperimentInfo(int experimentId, string sbName)
        {
            const string STRLOG_MethodName = "GetQueuedExperimentInfo";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote;

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            QueuedExperimentInfo queuedExperimentInfo = null;
            int queueLength = 0;
            int position = 1;
            int waitTime = 0;

            lock (this.queueLock)
            {
                try
                {
                    //
                    // Scan through all experiments waiting in the queue
                    //
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_RetrieveQueueAllWithStatus, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Status, StatusCodes.Waiting.ToString()));

                    try
                    {
                        this.sqlConnection.Open();

                        SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                        while (sqlDataReader.Read() == true)
                        {
                            int _experimentId = -1;
                            string _sbName = string.Empty;
                            int estExecutionTime = -1;

                            //
                            // Get the experiment ID, ServiceBroker's name and execution time for this experiment
                            //
                            object sdrObject = null;
                            if ((sdrObject = sqlDataReader[STRSQL_ExperimentId]) != System.DBNull.Value)
                                _experimentId = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_SbName]) != System.DBNull.Value)
                                _sbName = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_EstimatedExecTime]) != System.DBNull.Value)
                                estExecutionTime = (int)sdrObject;

                            //
                            // Check the experiment ID and ServiceBroker's name for a match
                            //
                            if (_experimentId == experimentId && _sbName.Equals(sbName, StringComparison.OrdinalIgnoreCase) &&
                                queuedExperimentInfo == null)
                            {
                                //
                                // Found the experiment, save experiment information
                                //
                                queuedExperimentInfo = new QueuedExperimentInfo(experimentId, sbName);
                                queuedExperimentInfo.estExecutionTime = estExecutionTime;

                                if ((sdrObject = sqlDataReader[STRSQL_QueueId]) != System.DBNull.Value)
                                    queuedExperimentInfo.queueId = (int)sdrObject;
                                if ((sdrObject = sqlDataReader[STRSQL_UserGroup]) != System.DBNull.Value)
                                    queuedExperimentInfo.userGroup = (string)sdrObject;
                                if ((sdrObject = sqlDataReader[STRSQL_PriorityHint]) != System.DBNull.Value)
                                    queuedExperimentInfo.priorityHint = (int)sdrObject;
                                if ((sdrObject = sqlDataReader[STRSQL_XmlSpecification]) != System.DBNull.Value)
                                    queuedExperimentInfo.xmlSpecification = (string)sdrObject;
                                if ((sdrObject = sqlDataReader[STRSQL_Status]) != System.DBNull.Value)
                                    queuedExperimentInfo.status = (StatusCodes)Enum.Parse(typeof(StatusCodes), (string)sdrObject);
                                if ((sdrObject = sqlDataReader[STRSQL_UnitId]) != System.DBNull.Value)
                                    queuedExperimentInfo.unitId = (int)sdrObject;
                                if ((sdrObject = sqlDataReader[STRSQL_Cancelled]) != System.DBNull.Value)
                                    queuedExperimentInfo.cancelled = (bool)sdrObject;

                                queuedExperimentInfo.position = position;
                                queuedExperimentInfo.waitTime = waitTime;
                            }

                            // Add the wait time for this experiment
                            waitTime += estExecutionTime;

                            //
                            // Increment queue length and position
                            //
                            queueLength++;
                            position++;
                        }
                        sqlDataReader.Close();

                        //
                        // Check if the experiment was found
                        //
                        if (queuedExperimentInfo == null)
                        {
                            //
                            // Not found, only provide the queue length and estimated wait time
                            //
                            queuedExperimentInfo = new QueuedExperimentInfo(0, null);
                            queuedExperimentInfo.waitTime = waitTime;
                        }
                        queuedExperimentInfo.queueLength = queueLength;
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

                    logMessage = STRLOG_position + queuedExperimentInfo.position.ToString() +
                        Logfile.STRLOG_Spacer + STRLOG_queueLength + queuedExperimentInfo.queueLength +
                        Logfile.STRLOG_Spacer + STRLOG_waitTime + queuedExperimentInfo.waitTime.ToString() + STRLOG_seconds +
                        Logfile.STRLOG_Spacer + STRLOG_estExecutionTime + queuedExperimentInfo.estExecutionTime.ToString() + STRLOG_seconds;
                }
                catch (Exception ex)
                {
                    Logfile.WriteError(ex.Message);
                }
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return queuedExperimentInfo;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Get the status of the specified experiment. If the experiment is not found then the
        /// status of 'Unknown' will be returned.
        /// </summary>
        /// <param name="experimentId"></param>
        /// <param name="sbName"></param>
        /// <returns></returns>
        public StatusCodes GetExperimentStatus(int experimentId, string sbName)
        {
            const string STRLOG_MethodName = "GetExperimentStatus";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote;

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            StatusCodes status = StatusCodes.Unknown;

            lock (this.queueLock)
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_RetrieveQueue, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_ExperimentId, experimentId));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_SbName, sbName));

                    try
                    {
                        this.sqlConnection.Open();

                        SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                        if (sqlDataReader.Read() == true)
                        {
                            object sdrObject = null;
                            if ((sdrObject = sqlDataReader[STRSQL_Status]) != System.DBNull.Value)
                                status = (StatusCodes)Enum.Parse(typeof(StatusCodes), (string)sdrObject);
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

            logMessage = STRLOG_status + status.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return status;
        }

        //---------------------------------------------------------------------------------------//

        public ExperimentInfo[] RetrieveAllWithStatus(StatusCodes status)
        {
            const string STRLOG_MethodName = "RetrieveAllWithStatus";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            List<ExperimentInfo> experimentInfoList = new List<ExperimentInfo>();

            lock (this.queueLock)
            {
                try
                {
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_RetrieveQueueAllWithStatus, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Status, status.ToString()));

                    try
                    {
                        this.sqlConnection.Open();

                        SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                        while (sqlDataReader.Read() == true)
                        {
                            ExperimentInfo experimentInfo = new ExperimentInfo(0, null);

                            //
                            // Get the experiment information from waiting experiment
                            //
                            object sdrObject = null;
                            if ((sdrObject = sqlDataReader[STRSQL_ExperimentId]) != System.DBNull.Value)
                                experimentInfo.experimentId = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_SbName]) != System.DBNull.Value)
                                experimentInfo.sbName = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_UserGroup]) != System.DBNull.Value)
                                experimentInfo.userGroup = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_PriorityHint]) != System.DBNull.Value)
                                experimentInfo.priorityHint = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_XmlSpecification]) != System.DBNull.Value)
                                experimentInfo.xmlSpecification = (string)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_EstimatedExecTime]) != System.DBNull.Value)
                                experimentInfo.estExecutionTime = (int)sdrObject;
                            if ((sdrObject = sqlDataReader[STRSQL_Cancelled]) != System.DBNull.Value)
                                experimentInfo.cancelled = (bool)sdrObject;

                            //
                            // Add the experiment info to the list
                            //
                            experimentInfoList.Add(experimentInfo);
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

            string logMessage = STRLOG_count + experimentInfoList.Count.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return experimentInfoList.ToArray();
        }

        //---------------------------------------------------------------------------------------//

        public string RetrieveWaitingToXml()
        {
            const string STRLOG_MethodName = "RetrieveWaitingToXml";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            ExperimentInfo[] experimentInfoArray = this.RetrieveAllWithStatus(StatusCodes.Waiting);
            string xmlQueueWaiting = ConvertToXml(experimentInfoArray);

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return xmlQueueWaiting;
        }

        //=======================================================================================//

        /// <summary>
        /// Update the status of the specified experiment to 'Running' and update the unit ID.
        /// </summary>
        /// <returns></returns>
        private bool UpdateStatusToRunning(int experimentId, string sbName, int unitId)
        {
            const string STRLOG_MethodName = "UpdateStatusToRunning";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote +
                Logfile.STRLOG_Spacer + STRLOG_unitId + unitId.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Catch all exceptions thrown and return false if an error occurred.
            //
            bool success = false;

            lock (this.queueLock)
            {
                try
                {
                    //
                    // Update queued experiment status
                    //
                    SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_UpdateQueueStatusToRunning, this.sqlConnection);
                    sqlCommand.CommandType = CommandType.StoredProcedure;

                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_ExperimentId, experimentId));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_SbName, sbName));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Status, StatusCodes.Running.ToString()));
                    sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_UnitId, unitId));

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

        private string ConvertToXml(ExperimentInfo[] experimentInfoArray)
        {
            const string STRLOG_MethodName = "ConvertToXml";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Catch all exceptions thrown and return an empty string if an error occurred
            //
            XmlDocument xmlDocument = null;
            string xmlExperimentQueue = string.Empty;
            try
            {
                //
                // Check that the experiment info array exists
                //
                if (experimentInfoArray == null)
                {
                    throw new ArgumentNullException(STRERR_ExperimentInfoArrayIsNull);
                }

                //
                // Take the experiment info  and put into the XML document
                //
                for (int i = 0; i < experimentInfoArray.GetLength(0); i++)
                {
                    ExperimentInfo experimentInfo = experimentInfoArray[i];

                    // Load experiment results XML template string
                    XmlDocument xmlTemplateDocument = XmlUtilities.GetXmlDocument(STRXMLDOC_XmlTemplate);

                    //
                    // Fill in the XML template with values from the experiment information
                    //
                    XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlTemplateDocument, STRXML_experimentQueue);
                    XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlRootNode, STRXML_experiment);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_experimentId, experimentInfo.experimentId);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_sbName, experimentInfo.sbName, false);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_userGroup, experimentInfo.userGroup, false);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_priorityHint, experimentInfo.priorityHint);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_specification, experimentInfo.xmlSpecification, false);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_estExecutionTime, experimentInfo.estExecutionTime);
                    XmlUtilities.SetXmlValue(xmlNode, STRXML_cancelled, experimentInfo.cancelled);

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
                // Check if there were any experiments queued
                //
                if (xmlDocument == null)
                {
                    xmlDocument = XmlUtilities.GetXmlDocument(STRXMLDOC_XmlTemplate);
                    XmlNode xmlRootNode = XmlUtilities.GetXmlRootNode(xmlDocument, STRXML_experimentQueue);
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
                xmlExperimentQueue = sw.ToString();
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return xmlExperimentQueue;
        }

    }
}
