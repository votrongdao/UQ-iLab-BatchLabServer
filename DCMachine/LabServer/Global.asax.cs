using System;
using System.IO;
using System.Web.Hosting;
using Library.Lab;
using Library.LabServer;
using Library.LabServerEngine;

namespace LabServer
{
    public class Global : System.Web.HttpApplication
    {
        private const string STRLOG_ClassName = "Global";

        public static AllowedServiceBrokersDB allowedServiceBrokers = null;
        public static Configuration configuration = null;
        public static ExperimentManager experimentManager = null;

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

            //
            // Create the experiment manager
            //
            allowedServiceBrokers = new AllowedServiceBrokersDB();
            configuration = new Configuration(rootFilePath);
            experimentManager = new ExperimentManager(allowedServiceBrokers, configuration);
            experimentManager.Create();

            //
            // Now start the experiment manager
            //
            experimentManager.Start();

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

        //-------------------------------------------------------------------------------------------------//

        protected void Application_End(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "Application_End";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Close experiment manager
            //
            if (experimentManager != null)
            {
                experimentManager.Close();
            }

            //
            // Close logfile class
            //
            Logfile.Close();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }
    }
}