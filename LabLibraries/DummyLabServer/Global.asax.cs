using System;
using System.IO;
using System.Web.Hosting;
using Library.Lab;
using Library.LabServerEngine;

namespace LabServer
{
    public class Global : System.Web.HttpApplication
    {
        private const string STRLOG_ClassName = "Global";

        //-------------------------------------------------------------------------------------------------//

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

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void Application_Error(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "Application_Error";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            // Need to do something here

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        protected void Application_End(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "Application_End";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Close logfile class
            //
            Logfile.Close();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }
    }
}