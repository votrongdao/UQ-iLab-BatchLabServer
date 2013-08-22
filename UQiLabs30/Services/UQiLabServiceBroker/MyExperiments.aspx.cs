using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

using iLabs.DataTypes;
using iLabs.DataTypes.StorageTypes;
using iLabs.ServiceBroker.Authorization;
using iLabs.ServiceBroker.DataStorage;
using iLabs.UtilLib;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class MyExperiments : System.Web.UI.Page
    {
        CultureInfo culture;
        int userTZ;
        AuthorizationWrapperClass wrapper = new AuthorizationWrapperClass();
        int userID = -1;
        int groupID = -1;

        //---------------------------------------------------------------------------------------//

        protected void Page_Init(object sender, EventArgs e)
        {
            Master.HeaderTitle = this.Title;
            this.Title = Master.PageTitle + this.Title;
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Session["UserID"] == null)
                Response.Redirect("../login.aspx");

            if (ddlTimeAttribute.SelectedValue != "between")
            {
                txtTime2.ReadOnly = true;
                txtTime2.BackColor = Color.Lavender;
            }
            userTZ = Convert.ToInt32(Session["UserTZ"]);
            culture = DateUtil.ParseCulture(Request.Headers["Accept-Language"]);
            if (Session["UserID"] != null)
            {
                userID = Convert.ToInt32(Session["UserID"]);
            }

            if (Session["GroupID"] != null)
            {
                groupID = Convert.ToInt32(Session["GroupID"]);
            }

            if (!IsPostBack)
            {
                culture = DateUtil.ParseCulture(Request.Headers["Accept-Language"]);
                List<Criterion> cList = new List<Criterion>();
                if (Session["UserID"] != null)
                {
                    cList.Add(new Criterion("User_ID", "=", Session["UserID"].ToString()));

                }
                if (Session["GroupID"] != null)
                {
                    cList.Add(new Criterion("Group_ID", "=", Session["GroupID"].ToString()));
                }
                long[] eIDs = DataStorageAPI.RetrieveAuthorizedExpIDs(userID, groupID, cList.ToArray());
                LongTag[] expTags = DataStorageAPI.RetrieveExperimentTags(eIDs, userTZ, culture, false, false, true, false, true, false, true, false);

                for (int i = 0; i < expTags.Length; i++)
                {
                    //System.Web.UI.WebControls.ListItem item = new System.Web.UI.WebControls.ListItem(eIDs[i].ToString () +" on "+eIDsinfo[i].submissionTime.ToString(),eIDs[i].ToString());
                    System.Web.UI.WebControls.ListItem item = new System.Web.UI.WebControls.ListItem(expTags[i].tag, expTags[i].id.ToString());
                    lbxSelectExperiment.Items.Add(item);
                }

                if (eIDs.Length == 0)
                {
                    string msg = "No experiment records were found for user '" + Session["UserName"] + "' in group '" + Session["GroupName"] + "'.";
                    lblResponse.Text = Utilities.FormatErrorMessage(msg);
                    lblResponse.Visible = true;
                }
                // "Are you sure" javascript for DeleteExperiment button
                //btnDeleteExperiment.Attributes.Add("onclick", "javascript:if(confirm('Are you sure you want to delete this experiment?')== false) return false;");
            }
        }

        protected void ddlTimeAttribute_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            if (ddlTimeAttribute.SelectedValue.ToString().CompareTo("between") == 0)
            {
                txtTime2.Enabled = true;
            }
        }

        protected void clearExperimentDisplay()
        {
            lblResponse.Visible = false;

            // get all criteria in place


            //txtExperimentID.Text = null;
            //txtUsername.Text = null;
            //txtLabServerName.Text = null;
            //txtClientName.Text = null;
            //txtGroupName.Text = null;
            //txtStatus.Text = null;
            //txtSubmissionTime.Text = null;
            //txtCompletionTime.Text = null;
            //txtRecordCount.Text = null;
            //txtAnnotation.Text = null;
            //trSaveAnnotation.Visible = false;
            //trShowExperiment.Visible = false;
            //trDeleteExperiment.Visible = false;
        }

        protected void btnGo_Click(object sender, System.EventArgs e)
        {
            clearExperimentDisplay();
            lbxSelectExperiment.Items.Clear();
            List<Criterion> cList = new List<Criterion>();
            if (Session["UserID"] != null)
            {
                cList.Add(new Criterion("User_ID", "=", Session["UserID"].ToString()));
            }

            if (Session["GroupID"] != null)
            {
                cList.Add(new Criterion("Group_ID", "=", Session["GroupID"].ToString()));
            }

            if ((ddlTimeAttribute.SelectedValue.ToString() != "") && ((txtTime1.Text != null) && (txtTime1.Text != "")))
            {
                DateTime time1 = new DateTime();
                DateTime time2 = new DateTime();

                try
                {
                    time1 = DateUtil.ParseUserToUtc(txtTime1.Text, culture, Convert.ToInt32(Session["UserTZ"]));
                }
                catch
                {
                    lblResponse.Text = Utilities.FormatErrorMessage("Please enter a valid time.");
                    lblResponse.Visible = true;
                    return;
                }
                if ((ddlTimeAttribute.SelectedValue.ToString().CompareTo("between") == 0)
                    || (ddlTimeAttribute.SelectedValue.ToString().CompareTo("on date") == 0))
                {
                    try
                    {
                        time2 = DateUtil.ParseUserToUtc(txtTime2.Text, culture, Convert.ToInt32(Session["UserTZ"]));
                    }
                    catch
                    {
                        lblResponse.Text = Utilities.FormatErrorMessage("Please enter a valid time in the second time field.");
                        lblResponse.Visible = true;
                        return;
                    }
                }
                if (ddlTimeAttribute.SelectedValue.ToString().CompareTo("before") == 0)
                {
                    cList.Add(new Criterion("CreationTime", "<", time1.ToString()));
                }
                else if (ddlTimeAttribute.SelectedValue.ToString().CompareTo("after") == 0)
                {
                    cList.Add(new Criterion("CreationTime", ">=", time1.ToString()));
                }
                else if (ddlTimeAttribute.SelectedValue.ToString().CompareTo("between") == 0)
                {
                    cList.Add(new Criterion("CreationTime", ">=", time1.ToString()));
                    cList.Add(new Criterion("CreationTime", "<", time2.ToString()));
                }
                else if (ddlTimeAttribute.SelectedValue.ToString().CompareTo("on date") == 0)
                {
                    cList.Add(new Criterion("CreationTime", ">=", time1.ToString()));
                    cList.Add(new Criterion("CreationTime", "<", time1.AddDays(1).ToString()));
                }
            }
            // cList.Add(new Criterion("Record_Count", ">", "0"));

            long[] eIDs = DataStorageAPI.RetrieveAuthorizedExpIDs(userID, groupID, cList.ToArray());
            LongTag[] expTags = DataStorageAPI.RetrieveExperimentTags(eIDs, userTZ, culture);

            for (int i = 0; i < expTags.Length; i++)
            {
                System.Web.UI.WebControls.ListItem item = new System.Web.UI.WebControls.ListItem(expTags[i].tag, expTags[i].id.ToString());
                lbxSelectExperiment.Items.Add(item);
            }
            if (eIDs.Length == 0)
            {
                string msg = "No experiment records were found for user '" + Session["UserName"] + "' in group '" + Session["GroupName"] + "'.";
                lblResponse.Text = Utilities.FormatErrorMessage(msg);
                lblResponse.Visible = true;
            }
        }
#if x
        protected void lbxSelectExperiment_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            clearExperimentDisplay();
            long experimentID = Int64.Parse(lbxSelectExperiment.Items[lbxSelectExperiment.SelectedIndex].Value);
            try
            {
                ExperimentSummary[] expInfo = wrapper.GetExperimentSummaryWrapper(new long[] { experimentID });
                if (expInfo[0] != null)
                {
                    txtExperimentID.Text = expInfo[0].experimentId.ToString();
                    txtUsername.Text = expInfo[0].userName;
                    txtGroupName.Text = expInfo[0].groupName;
                    txtLabServerName.Text = expInfo[0].labServerName;
                    txtClientName.Text = expInfo[0].clientName;
                    //Check if update needed from the ESS if one is used
                    if (expInfo[0].essGuid != null)
                    {
                        int expStatus = expInfo[0].status;
                        if ((expStatus == StorageStatus.UNKNOWN || expStatus == StorageStatus.INITIALIZED
                        || expStatus == StorageStatus.OPEN || expStatus == StorageStatus.REOPENED
                        || expStatus == StorageStatus.RUNNING
                        || expStatus == StorageStatus.BATCH_QUEUED || expStatus == StorageStatus.BATCH_RUNNING
                        || expStatus == StorageStatus.BATCH_TERMINATED || expStatus == StorageStatus.BATCH_TERMINATED_ERROR))
                        {

                            // This operation should happen within the Wrapper
                            BrokerDB ticketIssuer = new BrokerDB();
                            ProcessAgentInfo ess = ticketIssuer.GetProcessAgentInfo(expInfo[0].essGuid);
                            if (ess == null || ess.retired)
                            {
                                throw new Exception("The ESS is not registered or is retired");
                            }
                            Coupon opCoupon = ticketIssuer.GetEssOpCoupon(expInfo[0].experimentId, TicketTypes.RETRIEVE_RECORDS, 60, ess.agentGuid);
                            if (opCoupon != null)
                            {
                                ExperimentStorageProxy essProxy = new ExperimentStorageProxy();
                                OperationAuthHeader header = new OperationAuthHeader();
                                header.coupon = opCoupon;
                                essProxy.Url = ess.webServiceUrl;
                                essProxy.OperationAuthHeaderValue = header;

                                StorageStatus curStatus = essProxy.GetExperimentStatus(expInfo[0].experimentId);
                                if (expInfo[0].status != curStatus.status || expInfo[0].recordCount != curStatus.recordCount
                                    || expInfo[0].closeTime != curStatus.closeTime)
                                {
                                    DataStorageAPI.UpdateExperimentStatus(curStatus);
                                    expInfo[0].status = curStatus.status;
                                    expInfo[0].recordCount = curStatus.recordCount;
                                    expInfo[0].closeTime = curStatus.closeTime;
                                }
                            }
                        }

                    }
                    txtStatus.Text = DataStorageAPI.getStatusString(expInfo[0].status);
                    txtSubmissionTime.Text = DateUtil.ToUserTime(expInfo[0].creationTime, culture, userTZ);
                    if ((expInfo[0].closeTime != null) && (expInfo[0].closeTime != DateTime.MinValue))
                    {
                        txtCompletionTime.Text = DateUtil.ToUserTime(expInfo[0].closeTime, culture, userTZ);
                    }
                    else
                    {
                        txtCompletionTime.Text = "Experiment Not Closed!";
                    }
                    txtRecordCount.Text = expInfo[0].recordCount.ToString("    0");
                    txtAnnotation.Text = expInfo[0].annotation;
                    //trSaveAnnotation.Visible = true;
                    //trDeleteExperiment.Visible = true;
                    //trShowExperiment.Visible = (expInfo[0].recordCount > 0);
                }
            }
            catch (Exception ex)
            {
                lblResponse.Text = Utilities.FormatErrorMessage("Error retrieving experiment information. " + ex.Message);
                lblResponse.Visible = true;
            }
        }

        protected void btnSaveAnnotation_Click(object sender, System.EventArgs e)
        {
            lblResponse.Visible = false;
            try
            {
                wrapper.SaveExperimentAnnotationWrapper(Int32.Parse(txtExperimentID.Text), txtAnnotation.Text);

                lblResponse.Text = Utilities.FormatConfirmationMessage("Annotation saved for experiment ID " + txtExperimentID.Text);
                lblResponse.Visible = true;
            }
            catch (Exception ex)
            {
                lblResponse.Text = Utilities.FormatErrorMessage("Error saving experiment annotation. " + ex.Message);
                lblResponse.Visible = true;
            }
        }
#endif
        protected void btnShowExperiment_Click(object sender, System.EventArgs e)
        {
            try
            {
                long experimentID = Int64.Parse(lbxSelectExperiment.Items[lbxSelectExperiment.SelectedIndex].Value);
                ExperimentSummary[] expInfo = wrapper.GetExperimentSummaryWrapper(new long[] { experimentID });
                if (expInfo[0] != null)
                {
                    string strExperimentID = expInfo[0].experimentId.ToString();
                    Response.Redirect("showExperiment.aspx?expid=" + strExperimentID, true);
                }
            }
            catch (Exception ex)
            {
                lblResponse.Text = Utilities.FormatErrorMessage("Error retrieving experiment information. " + ex.Message);
                lblResponse.Visible = true;
            }
        }
#if x
        protected void btnDeleteExperiment_Click(object sender, System.EventArgs e)
        {
            ArrayList aList = new ArrayList();

            try
            {
                lbxSelectExperiment.Items.Clear();
                wrapper.RemoveExperimentsWrapper(new long[] { Convert.ToInt32(txtExperimentID.Text) });
                if (Session["UserID"] != null)
                {
                    aList.Add(new Criterion("User_ID", "=", Session["UserID"].ToString()));
                }

                if (Session["GroupID"] != null)
                {
                    aList.Add(new Criterion("Group_ID", "=", Session["GroupID"].ToString()));
                }

                Criterion[] carray = new Criterion[aList.Count];

                for (int i = 0; i < aList.Count; i++)
                {
                    carray[i] = (Criterion)aList[i];
                }

                long[] eIDs = wrapper.FindExperimentIDsWrapper(carray);
                LongTag[] eTags = DataStorageAPI.RetrieveExperimentTags(eIDs, userTZ, culture);

                for (int i = 0; i < eTags.Length; i++)
                {
                    System.Web.UI.WebControls.ListItem item = new System.Web.UI.WebControls.ListItem(eTags[i].tag, eTags[i].id.ToString());
                    lbxSelectExperiment.Items.Add(item);
                }

                if (eIDs.Length == 0)
                {
                    string msg = "No experiment records were found for user '" + Session["UserName"] + "' in group '" + Session["GroupName"] + "'.";
                    lblResponse.Text = Utilities.FormatErrorMessage(msg);
                    lblResponse.Visible = true;
                }
            }
            catch (Exception ex)
            {
                lblResponse.Text = Utilities.FormatErrorMessage("Error deleting experiment. " + ex.Message);
                lblResponse.Visible = true;
            }
        }
#endif
    }
}
