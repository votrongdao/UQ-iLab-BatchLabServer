using System;
using System.ComponentModel;
using System.Xml;
using System.Xml.Serialization;
using System.Web.Services;
using System.Web.Services.Protocols;
using Library.Lab;
using Library.LabEquipment.Engine;

namespace LabEquipment
{
    /// <summary>
    /// AuthHeader - Class that defines the Authentication Header object. For each WebMethod call, an instance of
    /// this class, containing the caller's server ID and passkey will be passed in the header of the SOAP Request.
    /// </summary>
    [XmlType(Namespace = "http://ilab.uq.edu.au/")]
    [XmlRoot(Namespace = "http://ilab.uq.edu.au/", IsNullable = false)]
    public class AuthHeader : SoapHeader
    {
        public string identifier;
        public string passKey;
    }

    //-------------------------------------------------------------------------------------------------//

    /// <summary>
    /// LabEquipmentStatus
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://ilab.uq.edu.au/")]
    public class LabEquipmentStatus
    {
        /// <summary>
        /// True if the LabEquipment is ready to use.
        /// </summary>
        public bool online;

        /// <summary>
        /// Domain-dependent human-readable text describing the status of the LabEquipment.
        /// </summary>
        public string statusMessage;

        public LabEquipmentStatus()
        {
            this.online = false;
            this.statusMessage = null;
        }

        /// <summary>
        /// Create the lab equipment status by specifying all values.
        /// </summary>
        /// <param name="online"></param>
        /// <param name="labStatusMessage"></param>
        public LabEquipmentStatus(bool online, string statusMessage)
        {
            this.online = online;
            this.statusMessage = statusMessage;
        }
    }

    //-------------------------------------------------------------------------------------------------//

    /// <summary>
    /// Summary description for EquipmentService
    /// </summary>
    [WebService(Namespace = "http://ilab.uq.edu.au/")]
    [WebServiceBinding(ConformsTo = WsiProfiles.BasicProfile1_1)]
    [ToolboxItem(false)]
    public class EquipmentService : System.Web.Services.WebService
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "EquipmentService";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_AccessDenied = "Access Denied!";
        private const string STRLOG_UnauthorisedAccess = "Unauthorised Access!";
        protected const string STRLOG_Online = "Online: ";
        protected const string STRLOG_LabStatusMessage = "LabStatusMessage: ";

        //
        // Local variables
        //
        public AuthHeader authHeader;

        #endregion

        //---------------------------------------------------------------------------------------//

        public EquipmentService()
        {
            this.authHeader = new AuthHeader();
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod(Description = "Get the status of the LabEquipment.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public LabEquipmentStatus GetLabEquipmentStatus()
        {
            const string STRLOG_MethodName = "GetLabEquipmentStatus";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            LabEquipmentStatus labEquipmentStatus = null;

            //
            // Check caller access is authorised
            //
            if (Authorised(authHeader) == true)
            {
                //
                // Pass on to the equipment engine
                //
                LabStatus labStatus = Global.equipmentManager.GetLabEquipmentStatus();
                labEquipmentStatus = new LabEquipmentStatus(labStatus.online, labStatus.labStatusMessage);
            }
            else
            {
                labEquipmentStatus = new LabEquipmentStatus(false, STRLOG_AccessDenied);
            }

            string logMessage = STRLOG_Online + labEquipmentStatus.online.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_LabStatusMessage + Logfile.STRLOG_Quote + labEquipmentStatus.statusMessage + Logfile.STRLOG_Quote;

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return labEquipmentStatus;
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod(Description = "Get the time in seconds until the LabEquipment is ready to use.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public int GetTimeUntilReady()
        {
            const string STRLOG_MethodName = "GetTimeUntilReady";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            int timeUntilReady = -1;

            //
            // Check caller access is authorised
            //
            if (Authorised(authHeader) == true)
            {
                // Pass on to the equipment engine
                timeUntilReady = Global.equipmentManager.GetTimeUntilReady();
            }

            string logMessage = timeUntilReady.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return timeUntilReady;
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod(Description = "Suspend the powerdown timeout of the LabEquipment before processing execution requests.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public bool SuspendPowerdown()
        {
            const string STRLOG_MethodName = "SuspendPowerdown";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            //
            // Check caller access is authorised
            //
            if (Authorised(authHeader) == true)
            {
                // Pass on to the equipment engine
                success = Global.equipmentManager.SuspendPowerdown();
            }

            string logMessage = success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod(Description = "Resume the powerdown timeout of the LabEquipment after processing execution requests.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public bool ResumePowerdown()
        {
            const string STRLOG_MethodName = "ResumePowerdown";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            //
            // Check caller access is authorised
            //
            if (Authorised(authHeader) == true)
            {
                // Pass on to the equipment engine
                success = Global.equipmentManager.ResumePowerdown();
            }

            string logMessage = success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //---------------------------------------------------------------------------------------//

        [WebMethod(Description = "Execute a LabEquipment specific request which is provided in XML format." +
            " A response to the request is returned in XML format.")]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public string ExecuteRequest(string xmlRequest)
        {
            const string STRLOG_MethodName = "ExecuteRequest";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, xmlRequest);

            string strXmlResponse;

            //
            // Check caller access is authorised
            //
            if (Authorised(authHeader) == false)
            {
                //
                // Create the XML response
                //
                XmlDocument xmlResponseDocument = new XmlDocument();
                XmlElement xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_Response);
                xmlResponseDocument.AppendChild(xmlElement);

                //
                // Add the error message
                //
                xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspErrorMessage);
                xmlElement.InnerText = STRLOG_AccessDenied;
                xmlResponseDocument.DocumentElement.AppendChild(xmlElement);

                //
                // Add success of command execution
                //
                xmlElement = xmlResponseDocument.CreateElement(Consts.STRXML_RspSuccess);
                xmlElement.InnerText = false.ToString();
                xmlResponseDocument.DocumentElement.AppendChild(xmlElement);

                strXmlResponse = xmlResponseDocument.InnerXml;
            }
            else
            {
                // Pass on to the equipment engine
                strXmlResponse = Global.equipmentManager.ExecuteXmlRequest(xmlRequest);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, strXmlResponse);

            return strXmlResponse;
        }

        //---------------------------------------------------------------------------------------//

        private bool Authorised(AuthHeader authHeader)
        {
            bool ok = true;

            if (Global.allowedCallers.IsAuthenticating == true)
            {
                //
                // Check if caller is specified
                //
                if (authHeader == null || authHeader.identifier == null || authHeader.passKey == null)
                {
                    // Caller is not specified
                    ok = false;
                    Logfile.Write(STRLOG_AccessDenied);
                }

                //
                // Check if access is authorised for this caller
                //
                else
                {
                    // Get the caller's name
                    string lsName = Global.allowedCallers.Authentication(authHeader.identifier, authHeader.passKey);

                    //
                    // Check if caller is allowed access to this web service
                    //
                    if (lsName == null)
                    {
                        // Caller is not allowed access to this Web Method
                        ok = false;
                        Logfile.Write(STRLOG_UnauthorisedAccess);
                    }
                }
            }

            return ok;
        }

    }
}
