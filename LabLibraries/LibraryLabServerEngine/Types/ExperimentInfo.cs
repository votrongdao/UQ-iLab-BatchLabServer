using System;
using Library.Lab;

namespace Library.LabServerEngine
{
    /// <summary>
    /// ExperimentInfo
    /// </summary>
    public class ExperimentInfo
    {
        /// <summary>
        /// Queue table entry Id.
        /// </summary>
        public int queueId;

        /// <summary>
        /// Experiment number (greater than zero)
        /// </summary>
        public int experimentId;

        /// <summary>
        /// ServiceBroker's name
        /// </summary>
        public string sbName;

        /// <summary>
        /// User Group for the lab experiment
        /// </summary>
        public string userGroup;

        /// <summary>
        /// Priority of the experiment - unused
        /// </summary>
        public int priorityHint;

        /// <summary>
        /// Experiment specification in XML format
        /// </summary>
        public string xmlSpecification;

        /// <summary>
        /// Experiment specification setup Id
        /// </summary>
        public string setupId;

        /// <summary>
        /// Estimated execution time of the experiment in seconds
        /// </summary>
        public int estExecutionTime;

        /// <summary>
        /// Status of this experiment.
        /// </summary>
        public StatusCodes status;

        /// <summary>
        /// The farm unit number that is executing this experiment.
        /// </summary>
        public int unitId;

        /// <summary>
        /// Flag to indicate if the experiment has been cancelled while waiting on the queue.
        /// </summary>
        public bool cancelled;

        /// <summary>
        /// Information that is provided when an experiment has completed with or without error.
        /// </summary>
        public ResultReport resultReport;

        public ExperimentInfo(int experimentId, string sbName)
        {
            this.experimentId = experimentId;
            this.sbName = sbName;
            this.userGroup = null;
            this.priorityHint = 0;
            this.xmlSpecification = null;
            this.estExecutionTime = -1;
            this.cancelled = false;
        }

        public ExperimentInfo(int experimentId, string sbName,
            string userGroup, int priorityHint, string xmlSpecification, int estExecutionTime)
        {
            this.experimentId = experimentId;
            this.sbName = sbName;
            this.userGroup = userGroup;
            this.priorityHint = priorityHint;
            this.xmlSpecification = xmlSpecification;
            this.estExecutionTime = estExecutionTime;
            this.cancelled = false;
        }
    }

}