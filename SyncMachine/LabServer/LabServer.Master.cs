using System;

namespace LabServer
{
    public partial class LabServerMaster : System.Web.UI.MasterPage
    {
        #region Class Constants

        private const string STRLOG_ClassName = "LabServerMaster";

        //
        // String constants
        //
        public const string STR_LabServer = "LabServer";
        public const string STR_Version = "Version ";
        public const string STR_Spacer = " ";

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

        #endregion

        #region Properties

        private static string bannerTitle;
        private static string statusVersion;

        public string Title
        {
            get { return bannerTitle; }
        }

        public string Version
        {
            get { return statusVersion; }
        }

        public string PageTitle
        {
            get { return bannerTitle + " - "; }
        }

        public string HeaderTitle
        {
            get { return Header.Title; }
            set { Header.Title = value; }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        protected void Page_Init(object sender, EventArgs e)
        {
            //
            // This method gets called before any other Page_Init() or Page_Load() methods.
            //

            //
            // Check if initialisation has already been carried out
            //
            if (initialised == false)
            {
                //
                // Get information from the lab configuration
                //
                bannerTitle = Global.configuration.Title + STR_Spacer + STR_LabServer;
                statusVersion = Global.configuration.Version;
                navmenuPhotoUrl = Global.configuration.PhotoUrl;

                initialised = true;
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            
            Banner.Title = bannerTitle;
            Statusbar.Version = STR_Version + statusVersion;
            Navmenu.PhotoUrl = navmenuPhotoUrl;
        }

    }
}
