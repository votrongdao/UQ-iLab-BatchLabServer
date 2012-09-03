using System;
using Library.Lab;
using Library.LabServerEngine;

namespace Library.LabServer
{
    public class ExperimentManager : LabExperimentManager
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "ExperimentManager";

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public ExperimentManager(AllowedServiceBrokersDB allowedServiceBrokers, Configuration configuration)
            : this(allowedServiceBrokers, configuration, 0)
        {
        }

        //-------------------------------------------------------------------------------------------------//

        public ExperimentManager(AllowedServiceBrokersDB allowedServiceBrokers, Configuration configuration, int farmSize)
            : base(allowedServiceBrokers, configuration, farmSize)
        {
        }

        //-------------------------------------------------------------------------------------------------//

        public override void Create()
        {
            const string STRLOG_MethodName = "Create";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            //
            // Create local class instances just to check that all is in order
            //
            Configuration configuration = (Configuration)this.appData.labConfiguration;
            Specification specification = new Specification(configuration, null);
            ExperimentResult experimentResult = new ExperimentResult(configuration);

            //
            // Create instances of the experiment engines
            //
            this.appData.labExperimentEngines = new ExperimentEngine[appData.farmSize];
            for (int i = 0; i < appData.farmSize; i++)
            {
                this.appData.labExperimentEngines[i] = new ExperimentEngine(i, this.appData);
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

    }
}
