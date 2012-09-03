using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;
using Library.Lab;

namespace LabServer
{
    [XmlType(Namespace = "http://ilab.mit.edu")]
    [XmlRoot(Namespace = "http://ilab.mit.edu", IsNullable = false)]
    public class AuthHeader : SoapHeader
    {
        public string identifier;
        public string passKey;
    }

    [WebService(Namespace = "http://ilab.mit.edu")]
    [XmlType(Namespace = "http://ilab.mit.edu")]
    public class LabServerWebService : System.Web.Services.WebService
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "LabServerWebService";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_AccessDenied = "Access Denied!";
        private const string STRLOG_UnauthorisedAccess = "Unauthorised Access!";

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

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public bool Cancel(int experimentID)
        {
            //throw new SoapException("Cancel Failed!", SoapException.ClientFaultCode);
            return false;
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public WaitEstimate GetEffectiveQueueLength(string userGroup, int priorityHint)
        {
            return new WaitEstimate();
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public LabExperimentStatus GetExperimentStatus(int experimentID)
        {
            return new LabExperimentStatus(new ExperimentStatus((int)StatusCodes.Completed));
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public string GetLabConfiguration(string userGroup)
        {
            return "<labConfiguration />";
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public string GetLabInfo()
        {
            return "http://localhost/LabInfo.html";
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public LabStatus GetLabStatus()
        {
            return new LabStatus(true, STRLOG_ClassName);
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public ResultReport RetrieveResult(int experimentID)
        {
            const string STRLOG_MethodName = "RetrieveResult";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Create a ResultReport to return
            //
            ResultReport resultReport = new ResultReport();

            if (experimentID > 0)
            {
                // Update result report
                resultReport.statusCode = (int)StatusCodes.Completed;
                resultReport.experimentResults = "<xmlExperimentResult />";
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);

            return resultReport;
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public SubmissionReport Submit(int experimentID, string experimentSpecification,
            string userGroup, int priorityHint)
        {
            return new SubmissionReport(experimentID);
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public ValidationReport Validate(string experimentSpecification, string userGroup)
        {
            return new ValidationReport();
        }

    }
}
