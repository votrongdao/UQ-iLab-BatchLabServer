using System;
using Library.Lab;

namespace Library.LabClient
{
    public class LabClientToSbAPI
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "LabClientToSbAPI";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_NotSpecified = "Not specified!";
        private const string STRLOG_couponID = " couponID: ";
        private const string STRLOG_serviceUrl = " serviceUrl: ";
        private const string STRLOG_labserverID = " labserverID: ";

        //
        // Local variables
        //
        private ServiceBrokerService serviceBroker;
        private string labServerId;

        #endregion

        //---------------------------------------------------------------------------------------//

        public LabClientToSbAPI(string strCouponID, string passkey, string serviceUrl, string labserverID)
        {
            const string STRLOG_MethodName = "LabClientToSbAPI";

            //
            // Log the serviceUrl and labserverID arguments
            //
            string logMessage = STRLOG_serviceUrl;
            if (serviceUrl == null)
            {
                logMessage += STRLOG_NotSpecified;
            }
            else
            {
                logMessage += Logfile.STRLOG_Quote + serviceUrl + Logfile.STRLOG_Quote;
            }
            logMessage += Logfile.STRLOG_Spacer + STRLOG_labserverID;
            if (labserverID == null)
            {
                logMessage += STRLOG_NotSpecified;
            }
            else
            {
                logMessage += Logfile.STRLOG_Quote;
                if (labserverID.Length > 4)
                {
                    logMessage += "..." + labserverID.Substring(labserverID.Length - 4);
                }
                else
                {
                    logMessage += labserverID;
                }
                logMessage += Logfile.STRLOG_Quote;
            }

            Logfile.WriteCalled(null, STRLOG_MethodName, logMessage);

            //
            // Get the coupon ID, ServiceBroker's service URL and LabServer ID
            //
            long couponID = 0;
            try
            {
                //
                // Convert couponID to a number
                //
                couponID = Convert.ToInt64(strCouponID);
                Logfile.Write(STRLOG_couponID + couponID.ToString());

                //
                // Get the ServiceBroker's service URL
                //
                if (serviceUrl == null)
                {
                    serviceUrl = Utilities.GetAppSetting(Consts.STRCFG_ServiceUrl);
                }
                if (serviceUrl == null)
                {
                    throw new ArgumentNullException(Consts.STRCFG_ServiceUrl);
                }

                //
                // Get the LabServer's ID
                //
                this.labServerId = labserverID;
                if (this.labServerId == null)
                {
                    this.labServerId = Utilities.GetAppSetting(Consts.STRCFG_LabServerId);
                }
                if (this.labServerId == null)
                {
                    throw new ArgumentNullException(Consts.STRCFG_LabServerId);
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            //
            // Create ServiceBroker interface
            //
            this.serviceBroker = new ServiceBrokerService();
            this.serviceBroker.Url = serviceUrl;

            //
            // Create authorisation information and fill in
            //
            sbAuthHeader sbHeader = new sbAuthHeader();
            sbHeader.couponID = couponID;
            sbHeader.couponPassKey = passkey;
            this.serviceBroker.sbAuthHeaderValue = sbHeader;

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //---------------------------------------------------------------------------------------//

        public bool Cancel(int experimentID)
        {
            return this.serviceBroker.Cancel(experimentID);
        }

        //---------------------------------------------------------------------------------------//

        public WaitEstimate GetEffectiveQueueLength()
        {
            return this.serviceBroker.GetEffectiveQueueLength(this.labServerId, 0);
        }

        //---------------------------------------------------------------------------------------//

        public WaitEstimate GetEffectiveQueueLength(int priorityHint)
        {
            return this.serviceBroker.GetEffectiveQueueLength(this.labServerId, priorityHint);
        }

        //---------------------------------------------------------------------------------------//

        public LabExperimentStatus GetExperimentStatus(int experimentID)
        {
            return this.serviceBroker.GetExperimentStatus(experimentID);
        }

        //---------------------------------------------------------------------------------------//

        public string GetLabConfiguration()
        {
            return this.serviceBroker.GetLabConfiguration(this.labServerId);
        }

        //---------------------------------------------------------------------------------------//

        public string GetLabInfo()
        {
            return this.serviceBroker.GetLabInfo(this.labServerId);
        }

        //---------------------------------------------------------------------------------------//

        public LabStatus GetLabStatus()
        {
            return this.serviceBroker.GetLabStatus(this.labServerId);
        }

        //---------------------------------------------------------------------------------------//

        public ResultReport RetrieveResult(int experimentID)
        {
            return this.serviceBroker.RetrieveResult(experimentID);
        }

        //---------------------------------------------------------------------------------------//

        public SubmissionReport Submit(string experimentSpecification)
        {
            return this.serviceBroker.Submit(this.labServerId, experimentSpecification, 0, false);
        }

        //---------------------------------------------------------------------------------------//

        public SubmissionReport Submit(string experimentSpecification, int priorityHint, bool emailNotification)
        {
            return this.serviceBroker.Submit(this.labServerId, experimentSpecification,
                priorityHint, emailNotification);
        }

        //---------------------------------------------------------------------------------------//

        public ValidationReport Validate(string experimentSpecification)
        {
            return this.serviceBroker.Validate(this.labServerId, experimentSpecification);
        }

    }
}
