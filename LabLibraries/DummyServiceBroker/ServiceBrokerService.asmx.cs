using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Web;
using System.Web.Hosting;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;
using Library.Lab;
using Library.ServiceBroker;

namespace ServiceBroker
{
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    [XmlRootAttribute(Namespace = "http://ilab.mit.edu", IsNullable = false)]
    public class sbAuthHeader : SoapHeader
    {
        public long couponID;
        public string couponPassKey;
    }

    [WebService(Namespace = "http://ilab.mit.edu")]
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    public class ServiceBrokerService : System.Web.Services.WebService
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "ServiceBrokerService";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_ExperimentId = " ExperimentId: ";
        private const string STRLOG_AccessDenied = "Access Denied!";
        private const string STRLOG_Success = " Success: ";

        //
        // String constants
        //
        private const string userGroup = "Test Group";
        private const string STR_ExperimentIdFilename = "App_Data\\ExperimentID.dat";

        //
        // Local variables
        //
        private static int nextExperimentId = 0;
        public sbAuthHeader sbHeader;

        #endregion

        //---------------------------------------------------------------------------------------//

        public ServiceBrokerService()
        {
            const string STRLOG_MethodName = "ServiceBrokerService";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            if (nextExperimentId == 0)
            {
                // Initialise the experiment number
                nextExperimentId = GetNextExperimentID();
            }

            Logfile.Write(" Experiment Id: " + nextExperimentId.ToString());

            sbHeader = new sbAuthHeader();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //---------------------------------------------------------------------------------------//

        private LabServerWebService GetLabServer(sbAuthHeader sbHeader)
        {
            const string STRLOG_MethodName = "GetLabServer";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            long couponId = sbHeader.couponID;
            string couponPasskey = sbHeader.couponPassKey;

            Logfile.Write(" Coupon Id: " + couponId.ToString());
            Logfile.Write(" Coupon Passkey: " + couponPasskey);

            LabServerWebService labServer = null;
            try
            {
                //
                // Get settings from Application's configuration file
                //
                string labserverId = couponId.ToString();

                Logfile.Write(" Labserver Id: " + labserverId);

                string labServerUrl = Utilities.GetAppSetting(labserverId);
                string sbGuid = Utilities.GetAppSetting(Consts.STRCFG_ServiceBrokerGuid);
                string sbToLsPasskey = Utilities.GetAppSetting(Consts.STRCFG_SbToLsPasskey);

                Logfile.Write(" ServiceBroker Guid: " + sbGuid);
                Logfile.Write(" Labserver Url: " + labServerUrl);
                Logfile.Write(" SBtoLS Passkey: " + sbToLsPasskey);

                //
                // Create LabServer interface
                //
                labServer = new LabServerWebService();
                labServer.Url = labServerUrl;

                //
                // Create and fill in authorisation information
                //
                AuthHeader authHeader = new AuthHeader();
                authHeader.identifier = sbGuid;
                authHeader.passKey = sbToLsPasskey;
                labServer.AuthHeaderValue = authHeader;
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return labServer;
        }

        //---------------------------------------------------------------------------------------//

        //
        // LabClient to LabServer pass-through web methods
        //

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public bool Cancel(int experimentID)
        {
            bool ok = false;
            try
            {
                LabServerWebService labServer = GetLabServer(sbHeader);
                ok = labServer.Cancel(experimentID);
            }
            catch (Exception)
            {
                throw new Exception("Cancel Failed.");
            }
            return ok;
        }

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public WaitEstimate GetEffectiveQueueLength(string labServerID, int priorityHint)
        {
            LabServerWebService labServer = GetLabServer(sbHeader);
            return labServer.GetEffectiveQueueLength(userGroup, priorityHint);
        }

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public LabExperimentStatus GetExperimentStatus(int experimentID)
        {
            LabServerWebService labServer = GetLabServer(sbHeader);
            return labServer.GetExperimentStatus(experimentID);
        }

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public string GetLabConfiguration(string labServerID)
        {
            LabServerWebService labServer = GetLabServer(sbHeader);
            return labServer.GetLabConfiguration(userGroup);
        }

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public string GetLabInfo(string labServerID)
        {
            LabServerWebService labServer = GetLabServer(sbHeader);
            return labServer.GetLabInfo();
        }

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public LabStatus GetLabStatus(string labServerID)
        {
            const string STRLOG_MethodName = "GetLabStatus";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            LabServerWebService labServer = GetLabServer(sbHeader);
            LabStatus labStatus = labServer.GetLabStatus();

            Logfile.Write(" Online: " + labStatus.online.ToString());
            Logfile.Write(" LabStatus Message: " + labStatus.labStatusMessage);

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return (labStatus);
        }

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public ResultReport RetrieveResult(int experimentID)
        {
            LabServerWebService labServer = GetLabServer(sbHeader);
            return labServer.RetrieveResult(experimentID);
        }

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public SubmissionReport Submit(string labServerID, string experimentSpecification,
            int priorityHint, bool emailNotification)
        {
            LabServerWebService labServer = GetLabServer(sbHeader);
            SubmissionReport submissionReport = labServer.Submit(nextExperimentId, experimentSpecification, userGroup, priorityHint);
            if (submissionReport.vReport.accepted == true)
            {
                // Go to next experiment number
                nextExperimentId = GetNextExperimentID();
            }

            return submissionReport;
        }

        [WebMethod(EnableSession = true)]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public ValidationReport Validate(string labServerID, string experimentSpecification)
        {
            LabServerWebService labServer = GetLabServer(sbHeader);
            return labServer.Validate(experimentSpecification, userGroup);
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("sbHeader", Direction = SoapHeaderDirection.In)]
        public void Notify(int experimentID)
        {
            const string STRLOG_MethodName = "Notify";

            string logMessage = STRLOG_ExperimentId + experimentID.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            bool success = false;

            try
            {
                //
                // Check LabServer's header information is valid
                //
                if (sbHeader == null)
                {
                    throw new ArgumentNullException("sbHeader");
                }
                if (sbHeader.couponPassKey == null)
                {
                    throw new ArgumentNullException("couponPassKey");
                }

                //
                // Determine LabServer
                //
                string strLabServerId = sbHeader.couponID.ToString("X");
                string strCsvLabServer = Utilities.GetAppSetting(strLabServerId);
                string[] strSplit = strCsvLabServer.Split(new char[] { ',' });
                if (strSplit.Length < 3)
                {
                    throw new ArgumentException("CSV LabServer string has insufficient parameters");
                }

                //
                // Check for valid passkey
                //
                if (sbHeader.couponPassKey.Equals(strSplit[0].Trim(), StringComparison.OrdinalIgnoreCase) == false)
                {
                    throw new ArgumentException("couponPassKey is invalid");
                }

                //
                // Create LabServer interface
                //
                string labserverUrl = strSplit[1].Trim();

                LabServerWebService labServer = new LabServerWebService();
                labServer.Url = labserverUrl;

                //
                // Create and fill in authorisation information
                //
                string sbGuid = Utilities.GetAppSetting(Consts.STRCFG_ServiceBrokerGuid);
                string sbToLsPasskey = strSplit[2].Trim();

                AuthHeader authHeader = new AuthHeader();
                authHeader.identifier = sbGuid;
                authHeader.passKey = sbToLsPasskey;
                labServer.AuthHeaderValue = authHeader;

                //
                // Retrieve result from LabServer
                //
                LabExperimentStatus labExperimentStatus = labServer.GetExperimentStatus(experimentID);
                if ((labExperimentStatus != null) && (labExperimentStatus.statusReport != null) &&
                    (labExperimentStatus.statusReport.statusCode >= (int)StatusCodes.Completed) &&
                    (labExperimentStatus.statusReport.statusCode != (int)StatusCodes.Unknown))
                {
                    ResultReport resultReport = labServer.RetrieveResult(experimentID);
                }

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);
        }

        //---------------------------------------------------------------------------------------//

        private int GetNextExperimentID()
        {
            int experimentID = 0;
            FileStream fs = null;
            BinaryWriter bw = null;
            BinaryReader br = null;

            string rootFilePath = HostingEnvironment.ApplicationPhysicalPath;
            string filename = Path.Combine(rootFilePath, STR_ExperimentIdFilename);

            try
            {
                // Initialise next experiment ID
                int nextExperimentID = 1;

                //
                // Check if the next experiment ID file exists
                //
                if (File.Exists(filename))
                {
                    // Create the reader for the next experiment ID
                    fs = new FileStream(filename, FileMode.Open, FileAccess.Read);
                    br = new BinaryReader(fs);

                    // Read the next experiment ID from the file
                    nextExperimentID = br.ReadInt32();

                    // Close the reader
                    br.Close();
                    fs.Close();

                    // Open filestream to write next experiment ID
                    fs = new FileStream(filename, FileMode.Open);
                }
                else
                {
                    // Create filestream to write next experiment ID
                    fs = new FileStream(filename, FileMode.CreateNew);
                }

                // Get next experiment ID and increment
                experimentID = nextExperimentID++;

                // Create the writer for the next experiment ID
                bw = new BinaryWriter(fs);

                // Write the next experiment ID to the file
                bw.Write(nextExperimentID);

                // Close the writer
                bw.Close();
                fs.Close();
            }
            catch (Exception ex)
            {
                Trace.WriteLine(ex.Message);
            }

            return experimentID;
        }

    }
}
