using System;
using System.IO;
using System.Web.Hosting;
using Library.Lab;
using Library.LabClient;

namespace LabClientHtml
{
    public class Global : System.Web.HttpApplication
    {
        private const string STRLOG_ClassName = "Global";

        //---------------------------------------------------------------------------------------//

        protected void Application_Start(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "Application_Start";

            //
            // Set the filepath for the log files 
            //
            string rootFilePath = HostingEnvironment.ApplicationPhysicalPath;
            string logFilesPath = Utilities.GetAppSetting(Consts.STRCFG_LogFilesPath);
            Logfile.SetFilePath(Path.Combine(rootFilePath, logFilesPath));

            Logfile.Write("");
            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            // Nothing to do here

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void Application_Error(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "Application_Error";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // An unhandled error has occured
            //
            Exception ex = Server.GetLastError();
            Logfile.WriteException(ex);

            // Clear the error from the server
            Server.ClearError();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //---------------------------------------------------------------------------------------//

        protected void Application_End(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "Application_End";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            Logfile.Close();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }
    }
}