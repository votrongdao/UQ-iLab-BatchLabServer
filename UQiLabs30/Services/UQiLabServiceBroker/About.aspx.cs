using System;

namespace iLabs.ServiceBroker.iLabSB
{
    public partial class About : System.Web.UI.Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            Master.HeaderTitle = this.Title;
            this.Title = Master.PageTitle + this.Title;
        }

        //---------------------------------------------------------------------------------------//

        protected void Page_Load(object sender, EventArgs e)
        {

        }
    }
}
