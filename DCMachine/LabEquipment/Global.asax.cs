using System;
using System.IO;
using System.Web.Hosting;
using Library.Lab;
using Library.LabEquipment;
using Library.LabEquipment.Engine;

namespace LabEquipment
{
    public class Global : System.Web.HttpApplication
    {
        private const string STRLOG_ClassName = "Global";

        public static AllowedCallers allowedCallers = null;
        public static EquipmentManager equipmentManager = null;

        //-------------------------------------------------------------------------------------------------//

        protected void Application_Start(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "Application_Start";

            //
            // Set the filepath for the log files and the logging level
            //
            string rootFilePath = HostingEnvironment.ApplicationPhysicalPath;
            string logFilesPath = Utilities.GetAppSetting(Library.LabEquipment.Engine.Consts.STRCFG_LogFilesPath);
            Logfile.SetFilePath(Path.Combine(rootFilePath, logFilesPath));

            //
            // Determine the logging level for this application
            //
            try
            {
                Logfile.LoggingLevels logLevel = (Logfile.LoggingLevels)Utilities.GetIntAppSetting(Library.LabEquipment.Engine.Consts.STRCFG_LoggingLevel);
                Logfile.SetLoggingLevel(logLevel);
            }
            catch
            {
                Logfile.SetLoggingLevel(Logfile.LoggingLevels.Minimum);
            }

            Logfile.Write(string.Empty);
            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Create experiment manager and start it
            //
            allowedCallers = new AllowedCallers();
            equipmentManager = new EquipmentManager(rootFilePath);
            equipmentManager.Create();
            equipmentManager.Start();

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
            // Close equipment manager
            //
            if (equipmentManager != null)
            {
                equipmentManager.Close();
            }

            //
            // Close logfile class
            //
            Logfile.Close();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }
    }
}