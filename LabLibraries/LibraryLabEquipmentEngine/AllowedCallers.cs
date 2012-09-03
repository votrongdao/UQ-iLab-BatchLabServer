using System;
using System.Collections.Generic;
using Library.Lab;

namespace Library.LabEquipment.Engine
{
    /// <summary>Provide authentication of ServiceBrokers (callers) before they can access LabServer methods.
    /// A list of callers that are allowed access is provided in the &lt;appsettings&gt; element of the
    /// Configuration file (web.config).
    /// </summary>
    public class AllowedCallers
    {
        // The list consists of a numerically sequential list of keys with the name "AllowedCaller?"
        // where '?' is a numeric value starting at 0. The value field of each key contains a comma-seperated-value
        // (CSV) string. The fields of each CSV string are:
        // 1. ServiceBroker's name - Typically the URL of the ServiceBroker.
        // 2. ServiceBroker's GUID - Typically a 32 hexadecimal character string. This string uniquely identifies
        //    the ServiceBroker and is passed to the LabServer in the SOAP header.
        // 3. ServiceBroker's Outgoing Passkey - Typically a 32 hexadecimal character string. This string is
        //    passed to the LabServer along with the ServiceBroker's GUID in the SOAP header.
        // 4. ServiceBroker's WebService URL - The LabServer accesses this web service to call the ServiceBroker's
        //    "Notify" web method.
        // 5. ServiceBroker's Incoming Passkey - A 64-bit (long) number that is passed to the ServiceBroker
        //    in the SOAP header when the LabServer calls the ServiceBroker's "Notify" web method.</para>
        // 6. Is Allowed? - A boolean value that can be used to disable a ServiceBroker's access to the LabServer's
        //    web methods.

        #region Class Constants and Variables

        private const string STRLOG_ClassName = "AllowedCallers";

        //
        // String constants
        //
        private const string STR_sbName = "sbName";
        private const string STR_sbGuid = "sbGuid";
        private const string STR_sbToLsPassKey = "sbToLsPassKey";
        private const string STR_sbNotifyUrl = "sbNotifyUrl";
        private const string STR_lsToSbPasskey = "lsToSbPasskey";
        private const string STR_isAllowed = "isAllowed";
        private const string STR_localhost = "localhost";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_CachingCallers = " Caching Callers...";
        private const string STRLOG_IsAuthenticating = " isAuthenticating: ";
        private const string STRLOG_IsLoggingCaller = " isLoggingCaller: ";
        private const string STRLOG_CallersCached = " Callers cached: ";
        private const string STRLOG_sbGuid = " sbGuid: ";
        private const string STRLOG_sbToLsPassKey = " sbToLsPassKey: ";
        private const string STRLOG_CallerAuthenticated = " Caller Authenticated: ";

        //
        // String constants for exception messages
        //
        private const string STRERR_CsvStringFieldCount = "CSV string has an incorrect number of fields";
        private const string STRERR_CsvStringEmptyField = "CSV string field is empty";
        private const string STRERR_AuthenticatingEmptyList = "Cannot authenticate an empty allowed caller list";
        private const string STRERR_CallerNotAllowed = "Caller not allowed!";
        private const string STRERR_IncorrectPasskey = "Passkey is incorrect!";
        private const string STRERR_CallerNotFound = "Caller not found!";

        //
        // Local constants
        //
        private enum CsvFields
        {
            SbName = 0,
            SbGuid = 1,
            SbToLsPasskey = 2,
            SbNotifyUrl = 3,
            LsToSbPasskey = 4,
            IsAllowed = 5,
            Length = 6
        };

        //
        // Local variables
        //
        private List<AllowedCaller> allowedCallerList;
        private bool isAuthenticating;
        private bool isLoggingCaller;

        /// <summary>
        /// Information about a single "allowed caller" which is provided in the Application's
        /// configuration file.
        /// </summary>
        private struct AllowedCaller
        {
            /// <summary>
            /// ServiceBroker's name, typically the URL of the ServiceBroker.
            /// </summary>
            public string sbName;

            /// <summary>
            /// ServiceBroker's GUID, typically a 32 hexadecimal character string.
            /// </summary>
            public string sbGuid;

            /// <summary>
            /// The passkey sent to the LabServer in the SOAP header object. The passkey
            /// identifies the calling ServiceBroker to the LabServer.
            /// </summary>
            public string sbToLsPassKey;

            /// <summary>
            /// URL of the ServiceBroker that will be notified of experiment completion
            /// </summary>
            public string sbNotifyUrl;

            /// <summary>
            /// The passkey sent to the ServiceBroker in the SOAP header object. The passkey
            /// identifies the calling LabServer to the ServiceBroker.
            /// </summary>
            public string lsToSbPasskey;

            /// <summary>
            /// If true, the LabServer allows calls from the ServiceBroker to the LabServer's
            /// WebMethods.
            /// </summary>
            public bool isAllowed;

            public AllowedCaller(string sbName, string sbGuid, string sbToLsPassKey,
                string sbNotifyUrl, string lsToSbPasskey, bool isAllowed)
            {
                this.sbName = sbName;
                this.sbGuid = sbGuid;
                this.sbToLsPassKey = sbToLsPassKey;
                this.sbNotifyUrl = sbNotifyUrl;
                this.lsToSbPasskey = lsToSbPasskey;
                this.isAllowed = isAllowed;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// A flag to indicate if authentication is enabled in the Application's configuration file
        /// </summary>
        public bool IsAuthenticating
        {
            get
            {
                return this.isAuthenticating;
            }
        }

        /// <summary>
        /// Number of allowed callers in the list
        /// </summary>
        public int Count
        {
            get
            {
                return this.allowedCallerList.Count;
            }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// <para>Exceptions:</para>
        /// <para>System.ArgumentNullException</para>
        /// <para>System.FormatException</para>
        /// </summary>
        public AllowedCallers()
            : this(null, false, false)
        {
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// <para>Exceptions:</para>
        /// <para>System.ArgumentNullException</para>
        /// <para>System.FormatException</para>
        /// </summary>
        /// <param name="allowedCallers"></param>
        /// <param name="authenticate"></param>
        /// <param name="logCaller"></param>
        public AllowedCallers(string[] allowedCallers, bool authenticate, bool logCaller)
        {
            const string STR_MethodName = "AllowedCallers";

            Logfile.WriteCalled(null, STR_MethodName);

            Logfile.Write(STRLOG_CachingCallers);

            // Create the allowed callers list
            this.allowedCallerList = new List<AllowedCaller>();

            bool configFileValues = false;

            if (allowedCallers == null)
            {
                configFileValues = true;
                try
                {
                    // Get the allowed caller values from the Application's configuration file
                    allowedCallers = Utilities.GetAppSettings(Consts.STRCFG_AllowedCaller);
                }
                catch
                {
                    // No allowed callers in configuration file, but that's ok
                }
            }

            try
            {
                //
                // Process the allowed callers
                //
                for (int i = 0; allowedCallers != null && i < allowedCallers.Length; i++)
                {
                    if (allowedCallers[i] == null)
                    {
                        throw new ArgumentNullException(Consts.STRCFG_AllowedCaller);
                    }

                    //
                    // Split the allowed caller CSV string into its parts
                    //
                    string[] strSplit = allowedCallers[i].Split(new char[] { Consts.CHR_CsvSplitterChar });
                    if (strSplit.Length != (int)CsvFields.Length)
                    {
                        throw new FormatException(STRERR_CsvStringFieldCount);
                    }

                    //
                    // Create an empty allowed caller ready to fill in
                    //
                    AllowedCaller allowedCaller = new AllowedCaller();

                    //
                    // Store ServiceBroker's name
                    //
                    allowedCaller.sbName = strSplit[(int)CsvFields.SbName].Trim();
                    if (allowedCaller.sbName.Length == 0)
                    {
                        throw new ArgumentNullException(STR_sbName, STRERR_CsvStringEmptyField);
                    }

                    //
                    // Store ServiceBroker's GUID
                    //
                    allowedCaller.sbGuid = strSplit[(int)CsvFields.SbGuid].Trim();
                    if (allowedCaller.sbGuid.Length == 0)
                    {
                        throw new ArgumentNullException(STR_sbGuid, STRERR_CsvStringEmptyField);
                    }

                    //
                    // Store ServiceBroker to LabServer passkey
                    //
                    allowedCaller.sbToLsPassKey = strSplit[(int)CsvFields.SbToLsPasskey].Trim();
                    if (allowedCaller.sbToLsPassKey.Length == 0)
                    {
                        throw new ArgumentNullException(STR_sbToLsPassKey, STRERR_CsvStringEmptyField);
                    }

                    //
                    // Store ServiceBroker's web service URL
                    //
                    allowedCaller.sbNotifyUrl = strSplit[(int)CsvFields.SbNotifyUrl].Trim();
                    if (allowedCaller.sbNotifyUrl.Length == 0)
                    {
                        throw new ArgumentNullException(STR_sbNotifyUrl, STRERR_CsvStringEmptyField);
                    }

                    //
                    // Store LabServer to ServiceBroker passkey
                    //
                    allowedCaller.lsToSbPasskey = strSplit[(int)CsvFields.LsToSbPasskey].Trim();
                    if (allowedCaller.lsToSbPasskey.Length == 0)
                    {
                        throw new ArgumentNullException(STR_lsToSbPasskey, STRERR_CsvStringEmptyField);
                    }

                    //
                    // Store allow access flag
                    //
                    string str = strSplit[(int)CsvFields.IsAllowed].Trim();
                    if (str.Length == 0)
                    {
                        throw new ArgumentNullException(STR_isAllowed, STRERR_CsvStringEmptyField);
                    }
                    try
                    {
                        allowedCaller.isAllowed = bool.Parse(str);
                    }
                    catch (Exception ex)
                    {
                        throw new ArgumentException(ex.Message, STR_isAllowed);
                    }

                    //
                    // Add allowed caller to the list
                    //
                    this.allowedCallerList.Add(allowedCaller);

                    Logfile.Write(" " + allowedCaller.sbName);
                }

                Logfile.Write(STRLOG_CallersCached + this.allowedCallerList.Count.ToString());

                //
                // Determine if caller authentication should be disabled
                //
                if (configFileValues == true)
                {
                    string authenticateCaller = null;
                    try
                    {
                        // Get authenticate flag from the Application Configuration file
                        authenticateCaller = Utilities.GetAppSetting(Consts.STRCFG_AuthenticateCaller);
                    }
                    catch
                    {
                        // Athenticate flag not specified
                    }

                    this.isAuthenticating = false;
                    if (authenticateCaller != null)
                    {
                        try
                        {
                            this.isAuthenticating = bool.Parse(authenticateCaller);
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
                // Check that we are not authenticating an empty allowed caller list
                //
                if (this.isAuthenticating == true && this.allowedCallerList.Count == 0)
                {
                    // Access to LabServer would always be denied
                    throw new ArgumentException(STRERR_AuthenticatingEmptyList);
                }

                //
                // Determine if caller identification should be logged
                //
                if (configFileValues == true)
                {
                    string logCallerIdPasskey = null;
                    try
                    {
                        // Get the caller logging flag from the Application's configuration file
                        logCallerIdPasskey = Utilities.GetAppSetting(Consts.STRCFG_LogCallerIdPasskey);
                    }
                    catch
                    {
                        // Caller logging not specified
                    }

                    this.isLoggingCaller = false;
                    if (logCallerIdPasskey != null)
                    {
                        try
                        {
                            this.isLoggingCaller = bool.Parse(logCallerIdPasskey);
                        }
                        catch (Exception ex)
                        {
                            throw new ArgumentException(ex.Message, Consts.STRCFG_LogCallerIdPasskey);
                        }
                    }
                }
                else
                {
                    // Set log caller directly
                    this.isLoggingCaller = logCaller;
                }

                Logfile.Write(STRLOG_IsLoggingCaller + isLoggingCaller.ToString());
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
        /// Search the allowed caller list for the specified ServiceBroker GUID and passkey.
        /// Return the ServiceBroker's name if found, otherwise return null.
        /// </summary>
        /// <param name="sbGuid"></param>
        /// <param name="sbToLsPasskey"></param>
        /// <returns></returns>
        public string Authentication(string sbGuid, string sbToLsPasskey)
        {
            //const string STRLOG_MethodName = "Authentication";

            //Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            string caller = null;

            //
            // Check special case where call is made directly from the LabServerWebService web page
            // on the localhost during application development and testing.
            //
            if (this.isAuthenticating == false && sbGuid == null && sbToLsPasskey == null)
            {
                // Caller is localhost where SOAP header contains no information
                caller = STR_localhost;
            }
            else
            {
                try
                {
                    //
                    // Check for ServiceBroker GUID
                    //
                    if (sbGuid == null)
                    {
                        throw new ArgumentNullException(STR_sbGuid);
                    }

                    //
                    // Check for Passkey
                    //
                    if (sbToLsPasskey == null)
                    {
                        throw new ArgumentNullException(STR_sbToLsPassKey);
                    }

                    //
                    // Remove whitespace
                    //
                    sbGuid = sbGuid.Trim();
                    sbToLsPasskey = sbToLsPasskey.Trim();

                    //
                    // Check if the GUID and passkey should be logged
                    //
                    if (this.isLoggingCaller == true)
                    {
                        // Log caller's id and passkey
                        Logfile.Write(STRLOG_sbGuid + sbGuid);
                        Logfile.Write(STRLOG_sbToLsPassKey + sbToLsPasskey);
                    }

                    //
                    // Scan the allowed caller list for a matching entry
                    //
                    for (int i = 0; i < this.allowedCallerList.Count; i++)
                    {
                        AllowedCaller allowedCaller = this.allowedCallerList[i];

                        //
                        // Find matching GUID - comparison is not case-sensitive
                        //
                        if (sbGuid.Equals(allowedCaller.sbGuid, StringComparison.OrdinalIgnoreCase) == true)
                        {
                            // Found GUID, now check the passkey - comparison is not case-sensitive
                            if (sbToLsPasskey.Equals(allowedCaller.sbToLsPassKey, StringComparison.OrdinalIgnoreCase) == true)
                            {
                                // Caller found, now check if caller is allowed
                                if (allowedCaller.isAllowed == false)
                                {
                                    // Caller is not allowed
                                    throw new ArgumentException(STRERR_CallerNotAllowed, allowedCaller.sbName);
                                }

                                // Caller is allowed
                                caller = allowedCaller.sbName;
                                break;
                            }
                            else
                            {
                                // Passkey does not match, caller not allowed
                                throw new ArgumentException(STRERR_IncorrectPasskey, allowedCaller.sbName);
                            }
                        }
                    }

                    if (caller == null)
                    {
                        // Caller not found in list
                        throw new ArgumentException(STRERR_CallerNotFound);
                    }

                    // Caller found
                    Logfile.Write(STRLOG_CallerAuthenticated + caller);
                }
                catch (Exception ex)
                {
                    // Log the message but don't throw the exception back to the caller
                    Logfile.WriteError(ex.Message);
                }
            }

            //Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return caller;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Get the ServiceBroker's name. Return null if the ServiceBroker's name is not in the list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetSbName(string caller)
        {
            string sbName = null;

            if (caller != null)
            {
                // Remove whitespace
                caller = caller.Trim();

                //
                // Scan the allowed caller list for a matching entry
                //
                for (int i = 0; i < this.allowedCallerList.Count; i++)
                {
                    AllowedCaller allowedCaller = this.allowedCallerList[i];

                    //
                    // Compare sbName - comparison is not case-sensitive
                    //
                    if (allowedCaller.sbName.Equals(caller, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // Save the lsToSbPasskey
                        sbName = allowedCaller.sbName;
                    }
                }
            }

            return sbName;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Get the ServiceBroker's URL. Return null if the ServiceBroker's name is not in the list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetSbNotifyUrl(string caller)
        {
            string sbNotifyUrl = null;

            if (caller != null)
            {
                // Remove whitespace
                caller = caller.Trim();

                //
                // Scan the allowed caller list for a matching entry
                //
                for (int i = 0; i < this.allowedCallerList.Count; i++)
                {
                    AllowedCaller allowedCaller = this.allowedCallerList[i];

                    //
                    // Compare sbName - comparison is not case-sensitive
                    //
                    if (allowedCaller.sbName.Equals(caller, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // Save the lsToSbPasskey
                        sbNotifyUrl = allowedCaller.sbNotifyUrl;
                    }
                }
            }

            return sbNotifyUrl;
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// Get the passkey to send to the ServiceBroker. Return null if the ServiceBroker's name is not in the list.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public string GetLsToSbPasskey(string caller)
        {
            string lsToSbPasskey = null;

            if (caller != null)
            {
                // Remove whitespace
                caller = caller.Trim();

                //
                // Scan the allowed caller list for a matching entry
                //
                for (int i = 0; i < this.allowedCallerList.Count; i++)
                {
                    AllowedCaller allowedCaller = this.allowedCallerList[i];

                    //
                    // Compare sbName - comparison is not case-sensitive
                    //
                    if (allowedCaller.sbName.Equals(caller, StringComparison.OrdinalIgnoreCase) == true)
                    {
                        // Save the lsToSbPasskey
                        lsToSbPasskey = allowedCaller.lsToSbPasskey;
                    }
                }
            }

            return lsToSbPasskey;
        }

    }
}
