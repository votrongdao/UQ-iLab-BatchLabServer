using System;
using System.Configuration;
using System.IO;
using System.Web;
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
        public static Library.LabServer.Configuration configuration = null;
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
            configuration = new Library.LabServer.Configuration(rootFilePath);
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

        //---------------------------------------------------------------------------------------//

        public static string FormatRegularURL(HttpRequest httpRequest, string relativePath)
        {
            return FormatURL(httpRequest, relativePath, Consts.STR_RegularProtocol);
        }

        //---------------------------------------------------------------------------------------//

        public static string FormatSecureURL(HttpRequest httpRequest, string relativePath)
        {
            return FormatURL(httpRequest, relativePath, Consts.STR_SecureProtocol);
        }

        //---------------------------------------------------------------------------------------//

        private static string FormatURL(HttpRequest httpRequest, string relativePath, string protocol)
        {
            string serverName = HttpUtility.UrlEncode(httpRequest.ServerVariables["SERVER_NAME"]);
            string serverPort = HttpUtility.UrlEncode(httpRequest.ServerVariables["SERVER_PORT"]);
            string vdirName = httpRequest.ApplicationPath;
            string formattedURL = protocol + "://" + serverName;
            // handle non-conventional ports
            if (serverPort != "80")
            {
                formattedURL += ":" + serverPort;
            }
            if (vdirName.EndsWith("/") == false)
            {
                vdirName += "/";
            }
            formattedURL += vdirName + relativePath;

            return formattedURL;
        }

    }
}