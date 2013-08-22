using System;
using System.Configuration;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Hosting;
using System.Web.Security;

using iLabs.ServiceBroker.Administration;
using iLabs.ServiceBroker.Authorization;
using iLabs.ServiceBroker.Internal;
using Library.Lab;

namespace iLabs.ServiceBroker.iLabSB
{
    public class Global : System.Web.HttpApplication
    {
        private const string STRLOG_ClassName = "Global";

        //private SBTicketRemover ticketRemover;

        //---------------------------------------------------------------------------------------//

        protected void Application_Start(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "Application_Start";

            //
            // Set the filepath for the log files 
            //
            string rootFilePath = HostingEnvironment.ApplicationPhysicalPath;
            string logFilesPath = Library.Lab.Utilities.GetAppSetting(Consts.STRCFG_LogFilesPath);
            Logfile.SetFilePath(Path.Combine(rootFilePath, logFilesPath));

            Logfile.Write("");
            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            string path = ConfigurationManager.AppSettings["logPath"];
            if (path != null && path.Length > 0)
            {
                iLabs.UtilLib.Utilities.LogPath = path;
                iLabs.UtilLib.Utilities.WriteLog("");
                iLabs.UtilLib.Utilities.WriteLog("#############################################################################");
                iLabs.UtilLib.Utilities.WriteLog("");
                iLabs.UtilLib.Utilities.WriteLog("ISB Application_Start: starting");
            }
            // The AuthCache class is defined in the Authorization
            AuthCache.GrantSet = InternalAuthorizationDB.RetrieveGrants();
            AuthCache.QualifierSet = InternalAuthorizationDB.RetrieveQualifiers();
            AuthCache.QualifierHierarchySet = InternalAuthorizationDB.RetrieveQualifierHierarchy();
            AuthCache.AgentHierarchySet = InternalAuthorizationDB.RetrieveAgentHierarchy();
            AuthCache.AgentsSet = InternalAuthorizationDB.RetrieveAgents();
            //ticketRemover = new SBTicketRemover();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //---------------------------------------------------------------------------------------//

        protected void Session_Start(object sender, EventArgs e)
        {
            object obj = Request;
            // Check for cookie added to Applet page
            HttpCookie cookie = Request.Cookies[ConfigurationManager.AppSettings["isbAuthCookieName"]];
            if (cookie != null)
            {
                object cValue = cookie.Value;
                if (cValue != null)
                {
                    long sesID = Convert.ToInt64(cValue);
                    SessionInfo info = AdministrativeAPI.GetSessionInfo(sesID);
                    if (info != null)
                    {
                        AdministrativeAPI.SetSessionKey(sesID, Session.SessionID);
                        Session["SessionID"] = sesID;
                        Session["UserID"] = info.userID;
                        int[] myGrps = AdministrativeAPI.ListNonRequestGroupsForAgent(info.userID);
                        if (myGrps != null)
                            Session["GroupCount"] = myGrps.Length;
                        Session["UserName"] = info.userName;
                        if (info.clientID > 0)
                        {
                            Session["ClientID"] = info.clientID;
                        }
                        else
                        {
                            Session.Remove("ClientID");
                        }
                        Session["UserTZ"] = info.tzOffset;
                        Session["IsAdmin"] = false;
                        Session["IsServiceAdmin"] = false;
                        Group[] grps = null;
                        if (info.groupID > 0)
                        {
                            grps = AdministrativeAPI.GetGroups(new int[] { info.groupID });
                        }
                        if (grps != null && grps.Length == 1)
                        {
                            Session["GroupID"] = info.groupID;
                            Session["GroupName"] = grps[0].groupName;
                            if ((grps[0].groupName.Equals(Group.SUPERUSER)) || (grps[0].groupType.Equals(GroupType.COURSE_STAFF)))
                            {
                                Session["IsAdmin"] = true;
                            }
                            // if the effective group is a service admin group, then redirect to the service admin page.
                            // the session variable is used in the userNav page to check whether to make the corresponing tab visible
                            else if (grps[0].groupType.Equals(GroupType.SERVICE_ADMIN))
                            {
                                Session["IsServiceAdmin"] = true;
                            }
                        }
                        else
                        {
                            Session.Remove("GroupID");
                            Session.Remove("GroupName");
                            Response.Redirect("myGroups.aspx");
                        }
                    }
                }
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void Application_BeginRequest(object sender, EventArgs e)
        {
            if (Request.Path.IndexOf('\\') >= 0 ||
                System.IO.Path.GetFullPath(Request.PhysicalPath) != Request.PhysicalPath)
            {
                throw new HttpException(404, "not found");
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void Application_AuthenticateRequest(object sender, EventArgs e)
        {

        }

        //---------------------------------------------------------------------------------------//

        protected void Application_Error(object sender, EventArgs e)
        {
            Exception ex = Server.GetLastError();
            if (ex is HttpUnhandledException)
            {
                EventLog.WriteEntry(this.Application.ToString(), ex.Message, EventLogEntryType.Error);

                Server.Transfer("reportBug.aspx?ex=true");
            }
        }

        //---------------------------------------------------------------------------------------//

        protected void Session_End(object sender, EventArgs e)
        {
            AdministrativeAPI.SaveUserSessionEndTime(Convert.ToInt64(Session["SessionID"]));
            Session.RemoveAll();
            FormsAuthentication.SignOut();
        }

        //---------------------------------------------------------------------------------------//

        protected void Application_End(object sender, EventArgs e)
        {
            const string STRLOG_MethodName = "Application_End";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //if (ticketRemover != null)
            //{
            //    ticketRemover.Stop();
            //}
            iLabs.UtilLib.Utilities.WriteLog("ISB Application_End:");

            HttpRuntime runtime = (HttpRuntime)typeof(System.Web.HttpRuntime).InvokeMember(
                "_theRuntime",
                BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.GetField,
                null, null, null);
            if (runtime == null)
            {
                return;
            }

            string shutDownMessage = (string)runtime.GetType().InvokeMember(
                "_shutDownMessage",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, runtime, null);

            string shutDownStack = (string)runtime.GetType().InvokeMember(
                "_shutDownStack",
                BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetField,
                null, runtime, null);

            if (!EventLog.SourceExists(".NET Runtime"))
            {
                EventLog.CreateEventSource(".NET Runtime", "Application");
            }
            EventLog log = new EventLog();
            log.Source = ".NET Runtime";
            log.WriteEntry(String.Format("\r\n\r\n_shutDownMessage={0}\r\n\r\n_shutDownStack={1}",
                                         shutDownMessage,
                                         shutDownStack),
                                         EventLogEntryType.Error);
            //
            // Close logfile class
            //
            Logfile.Close();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        //---------------------------------------------------------------------------------------//

        public static string FormatRegularURL(HttpRequest httpRequest, string relativePath)
        {
            return FormatURL(httpRequest, relativePath, Consts.STRCFG_RegularProtocol);
        }

        //---------------------------------------------------------------------------------------//

        public static string FormatSecureURL(HttpRequest httpRequest, string relativePath)
        {
            return FormatURL(httpRequest, relativePath, Consts.STRCFG_SecureProtocol);
        }

        //---------------------------------------------------------------------------------------//

        private static string FormatURL(HttpRequest httpRequest, string relativePath, string protocolType)
        {
            string protocol = ConfigurationManager.AppSettings[protocolType];
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