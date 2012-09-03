using System;
using Library.LabServerEngine;

namespace LabServer
{
    public partial class Administration : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            Master.HeaderTitle = this.Title;
            this.Title = Master.PageTitle + this.Title;
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnDownloadStatistics_Click(object sender, EventArgs e)
        {
            // Set the content type of the file to be downloaded
            Response.ContentType = Consts.StrRsp_ContentType_TextXml;

            // Clear all response headers
            Response.Clear();

            // Add response header
            Response.AddHeader(Consts.StrRsp_Disposition, Consts.StrRsp_Attachment_ExperimentStatisticsXml);

            //
            // Retrieve all experiment results, convert to XML and write out
            //
            ExperimentStatistics experimentStatistics = new ExperimentStatistics();
            string xmlExperimentStatistics = experimentStatistics.RetrieveAllToXml();
            Response.Write(xmlExperimentStatistics);

            // End the http response
            Response.End();
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnDownloadResults_Click(object sender, EventArgs e)
        {
            // Set the content type of the file to be downloaded
            Response.ContentType = Consts.StrRsp_ContentType_TextXml;

            // Clear all response headers
            Response.Clear();

            // Add response header
            Response.AddHeader(Consts.StrRsp_Disposition, Consts.StrRsp_Attachment_ExperimentResultsXml);

            //
            // Retrieve all experiment results, convert to XML and write out
            //
            ExperimentResults experimentResults = new ExperimentResults();
            string xmlExperimentResults = experimentResults.RetrieveAllToXml();
            Response.Write(xmlExperimentResults);

            // End the http response
            Response.End();
        }

        //-------------------------------------------------------------------------------------------------//

        protected void btnDownloadQueue_Click(object sender, EventArgs e)
        {
            // Set the content type of the file to be downloaded
            Response.ContentType = Consts.StrRsp_ContentType_TextXml;

            // Clear all response headers
            Response.Clear();

            // Add response header
            Response.AddHeader(Consts.StrRsp_Disposition, Consts.StrRsp_Attachment_ExperimentQueueXml);

            //
            // Retrieve all queued experiments, convert to XML and write out
            //
            ExperimentQueueDB experimentQueue = new ExperimentQueueDB();
            string xmlExperimentQueue = experimentQueue.RetrieveWaitingToXml();
            Response.Write(xmlExperimentQueue);

            // End the http response
            Response.End();
        }

    }
}
