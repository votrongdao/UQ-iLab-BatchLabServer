using System;

namespace Library.LabServerEngine
{
    public class AppData
    {
        public AllowedServiceBrokersDB allowedServiceBrokers;
        public ExperimentQueueDB experimentQueue;
        public ExperimentResults experimentResults;
        public ExperimentStatistics experimentStatistics;
        public LabConfiguration labConfiguration;
        public int farmSize;
        public string emailAddressLabServer;
        public string[] emailAddressesExperimentCompleted;
        public string[] emailAddressesExperimentFailed;
        public LabExperimentEngine[] labExperimentEngines;
        public Object signalCompleted;
    }
}
