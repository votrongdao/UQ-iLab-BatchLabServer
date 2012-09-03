using System;
using System.Diagnostics;
using Library.Lab;

namespace Library.LabServerEngine
{
    public class LabServerToSbAPI
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "LabServerToSbAPI";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_experimentId = " experimentId: ";
        private const string STRLOG_sbName = " sbName: ";
        private const string STRLOG_webServiceUrl = " webServiceUrl: ";
        private const string STRLOG_lsToSbPasskey = " lsToSbPasskey: ";
        private const string STRLOG_labServerId = " labServerId: ";
        private const string STRLOG_SbNotifyUrlNotSpecified = " sbNotifyUrl is not specified. ServiceBroker will not be notified.";
        private const string STRLOG_NotifyingServiceBroker = "Notifying ServiceBroker -> ";
        private const string STRLOG_success = " success: ";

        //
        // Local variables
        //
        private AllowedServiceBrokersDB allowedServiceBrokers;

        #endregion

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// </summary>
        /// <param name="allowedCallers"></param>
        public LabServerToSbAPI(AllowedServiceBrokersDB allowedServiceBrokers)
        {
            const string STRLOG_MethodName = "LabServerToSbAPI";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            this.allowedServiceBrokers = allowedServiceBrokers;

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //---------------------------------------------------------------------------------------//

        /// <summary>
        /// 
        /// </summary>
        /// <param name="experimentID"></param>
        /// <param name="sbName"></param>
        /// <returns>True if the ServiceBroker was successfully notified.</returns>
        public bool Notify(int experimentId, string sbName)
        {
            const string STRLOG_MethodName = "Notify";

            string logMessage = STRLOG_experimentId + experimentId.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_sbName + Logfile.STRLOG_Quote + sbName + Logfile.STRLOG_Quote;

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = true;

            //
            // Get the ServiceBroker's web service URL for notification
            //
            ServiceBrokerInfo serviceBrokerInfo = this.allowedServiceBrokers.GetServiceBrokerInfo(sbName);
            string webServiceUrl = serviceBrokerInfo.webServiceUrl;
            if (webServiceUrl != null && webServiceUrl.Length > 0)
            {
                //
                // Notify Url is specified so notify ServiceBroker
                //
                try
                {
                    Logfile.Write(STRLOG_webServiceUrl + webServiceUrl);

                    //
                    // Get the passkey for notification
                    //
                    string lsToSbPasskey = serviceBrokerInfo.incomingPasskey;
                    if (lsToSbPasskey == null)
                    {
                        throw new ArgumentNullException(STRLOG_lsToSbPasskey);
                    }

                    //Trace.WriteLine(STRLOG_lsToSbPasskey + lsToSbPasskey);

                    //
                    // Get LabServer Id from application's configuration file
                    //
                    string str = Utilities.GetAppSetting(Consts.STRCFG_LabServerGuid);
                    str = str.Substring(0, 16);
                    Int64 labServerId = Convert.ToInt64(str, 16);

                    //Trace.WriteLine(STRLOG_labServerId + "0x" + labServerId.ToString("X"));

                    //
                    // Create ServiceBroker interface
                    //
                    ServiceBrokerService serviceBroker = new ServiceBrokerService();
                    serviceBroker.Url = webServiceUrl;

                    //
                    // Create and fill in authorisation information
                    //
                    sbAuthHeader sbHeader = new sbAuthHeader();
                    sbHeader.couponID = labServerId;
                    sbHeader.couponPassKey = lsToSbPasskey;
                    serviceBroker.sbAuthHeaderValue = sbHeader;

                    //
                    // Notify the ServiceBroker that this experiment has finished
                    //
                    serviceBroker.Notify(experimentId);
                }
                catch (Exception ex)
                {
                    //
                    // ServiceBroker notification failed
                    //
                    logMessage = ex.Message;
                    if (ex.InnerException != null)
                    {
                        logMessage += " ==>> " + ex.InnerException.Message;
                    }
                    Logfile.WriteError(logMessage);

                    success = false;
                }
            }
            else
            {
                //
                // Notify Url is not specified so cannot notify ServiceBroker
                //
                Logfile.Write(STRLOG_SbNotifyUrlNotSpecified);
            }

            logMessage = STRLOG_success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

    }
}
