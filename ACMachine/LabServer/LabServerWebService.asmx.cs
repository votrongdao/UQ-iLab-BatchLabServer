using System;
using System.ComponentModel;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;
using Library.Lab;
using Library.LabServerEngine;

namespace LabServer
{
    /// <summary>
    /// AuthHeader - Class that defines the Authentication Header object. For each WebMethod call, an instance of
    /// this class, containing the caller's server ID and passkey will be passed in the header of the SOAP Request.
    /// </summary>
    [XmlType(Namespace = "http://ilab.mit.edu")]
    [XmlRoot(Namespace = "http://ilab.mit.edu", IsNullable = false)]
    public class AuthHeader : SoapHeader
    {
        public string identifier;
        public string passKey;
    }

    /// <summary>
    /// LabServerWebService - This is the web service interface to the LabServer from the
    /// ServiceBroker. Using the original ServiceBroker61 which runs under ASP.NET 1.1 to
    /// postpone updating the ServiceBroker to ASP.NET 2.0. The namespace shown here must
    /// be exactly "http://ilab.mit.edu" with no trailing forward slash.
    /// </summary>
    [WebService(Namespace = "http://ilab.mit.edu")]
    [XmlType(Namespace = "http://ilab.mit.edu")]
    [ToolboxItem(false)]
    public class LabServerWebService : System.Web.Services.WebService
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "LabServerWebService";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_AccessDenied = " Access Denied!";
        private const string STRLOG_UnauthorisedAccess = " Unauthorised Access!";
        private const string STRLOG_experimentID = " experimentID: ";
        private const string STRLOG_sbName = " sbName: ";
        private const string STRLOG_online = " online: ";
        private const string STRLOG_labStatusMessage = " labStatusMessage: ";
        private const string STRLOG_IsRunning = " Is Running";
        private const string STRLOG_IsQueued = " Is Queued";
        private const string STRLOG_statusCode = " statusCode: ";
        private const string STRLOG_experimentResults = " experimentResults: ";
        private const string STRLOG_errorMessage = " errorMessage: ";

        //
        // Local variables
        //
        public AuthHeader authHeader;

        #endregion

        //---------------------------------------------------------------------------------------//

        public LabServerWebService()
        {
            authHeader = new AuthHeader();
        }

        //---------------------------------------------------------------------------------------//

        #region WebMethod Cancel

        /// <summary>
        /// 
        /// </summary>
        /// <param name="experimentID">A number &gt; 0 that identifies the experiment.</param>
        /// <returns></returns>
        [WebMethod(Description = "Batch - Cancels a previously submitted experiment. " +
            "If the experiment is already running, makes best efforts to abort execution, " +
            "but there is no guarantee that the experiment will not run to completion.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public bool Cancel(int experimentID)
        // experimentID - A token identifying the experiment returned from a previous call to Submit().
        // Returns: True if the experiment was successfully removed from the queue (before execution
        //      had begun). If false, the user may want to call GetExperimentStatus() for more detailed
        //      information.
        {
            const string STRLOG_MethodName = "Cancel";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool cancelled = false;

            // Get the identity of the caller
            string sbName = GetCallerName(authHeader);

            // Check caller access is authorised
            if (sbName != null)
            {
                // Pass on to experiment manager
                cancelled = Global.experimentManager.Cancel(experimentID, sbName);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return (cancelled);
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        #region WebMethod GetEffectiveQueueLength

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userGroup">The group of the user making the request.</param>
        /// <param name="priorityHint">The priority of the user within the group. Possible values
        /// range from 20 (highest priority) to -20 (lowest priority); 0 is normal. Priority hints
        /// may or may not be considered by the LabServer.</param>
        /// <returns>WaitEstimate</returns>
        [WebMethod(Description = "Batch - Checks the effective queue length of the Lab Server. " +
            "Answers the question: How many experiments currently in the execution queue " +
            "and how long to wait before running the new experiment?")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public WaitEstimate GetEffectiveQueueLength(string userGroup, int priorityHint)
        {
            const string STRLOG_MethodName = "GetEffectiveQueueLength";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            WaitEstimate waitEstimate = null;

            // Get the identity of the caller
            string sbName = GetCallerName(authHeader);

            // Check caller access is authorised
            if (sbName != null)
            {
                // Pass on to experiment manager
                waitEstimate = Global.experimentManager.GetEffectiveQueueLength(userGroup, priorityHint);
            }
            else
            {
                waitEstimate = new WaitEstimate();
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return waitEstimate;
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        #region WebMethod GetExperimentStatus

        /// <summary>
        /// 
        /// </summary>
        /// <param name="experimentID">A number &gt; 0 that identifies the experiment.</param>
        /// <returns></returns>
        [WebMethod(Description = "Batch - Get the status of an experiment. The experiment may be " +
            "currently running, waiting to run, have completed or may not even exist.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public LabExperimentStatus GetExperimentStatus(int experimentID)
        {
            const string STRLOG_MethodName = "GetExperimentStatus";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            LabExperimentStatus labExperimentStatus = null;

            // Get the identity of the caller
            string sbName = GetCallerName(authHeader);

            // Check caller access is authorised
            if (sbName != null)
            {
                // Pass on to experiment manager
                labExperimentStatus = Global.experimentManager.GetExperimentStatus(experimentID, sbName);
            }
            else
            {
                labExperimentStatus = new LabExperimentStatus(
                    new ExperimentStatus((int)StatusCodes.Unknown));
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return labExperimentStatus;
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        #region WebMethod GetLabConfiguration

        /// <summary>
        /// userGroup - The group of the user making the request.
        /// </summary>
        /// <param name="userGroup"></param>
        /// <returns>An opaque, domain-dependent lab configuration.</returns>
        [WebMethod(Description = "Batch - Gets the configuration of the Lab Server.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public string GetLabConfiguration(string userGroup)
        {
            const string STRLOG_MethodName = "GetLabConfiguration";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            string xmlLabConfiguration = null;

            // Get the identity of the caller
            string sbName = GetCallerName(authHeader);

            // Check caller access is authorised
            if (sbName != null)
            {
                // Pass on to experiment manager
                xmlLabConfiguration = Global.experimentManager.GetLabConfiguration(userGroup);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return xmlLabConfiguration;
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        #region WebMethod GetLabInfo

        /// <summary>
        /// Get the URL to a lab-specific information resource, e.g. a lab information page.
        /// </summary>
        /// <returns></returns>
        [WebMethod(Description = "Batch - Get the URL containing information about the Lab Server.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public string GetLabInfo()
        {
            const string STRLOG_MethodName = "GetLabInfo";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            string labInfo = null;

            // Get the identity of the caller
            string sbName = GetCallerName(authHeader);

            // Check caller access is authorised
            if (sbName != null)
            {
                // Pass on to experiment manager
                labInfo = Global.experimentManager.GetLabInfo();
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return labInfo;
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        #region WebMethod GetLabStatus

        /// <summary>
        /// 
        /// </summary>
        /// <returns>LabStatus</returns>
        [WebMethod(Description = "Batch - Check the status of the LabServer.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public LabStatus GetLabStatus()
        {
            const string STRLOG_MethodName = "GetLabStatus";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            LabStatus labStatus = null;

            // Get the identity of the caller
            string sbName = GetCallerName(authHeader);

            // Check caller access is authorised
            if (sbName != null)
            {
                // Pass on to experiment manager
                labStatus = Global.experimentManager.GetLabStatus();
            }
            else
            {
                labStatus = new LabStatus(false, STRLOG_AccessDenied);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
            
            return (labStatus);
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        #region WebMethod RetrieveResult

        /// <summary>
        /// 
        /// </summary>
        /// <param name="experimentID">A number &gt; 0 that identifies the experiment.</param>
        /// <returns>ResultReport</returns>
        [WebMethod(Description = "Batch - Retrieves the results from (or errors generated by) " +
            "a previously submitted experiment.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public ResultReport RetrieveResult(int experimentID)
        {
            const string STRLOG_MethodName = "RetrieveResult";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            ResultReport resultReport = null;

            // Get the identity of the caller
            string sbName = GetCallerName(authHeader);

            // Check caller access is authorised
            if (sbName != null)
            {
                // Pass on to experiment manager
                resultReport = Global.experimentManager.RetrieveResult(experimentID, sbName);
            }
            else
            {
                resultReport = new ResultReport((int)StatusCodes.Failed, STRLOG_AccessDenied);
            }

            string logMessage = STRLOG_statusCode + ((StatusCodes)resultReport.statusCode).ToString();
            if ((StatusCodes)resultReport.statusCode == StatusCodes.Completed)
            {
                logMessage += Logfile.STRLOG_Spacer + STRLOG_experimentResults + resultReport.experimentResults;
            }
            else
            {
                logMessage += Logfile.STRLOG_Spacer + STRLOG_errorMessage + resultReport.errorMessage;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return resultReport;
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        #region WebMethod Submit

        /// <summary>
        /// 
        /// </summary>
        /// <param name="experimentID">A number &gt; 0 that identifies the experiment.</param>
        /// <param name="experimentSpecification">An opaque, domain-dependent experiment specification.</param>
        /// <param name="userGroup">Effective group of the user submitting this experiment.</param>
        /// <param name="priorityHint">Indicates a requested priority for this experiment.
        /// Possible values range from 20 (highest priority) to -20 (lowest priority); 0 is normal.
        /// Priority hints may or may not be considered by the lab server.</param>
        /// <returns>SubmissionReport</returns>
        [WebMethod(Description = "Batch - Submits an experiment specification to the LabServer for execution.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public SubmissionReport Submit(int experimentID, string experimentSpecification, string userGroup, int priorityHint)
        {
            const string STRLOG_MethodName = "Submit";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            SubmissionReport submissionReport = null;

            // Get the identity of the caller
            string sbName = GetCallerName(authHeader);

            // Check caller access is authorised
            if (sbName != null)
            {
                // Pass on to experiment manager
                submissionReport = Global.experimentManager.Submit(experimentID, sbName,
                    experimentSpecification, userGroup, priorityHint);
            }
            else
            {
                submissionReport = new SubmissionReport(experimentID);
                submissionReport.vReport.errorMessage = STRLOG_AccessDenied;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return submissionReport;
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        #region WebMethod Validate

        /// <summary>
        /// 
        /// </summary>
        /// <param name="experimentSpecification">An opaque, domain-dependent experiment specification.</param>
        /// <param name="userGroup">Effective group of the user submitting this experiment.</param>
        /// <returns>ValidationReport</returns>
        [WebMethod(Description = "Batch - Checks whether an experiment specification would be accepted " +
            "if submitted for execution.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public ValidationReport Validate(string experimentSpecification, string userGroup)
        {
            const string STRLOG_MethodName = "Validate";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            ValidationReport validationReport = null;

            // Get the identity of the caller
            string sbName = GetCallerName(authHeader);

            // Check caller access is authorised
            if (sbName != null)
            {
                // Pass on to experiment manager
                validationReport = Global.experimentManager.Validate(experimentSpecification, userGroup);
            }
            else
            {
                validationReport = new ValidationReport(false);
                validationReport.errorMessage = STRLOG_AccessDenied;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return (validationReport);
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        private string GetCallerName(AuthHeader authHeader)
        {
            string sbName = null;

            //
            // Check if caller is specified
            //
            if (authHeader == null || authHeader.identifier == null || authHeader.passKey == null)
            {
                //
                // Check if authenticating
                //
                if (Global.allowedServiceBrokers.IsAuthenticating == false)
                {
                    // Set string to LocalHost
                    sbName = Consts.STR_SbNameLocalHost;
                }
                else
                {
                    // Caller is not specified
                    Logfile.Write(STRLOG_AccessDenied);
                }
            }
            else
            {
                // Get the caller's name
                sbName = Global.allowedServiceBrokers.Authentication(authHeader.identifier, authHeader.passKey);

                //
                // Check if caller is allowed access to this web service
                //
                if (sbName == null)
                {
                    // Caller is not allowed access to this Web Method
                    Logfile.Write(STRLOG_UnauthorisedAccess);
                }
            }

            return sbName;
        }

    }
}
