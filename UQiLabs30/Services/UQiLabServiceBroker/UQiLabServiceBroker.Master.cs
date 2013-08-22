using System;
using System.Configuration;
using System.Reflection;
using Library.Lab;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class UQiLabServiceBroker : System.Web.UI.MasterPage
    {
        #region Class Constants

        private const string STRLOG_ClassName = "UQiLabServiceBroker";

        //
        // String constants
        //
        public const string STR_Initialising = " Initialising...";
        public const string STR_User = "User: ";
        public const string STR_Group = "Group: ";
        public const string STR_Timezone = "Timezone: GMT";
        public const string STR_Version = "Version ";

        //
        // String constants for logfile messages
        //
        public const string STRLOG_Title = " Title: ";
        public const string STRLOG_Version = " Version: ";
        public const string STRLOG_PhotoUrl = " Photo Url: ";

        //
        // Local variables
        //
        private static bool initialised = false;
        private static string navmenuPhotoUrl;
        private static string feedbackEmailUrl;
        private static string registrationEmail;

        #endregion

        #region Properties

        private static string serviceBrokerName;
        private static string statusbarVersion;
        private static string serviceBrokerUrl;

        public string Title
        {
            get { return serviceBrokerName; }
        }

        public string Version
        {
            get { return statusbarVersion; }
        }

        public string PageTitle
        {
            get { return serviceBrokerName + " - "; }
        }

        public string HeaderTitle
        {
            get { return Header.Title; }
            set { Header.Title = value; }
        }

        public string ServiceBrokerUrl
        {
            get { return serviceBrokerUrl; }
        }

        public string RegistrationEmail
        {
            get { return registrationEmail; }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        protected void Page_Init(object sender, EventArgs e)
        {
            //
            // Check if initialisation has already been carried out
            //
            if (initialised == false)
            {
                //
                // Carry out one-time initialisation for all UQiLabServiceBroker instances
                //

                serviceBrokerName = Utilities.GetAppSetting(Consts.STRCFG_ServiceBrokerName);
                serviceBrokerUrl = Utilities.GetAppSetting(Consts.STRCFG_ServiceBrokerUrl);
                navmenuPhotoUrl = Utilities.GetAppSetting(Consts.STRCFG_NavmenuPhotoUrl);
                feedbackEmailUrl = Utilities.GetAppSetting(Consts.STRCFG_FeedbackEmailUrl);
                registrationEmail = Utilities.GetAppSetting(Consts.STRCFG_RegistrationEmailAddress);

                //
                // Get version number from AssemblyInfo
                //
                Assembly assembly = Assembly.GetExecutingAssembly();
                string assembly_FullName = assembly.FullName;
                AssemblyName assemblyName = assembly.GetName();
                statusbarVersion = assemblyName.Version.Major + "." + assemblyName.Version.Minor + "." + assemblyName.Version.Revision;

                initialised = true;
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            Banner.Title = serviceBrokerName;
            Statusbar.Version = STR_Version + statusbarVersion;
            Navmenu.PhotoUrl = navmenuPhotoUrl;
            Feedback.MailtoUrl = feedbackEmailUrl;

            //
            // Get User Name
            //
            object username = Session[Consts.STRSSN_UserName];
            if (username != null)
            {
                Statusbar.Username = STR_User + username.ToString();
            }
            else
            {
                Statusbar.Username = string.Empty;
            }

            //
            // Get Group Name
            //
            object groupname = Session[Consts.STRSSN_GroupName];
            if (groupname != null)
            {
                Statusbar.Groupname = STR_Group + groupname.ToString();
            }
            else
            {
                Statusbar.Groupname = string.Empty;
            }

            //
            // Get Timezone GMT +/- ?? hrs:mins
            //
            object timezone = Session[Consts.STRSSN_UserTZ];
            if (timezone != null)
            {
                try
                {
                    int totalMinutes = Convert.ToInt32(timezone.ToString());
                    int hours = totalMinutes / 60;
                    int minutes = totalMinutes % 60;
                    string strTimezone = ((totalMinutes >= 0) ? "+" : "") + hours.ToString();
                    strTimezone += ":" + minutes.ToString("D2");
                    Feedback.Timezone = STR_Timezone + strTimezone;
                }
                catch
                {
                    Feedback.Timezone = string.Empty;
                }
            }
            else
            {
                Feedback.Timezone = string.Empty;
            }
        }
    }
}
