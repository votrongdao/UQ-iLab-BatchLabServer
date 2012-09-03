using System;

namespace Library.LabServerEngine
{
    /// <summary>
    /// Information about the specified experiment in the queue.
    /// </summary>
    public class QueuedExperimentInfo : ExperimentInfo
    {
        /// <summary>
        /// Length of the queue
        /// </summary>
        public int queueLength;

        /// <summary>
        /// Position in the queue, starts at 1
        /// </summary>
        public int position;

        /// <summary>
        /// The time in seconds that this experiment has to wait before it begins execution.
        /// </summary>
        public int waitTime;

        public QueuedExperimentInfo(int experimentId, string sbName)
            : base(experimentId, sbName)
        {
            this.position = 0;
            this.waitTime = 0;
            this.queueLength = 0;
        }

        public QueuedExperimentInfo(int experimentId, string sbName,
            int position, int estExecutionTime, int waitTime, int queueLength)
            : base(experimentId, sbName, null, 0, null, estExecutionTime)
        {
            this.position = position;
            this.waitTime = waitTime;
            this.queueLength = queueLength;
        }
    }
}
