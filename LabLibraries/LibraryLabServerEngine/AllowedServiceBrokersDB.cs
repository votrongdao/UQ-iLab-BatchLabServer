using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using Library.Lab;

namespace Library.LabServerEngine
{
    public class AllowedServiceBrokersDB
    {
        #region Constants

        private const string STRLOG_ClassName = "AllowedServiceBrokersDB";

        //
        // String constants for SQL processing
        //
        private const string STRSQLCMD_DeleteServiceBroker = "DeleteServiceBroker";
        private const string STRSQLCMD_RetrieveServiceBroker = "RetrieveServiceBroker";
        private const string STRSQLCMD_RetrieveServiceBrokerAll = "RetrieveServiceBrokerAll";
        private const string STRSQLCMD_StoreServiceBroker = "StoreServiceBroker";
        private const string STRSQLCMD_UpdateServiceBroker = "UpdateServiceBroker";

        private const string STRSQLPRM_Name = "@Name";
        private const string STRSQLPRM_Guid = "@Guid";
        private const string STRSQLPRM_OutgoingPasskey = "@OutgoingPasskey";
        private const string STRSQLPRM_IncomingPasskey = "@IncomingPasskey";
        private const string STRSQLPRM_WebServiceUrl = "@WebServiceUrl";
        private const string STRSQLPRM_IsAllowed = "@IsAllowed";

        private const string STRSQL_Name = "Name";
        private const string STRSQL_Guid = "Guid";
        private const string STRSQL_OutgoingPasskey = "OutgoingPasskey";
        private const string STRSQL_IncomingPasskey = "IncomingPasskey";
        private const string STRSQL_WebServiceUrl = "WebServiceUrl";
        private const string STRSQL_IsAllowed = "IsAllowed";

        //
        // String constants
        //
        private const string STR_name = "name";
        private const string STR_guid = "guid";
        private const string STR_outgoingPasskey = "outgoingPasskey";
        private const string STR_localhost = "localhost";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_CachingServiceBrokers = " Caching ServiceBrokers...";
        private const string STRLOG_IsAuthenticating = " isAuthenticating: ";
        private const string STRLOG_IsLoggingServiceBroker = " isLoggingServiceBroker: ";
        private const string STRLOG_CallersCached = " Callers cached: ";
        private const string STRLOG_Guid = " Guid: ";
        private const string STRLOG_OutgoingPasskey = " OutgoingPasskey: ";
        private const string STRLOG_ServiceBrokerAuthenticated = " ServiceBroker Authenticated: ";

        private const string STRLOG_name = " name: ";
        private const string STRLOG_success = " success: ";
        private const string STRLOG_count = "count: ";
        private const string STRLOG_ServiceBrokerNotFound = "ServiceBroker not found.";

        //
        // String constants for exception messages
        //
        private const string STRERR_SqlException = "SqlException: ";
        private const string STRERR_Exception = "Exception: ";
        private const string STRERR_AuthenticatingEmptyList = "Cannot authenticate an empty ServiceBroker info list";
        private const string STRERR_ServiceBrokerNotAllowed = "ServiceBroker not allowed!";
        private const string STRERR_IncorrectPasskey = "Passkey is incorrect!";
        private const string STRERR_ServiceBrokerNotFound = "ServiceBroker not found!";

        #endregion

        #region Variables

        //
        // Local variables
        //
        private SqlConnection sqlConnection;
        private List<ServiceBrokerInfo> serviceBrokerInfoList;
        private bool isAuthenticating;
        private bool isLoggingServiceBroker;

        #endregion

        #region Properties

        /// <summary>
        /// A flag to indicate if authentication is enabled in the Application's configuration file
        /// </summary>
        public bool IsAuthenticating
        {
            get { return this.isAuthenticating; }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        public AllowedServiceBrokersDB()
            : this(true, false, false)
        {
        }

        //---------------------------------------------------------------------------------------//

        public AllowedServiceBrokersDB(bool useConfigFileValues, bool authenticate, bool logCaller)
        {
            const string STR_MethodName = "AllowedServiceBrokersDB";

            Logfile.WriteCalled(null, STR_MethodName);

            try
            {
                //
                // Determine if ServiceBroker authentication should be disabled
                //
                if (useConfigFileValues == true)
                {
                    string authenticateServiceBroker = null;
                    try
                    {
                        // Get authenticate flag from the Application Configuration file
                        authenticateServiceBroker = Utilities.GetAppSetting(Consts.STRCFG_AuthenticateCaller);
                    }
                    catch
                    {
                        // Athenticate flag not specified
                    }

                    this.isAuthenticating = false;
                    if (authenticateServiceBroker != null)
                    {
                        try
                        {
                            this.isAuthenticating = bool.Parse(authenticateServiceBroker);
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentException(ex.Message, Consts.STRCFG_AuthenticateCaller);
                        }
                    }
                }
                else
                {
                    // Set authenticate directly
                    this.isAuthenticating = authenticate;
                }

                Logfile.Write(STRLOG_IsAuthenticating + isAuthenticating.ToString());

                //
                // Check if authenticating
                //
                if (this.isAuthenticating == true)
                {
                    //
                    // Get the SQL connection string from Application's configuration file
                    //
                    string sqlConnectionString = Utilities.GetAppSetting(Consts.STRCFG_SqlConnection);
                    this.sqlConnection = new SqlConnection(sqlConnectionString);

                    //
                    // Get allowed ServiceBroker list
                    //
                    Logfile.Write(STRLOG_CachingServiceBrokers);
                    this.serviceBrokerInfoList = this.RetrieveAll();

                    //
                    // Check that we are not authenticating an empty allowed ServiceBroker list
                    //
                    if (this.serviceBrokerInfoList.Count == 0)
                    {
                        // Access to LabServer would always be denied
                        throw new ArgumentException(STRERR_AuthenticatingEmptyList);
                    }

                    //
                    // Log the names of the cached ServiceBrokers
                    //
                    for (int i = 0; i < this.serviceBrokerInfoList.Count; i++)
                    {
                        Logfile.Write(String.Format(" {0}: {1}", i + 1, this.serviceBrokerInfoList[i].name));
                    }
                }

                //
                // Determine if ServiceBroker identification should be logged
                //
                if (useConfigFileValues == true)
                {
                    string logNamePasskey = null;
                    try
                    {
                        // Get the caller logging flag from the Application's configuration file
                        logNamePasskey = Utilities.GetAppSetting(Consts.STRCFG_LogCallerIdPasskey);
                    }
                    catch
                    {
                        // Caller logging not specified
                    }

                    this.isLoggingServiceBroker = false;
                    if (logNamePasskey != null)
                    {
                        try
                        {
                            this.isLoggingServiceBroker = bool.Parse(logNamePasskey);
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentException(ex.Message, Consts.STRCFG_LogCallerIdPasskey);
                        }
                    }
                }
                else
                {
                    // Set log ServiceBroker directly
                    this.isLoggingServiceBroker = logCaller;
                }

                Logfile.Write(STRLOG_IsLoggingServiceBroker + isLoggingServiceBroker.ToString());
            }
            catch (Exception ex)
            {
                // Log the message and throw the exception back to the caller
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STR_MethodName);
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Search the allowed ServiceBroker info list for the specified GUID and passkey.
        /// Return the ServiceBroker's name if found, otherwise return null.
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="sbToLsPasskey"></param>
        /// <returns></returns>
        public string Authentication(string guid, string outgoingPassKey)
        {
            //const string STRLOG_MethodName = "Authentication";

            //Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            string serviceBrokerName = null;

            //
            // Check special case where call is made directly from the LabServerWebService web page
            // on the localhost during application development and testing.
            //
            if (this.isAuthenticating == false && guid == null && outgoingPassKey == null)
            {
                // ServiceBroker is localhost where SOAP header contains no information
                serviceBrokerName = STR_localhost;
            }
            else
            {
                try
                {
                    //
                    // Check for ServiceBroker GUID
                    //
                    if (guid == null)
                    {
                        throw new ArgumentNullException(STR_guid);
                    }

                    //
                    // Check for Outgoing Passkey (ServiceBroker to LabServer)
                    //
                    if (outgoingPassKey == null)
                    {
                        throw new ArgumentNullException(STR_outgoingPasskey);
                    }

                    //
                    // Remove whitespace
                    //
                    guid = guid.Trim();
                    outgoingPassKey = outgoingPassKey.Trim();

                    //
                    // Check if the GUID and passkey should be logged
                    //
                    if (this.isLoggingServiceBroker == true)
                    {
                        // Log ServiceBroker's GUID and passkey
                        Logfile.Write(STRLOG_Guid + guid);
                        Logfile.Write(STRLOG_OutgoingPasskey + outgoingPassKey);
                    }

                    //
                    // Scan the allowed ServiceBroker info list for a matching entry
                    //
                    for (int i = 0; i < this.serviceBrokerInfoList.Count; i++)
                    {
                        ServiceBrokerInfo serviceBrokerInfo = this.serviceBrokerInfoList[i];

                        //
                        // Find matching GUID - comparison is not case-sensitive
                        //
                        if (guid.Equals(serviceBrokerInfo.guid, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            // Found GUID, now check the passkey - comparison is not case-sensitive
                            if (outgoingPassKey.Equals(serviceBrokerInfo.outgoingPasskey, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                // ServiceBroker found, now check if ServiceBroker is allowed
                                if (serviceBrokerInfo.isAllowed == false)
                                {
                                    // ServiceBroker is not allowed
                                    throw new ArgumentException(STRERR_ServiceBrokerNotAllowed, serviceBrokerInfo.name);
                                }

                                // ServiceBroker is allowed
                                serviceBrokerName = serviceBrokerInfo.name;
                                break;
                            }
                            else
                            {
                                // Passkey does not match, caller not allowed
                                throw new ArgumentException(STRERR_IncorrectPasskey, serviceBrokerInfo.name);
                            }
                        }
                    }

                    if (serviceBrokerName == null)
                    {
                        // ServiceBroker not found in list
                        throw new ArgumentException(STRERR_ServiceBrokerNotFound);
                    }

                    // Caller found
                    Logfile.Write(STRLOG_ServiceBrokerAuthenticated + Logfile.STRLOG_Quote + serviceBrokerName + Logfile.STRLOG_Quote);
                }
                catch (Exception ex)
                {
                    // Log the message but don't throw the exception back to the caller
                    Logfile.WriteError(ex.Message);
                }
            }

            //Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return serviceBrokerName;
        }

        //---------------------------------------------------------------------------------------//

        public string[] GetServiceBrokerNames()
        {
            int count = this.serviceBrokerInfoList.Count;
            string[] names = new string[count];

            for (int i = 0; i < count; i++)
            {
                names[i] = this.serviceBrokerInfoList[i].name;
            }

            return names;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Get the ServiceBroker info for the specified name. Return null if the ServiceBroker's name is not in the list.
        /// </summary>
        /// <param name="sbName"></param>
        /// <returns></returns>
        public ServiceBrokerInfo GetServiceBrokerInfo(string sbName)
        {
            ServiceBrokerInfo serviceBrokerInfo = null;

            if (sbName != null)
            {
                // Remove whitespace
                sbName = sbName.Trim();

                //
                // Scan the allowed ServiceBroker info list for a matching entry
                //
                for (int i = 0; i < this.serviceBrokerInfoList.Count; i++)
                {
                    //
                    // Compare name - comparison is not case-sensitive
                    //
                    if (this.serviceBrokerInfoList[i].name.Equals(sbName, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        //
                        // Save the ServiceBroker info
                        //
                        serviceBrokerInfo = this.serviceBrokerInfoList[i];
                        break;
                    }
                }
            }

            return serviceBrokerInfo;
        }

        //---------------------------------------------------------------------------------------//

        public bool Delete(string name)
        {
            const string STRLOG_MethodName = "Delete";

            string logMessage = STRLOG_name + Logfile.STRLOG_Quote + name + Logfile.STRLOG_Quote;

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Catch all exceptions thrown and return false if an error occurred.
            //
            bool success = false;

            try
            {
                //
                // Update queued experiment status
                //
                SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_DeleteServiceBroker, this.sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Name, name));

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

            logMessage = STRLOG_success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public ServiceBrokerInfo Retrieve(string guid)
        {
            const string STRLOG_MethodName = "Retrieve";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            ServiceBrokerInfo serviceBrokerInfo = null;

            try
            {
                SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_RetrieveServiceBroker, this.sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Guid, guid));

                try
                {
                    this.sqlConnection.Open();

                    SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                    if (sqlDataReader.Read() == true)
                    {
                        serviceBrokerInfo = new ServiceBrokerInfo();

                        //
                        // Get the experiment information from waiting experiment
                        //
                        object sdrObject = null;
                        if ((sdrObject = sqlDataReader[STRSQL_Name]) != System.DBNull.Value)
                            serviceBrokerInfo.name = (string)sdrObject;
                        if ((sdrObject = sqlDataReader[STRSQL_Guid]) != System.DBNull.Value)
                            serviceBrokerInfo.guid = (string)sdrObject;
                        if ((sdrObject = sqlDataReader[STRSQL_OutgoingPasskey]) != System.DBNull.Value)
                            serviceBrokerInfo.outgoingPasskey = (string)sdrObject;
                        if ((sdrObject = sqlDataReader[STRSQL_IncomingPasskey]) != System.DBNull.Value)
                            serviceBrokerInfo.incomingPasskey = (string)sdrObject;
                        if ((sdrObject = sqlDataReader[STRSQL_WebServiceUrl]) != System.DBNull.Value)
                            serviceBrokerInfo.webServiceUrl = (string)sdrObject;
                        if ((sdrObject = sqlDataReader[STRSQL_IsAllowed]) != System.DBNull.Value)
                            serviceBrokerInfo.isAllowed = (bool)sdrObject;
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

            string logMessage = null;
            if (serviceBrokerInfo != null)
            {
                logMessage = STRLOG_name + serviceBrokerInfo.name;
            }
            else
            {
                logMessage = STRLOG_ServiceBrokerNotFound;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return serviceBrokerInfo;
        }

        //---------------------------------------------------------------------------------------//

        public bool Store(ServiceBrokerInfo serviceBroker)
        {
            const string STRLOG_MethodName = "Store";

            string logMessage = STRLOG_name + Logfile.STRLOG_Quote + serviceBroker.name + Logfile.STRLOG_Quote;

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Catch all exceptions thrown and return false if an error occurred.
            //
            bool success = false;
            try
            {
                //
                // Update queued experiment status
                //
                SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_StoreServiceBroker, this.sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Name, serviceBroker.name));
                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Guid, serviceBroker.guid));
                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_IncomingPasskey, serviceBroker.incomingPasskey));
                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_OutgoingPasskey, serviceBroker.outgoingPasskey));
                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_WebServiceUrl, serviceBroker.webServiceUrl));
                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_IsAllowed, serviceBroker.isAllowed));

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

            logMessage = STRLOG_success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        public bool Update(ServiceBrokerInfo serviceBroker)
        {
            const string STRLOG_MethodName = "Update";

            string logMessage = STRLOG_name + Logfile.STRLOG_Quote + serviceBroker.name + Logfile.STRLOG_Quote;

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Catch all exceptions thrown and return false if an error occurred.
            //
            bool success = false;
            try
            {
                //
                // Update queued experiment status
                //
                SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_UpdateServiceBroker, this.sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Name, serviceBroker.name));
                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_Guid, serviceBroker.guid));
                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_IncomingPasskey, serviceBroker.incomingPasskey));
                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_OutgoingPasskey, serviceBroker.outgoingPasskey));
                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_WebServiceUrl, serviceBroker.webServiceUrl));
                sqlCommand.Parameters.Add(new SqlParameter(STRSQLPRM_IsAllowed, serviceBroker.isAllowed));

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

            logMessage = STRLOG_success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //=======================================================================================//

        private List<ServiceBrokerInfo> RetrieveAll()
        {
            const string STRLOG_MethodName = "RetrieveAll";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            List<ServiceBrokerInfo> serviceBrokerInfoList = new List<ServiceBrokerInfo>();

            try
            {
                SqlCommand sqlCommand = new SqlCommand(STRSQLCMD_RetrieveServiceBrokerAll, this.sqlConnection);
                sqlCommand.CommandType = CommandType.StoredProcedure;

                try
                {
                    this.sqlConnection.Open();

                    SqlDataReader sqlDataReader = sqlCommand.ExecuteReader();
                    while (sqlDataReader.Read() == true)
                    {
                        ServiceBrokerInfo serviceBrokerInfo = new ServiceBrokerInfo();

                        object sdrObject = null;
                        if ((sdrObject = sqlDataReader[STRSQL_Name]) != System.DBNull.Value)
                            serviceBrokerInfo.name = (string)sdrObject;
                        if ((sdrObject = sqlDataReader[STRSQL_Guid]) != System.DBNull.Value)
                            serviceBrokerInfo.guid = (string)sdrObject;
                        if ((sdrObject = sqlDataReader[STRSQL_OutgoingPasskey]) != System.DBNull.Value)
                            serviceBrokerInfo.outgoingPasskey = (string)sdrObject;
                        if ((sdrObject = sqlDataReader[STRSQL_IncomingPasskey]) != System.DBNull.Value)
                            serviceBrokerInfo.incomingPasskey = (string)sdrObject;
                        if ((sdrObject = sqlDataReader[STRSQL_WebServiceUrl]) != System.DBNull.Value)
                            serviceBrokerInfo.webServiceUrl = (string)sdrObject;
                        if ((sdrObject = sqlDataReader[STRSQL_IsAllowed]) != System.DBNull.Value)
                            serviceBrokerInfo.isAllowed = (bool)sdrObject;

                        //
                        // Add the ServiceBroker information to the list
                        //
                        serviceBrokerInfoList.Add(serviceBrokerInfo);
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

            string logMessage = STRLOG_count + serviceBrokerInfoList.Count.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return serviceBrokerInfoList;
        }

    }
}
