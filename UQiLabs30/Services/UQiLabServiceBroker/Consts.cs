using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;

namespace iLabs.ServiceBroker.iLabSB
{
    public class Consts
    {
        //
        // Config file key strings
        //
        public const string STRCFG_LogFilesPath = "LogFilesPath";
        public const string STRCFG_ServiceBrokerName = "ServiceBrokerName";
        public const string STRCFG_ServiceBrokerUrl = "ServiceBrokerUrl";
        public const string STRCFG_NavmenuPhotoUrl = "NavmenuPhotoUrl";
        public const string STRCFG_FeedbackEmailUrl = "FeedbackEmailUrl";
        public const string STRCFG_ClientFilesPath = "ClientFilesPath";

        public const string STRCFG_RegistrationEmailAddress = "registrationMailAddress";
        public const string STRCFG_AffiliationOptions = "affiliationOptions";
        public const string STRCFG_HaveSSL = "haveSSL";
        public const string STRCFG_RegularProtocol = "regularProtocol";
        public const string STRCFG_SecureProtocol = "secureProtocol";

        public const string STRCFG_ISBAuthCookieName = "ISBAuthCookieName";

        public const char CHR_CsvSplitter = ',';

        //
        // Session variables
        //
        public const string STRSSN_UserName = "UserName";
        public const string STRSSN_GroupName = "GroupName";
        public const string STRSSN_UserID = "UserID";
        public const string STRSSN_GroupID = "GroupID";
        public const string STRSSN_UserTZ = "UserTZ";
        public const string STRSSN_IsAdmin = "IsAdmin";
        public const string STRSSN_IsServiceAdmin = "IsServiceAdmin";
        public const string STRSSN_GroupCount = "GroupCount";
        public const string STRSSN_SessionID = "SessionID";
        public const string STRSSN_ClientID = "ClientID";
        public const string STRSSN_ClientCount = "ClientCount";
        public const string STRSSN_LabClientList = "LabClientList";

        //
        // Webpage URLs
        //
        public const string STRURL_Home = "Home.aspx";
        public const string STRURL_MyGroups = "MyGroups.aspx";
        public const string STRURL_MyLabs = "MyLabs.aspx";
        public const string STRURL_MyClientList = "MyClientList.aspx";
        public const string STRURL_MyClient = "MyClient.aspx";
        public const string STRURL_Register = "Register.aspx";

        //
        // Strings
        //
        public const string STR_LocalhostIP = "127.0.0.1";
    }
}
