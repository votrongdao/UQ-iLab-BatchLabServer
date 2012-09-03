using System;
using System.Web.Services;
using System.Web.Services.Protocols;
using System.Xml.Serialization;

namespace ILabEquipment
{
    [XmlType(Namespace = "http://ilab.uq.edu.au/")]
    [XmlRoot(Namespace = "http://ilab.uq.edu.au/", IsNullable = false)]
    public class AuthHeader : SoapHeader
    {
        public string identifier;
        public string passKey;
    }

    //-------------------------------------------------------------------------------------------------//

    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://ilab.uq.edu.au/")]
    public class LabEquipmentStatus
    {
        public bool online;
        public string statusMessage;
    }

    //-------------------------------------------------------------------------------------------------//

    [WebService(Namespace = "http://ilab.uq.edu.au/")]
    public abstract class EquipmentService : System.Web.Services.WebService
    {
        public AuthHeader authHeader;

        //
        // LabServer to LabEquipment web methods
        //

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract LabEquipmentStatus GetLabEquipmentStatus();

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract int GetTimeUntilReady();

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract bool SuspendPowerdown();

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract bool ResumePowerdown();

        [WebMethod()]
        [SoapHeader("authHeader", Direction = SoapHeaderDirection.In)]
        public abstract string ExecuteRequest(string xmlRequest);

    }
}
