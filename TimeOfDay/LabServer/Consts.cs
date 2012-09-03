using System;

namespace LabServer
{
    public class Consts
    {
        //
        // Application configuration file key strings
        //
        public const string STRCFG_LogFilesPath = "LogFilesPath";
        public const string STRCFG_HaveSSL = "haveSSL";

        //
        // String constants
        //
        public const string STR_RegularProtocol = "http";
        public const string STR_SecureProtocol = "https";
        public const string STR_GroupName_Admin = "Admin";
        public const string STR_GroupName_User = "User";

        //
        // ServiceBroker name to use when caller authentication is set to false. Used when
        // developing and running the LabServer
        public const string STR_SbNameLocalHost = "localhost";

        //
        // Session variables
        //
        public const string STRSSN_UserId = "LS_UserId";
        public const string STRSSN_UserName = "LS_UserName";
        public const string STRSSN_GroupName = "LS_GroupName";

        //
        // Webpage URLs
        //
        public const string STRURL_Home = "Home.aspx";
        public const string STRURL_MyAccount = "MyAccount.aspx";

        //
        // Results XML download response
        //
        public const string StrRsp_ContentType_TextXml = "text/xml";
        public const string StrRsp_Disposition = "content-disposition";
        public const string StrRsp_Attachment_ExperimentQueueXml = "attachment; filename=\"ExperimentQueue.xml\"";
        public const string StrRsp_Attachment_ExperimentResultsXml = "attachment; filename=\"ExperimentResults.xml\"";
        public const string StrRsp_Attachment_ExperimentStatisticsXml = "attachment; filename=\"ExperimentStatistics.xml\"";

    }
}
