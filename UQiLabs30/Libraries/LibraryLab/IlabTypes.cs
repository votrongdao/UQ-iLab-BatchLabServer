using System;
using System.Xml.Serialization;

namespace Library.Lab
{
    public enum StatusCodes
    {
        Ready = 0,        // Ready to execute
        Waiting = 1,      // Waiting in the execution queue
        Running = 2,      // Currently running
        Completed = 3,    // Completely normally
        Failed = 4,       // Terminated with errors
        Cancelled = 5,    // Cancelled by user before execution had begun
        Unknown = 6       // Unknown experimentID
    }

    //-------------------------------------------------------------------------------------------------//

    /// <summary>
    /// ExperimentStatus
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    public class ExperimentStatus
    {
        /// <summary>
        /// Indicates the status of this experiment.
        /// </summary>
        public int statusCode;

        /// <summary>
        /// 
        /// </summary>
        public WaitEstimate wait;

        /// <summary>
        /// Estimated runtime (in seconds) of this experiment. [OPTIONAL, &lt; 0 if not used].
        /// </summary>
        public double estRuntime;

        /// <summary>
        /// Estimated remaining run time (in seconds) of this experiment, if the experiment is
        /// currently running. [OPTIONAL, &lt; 0 if not used].
        /// </summary>
        public double estRemainingRuntime;

        public ExperimentStatus()
        {
            this.statusCode = (int)StatusCodes.Unknown;
            this.wait = new WaitEstimate();
            this.estRuntime = 0.0;
            this.estRemainingRuntime = 0.0;
        }

        public ExperimentStatus(int statusCode)
        {
            this.statusCode = statusCode;
            this.wait = new WaitEstimate();
            this.estRuntime = 0.0;
            this.estRemainingRuntime = 0.0;
        }

        public ExperimentStatus(int statusCode, WaitEstimate wait, double estRuntime,
            double estRemainingRuntime)
        {
            this.statusCode = statusCode;
            this.wait = wait;
            this.estRuntime = estRuntime;
            this.estRemainingRuntime = estRemainingRuntime;
        }
    }

    //-------------------------------------------------------------------------------------------------//

    /// <summary>
    /// LabExperimentStatus
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    public class LabExperimentStatus
    {
        public ExperimentStatus statusReport;
        // See description above.

        /// <summary>
        /// Guaranteed minimum remaining time (in seconds) before this experimentID and
        /// associated data will be purged from the LabServer.
        /// </summary>
        public double minTimetoLive;

        public LabExperimentStatus()
        {
            this.statusReport = new ExperimentStatus();
            this.minTimetoLive = 0.0;
        }

        public LabExperimentStatus(ExperimentStatus statusReport)
        {
            this.statusReport = statusReport;
            this.minTimetoLive = 0.0;
        }

        public LabExperimentStatus(ExperimentStatus statusReport, double minTimetoLive)
        {
            this.statusReport = statusReport;
            this.minTimetoLive = minTimetoLive;
        }
    }

    //-------------------------------------------------------------------------------------------------//

    /// <summary>
    /// LabStatus
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    public class LabStatus
    {
        /// <summary>
        /// True if the LabServer is accepting experiments.
        /// </summary>
        public bool online;

        /// <summary>
        /// Domain-dependent human-readable text describing the status of Lab Server.
        /// </summary>
        public string labStatusMessage;

        public LabStatus()
        {
            this.online = false;
            this.labStatusMessage = null;
        }

        /// <summary>
        /// Create lab status by specifying all values.
        /// </summary>
        /// <param name="online"></param>
        /// <param name="labStatusMessage"></param>
        public LabStatus(bool online, string labStatusMessage)
        {
            this.online = online;
            this.labStatusMessage = labStatusMessage;
        }
    }

    //-------------------------------------------------------------------------------------------------//

    /// <summary>
    /// ResultReport
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    public class ResultReport
    {
        /// <summary>
        /// Indicates the status of this experiment.
        /// </summary>
        public int statusCode;

        /// <summary>
        /// An opaque, domain-dependent set of experiment results.
        /// [REQUIRED if experimentStatus == Completed (3), OPTIONAL if experimentStatus == Failed (4)].
        /// </summary>
        public string experimentResults;
        // 
        // 

        /// <summary>
        /// A transparent XML string that helps to identify this experiment. Used for
        /// indexing and querying in generic components which can't understand the opaque
        /// experimentSpecification and experimentResults. [OPTIONAL, null if unused].
        /// </summary>
        public string xmlResultExtension;

        /// <summary>
        /// A transparent XML string that helps to identify any blobs saved as part of
        /// this experiment's results. [OPTIONAL, null if unused].
        /// </summary>
        public string xmlBlobExtension;

        /// <summary>
        /// Domain-dependent human-readable text containing non-fatal warnings about
        /// the experiment including runtime warnings.
        /// </summary>
        public string[] warningMessages;

        /// <summary>
        /// Domain-dependent human-readable text describing why the experiment terminated
        /// abnormally including runtime errors. [REQUIRED if experimentStatus == Failed (4)].
        /// </summary>
        public string errorMessage;

        public ResultReport()
        {
            this.statusCode = (int)StatusCodes.Unknown;
            this.experimentResults = null;
            this.xmlResultExtension = null;
            this.xmlBlobExtension = null;
            this.warningMessages = null;
            this.errorMessage = null;
        }

        /// <summary>
        /// Create a result report with the specified status code and error message
        /// </summary>
        /// <param name="statusCode"></param>
        public ResultReport(int statusCode, string errorMessage)
        {
            this.statusCode = statusCode;
            this.experimentResults = null;
            this.xmlResultExtension = null;
            this.xmlBlobExtension = null;
            this.warningMessages = null;
            this.errorMessage = errorMessage;
        }

        /// <summary>
        /// Create a result report by specifying all values.
        /// </summary>
        /// <param name="statusCode"></param>
        /// <param name="experimentResults"></param>
        /// <param name="xmlResultExtension"></param>
        /// <param name="xmlBlobExtension"></param>
        /// <param name="warningMessages"></param>
        /// <param name="errorMessage"></param>
        public ResultReport(int statusCode, string experimentResults, string xmlResultExtension,
            string xmlBlobExtension, string[] warningMessages, string errorMessage)
        {
            this.statusCode = statusCode;
            this.experimentResults = experimentResults;
            this.xmlResultExtension = xmlResultExtension;
            this.xmlBlobExtension = xmlBlobExtension;
            this.warningMessages = warningMessages;
            this.errorMessage = errorMessage;
        }
    }

    //-------------------------------------------------------------------------------------------------//

    /// <summary>
    /// SubmissionReport
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    public class SubmissionReport
    {
        public ValidationReport vReport;
        // See struct description below.

        /// <summary>
        /// A number &gt; 0 that identifies the experiment.
        /// </summary>
        public int experimentID;
        // 

        /// <summary>
        /// Guaranteed minimum time (in seconds, starting now) before this
        /// experimentID and associated data will be purged from the Lab Server.
        /// </summary>
        public double minTimeToLive;

        public WaitEstimate wait;
        // See struct description below.

        public SubmissionReport()
        {
            this.vReport = new ValidationReport();
            this.experimentID = -1;
            this.minTimeToLive = 0.0;
            this.wait = new WaitEstimate();
        }

        /// <summary>
        /// Constructor specifying only the 
        /// </summary>
        /// <param name="experimentID"></param>
        public SubmissionReport(int experimentID)
        {
            this.vReport = new ValidationReport();
            this.experimentID = experimentID;
            this.minTimeToLive = 0.0;
            this.wait = new WaitEstimate();
        }

        /// <summary>
        /// Create a submission report by specifying all values.
        /// </summary>
        /// <param name="vReport"></param>
        /// <param name="experimentID"></param>
        /// <param name="minTimeToLive"></param>
        /// <param name="wait"></param>
        public SubmissionReport(ValidationReport vReport, int experimentID, double minTimeToLive,
            WaitEstimate wait)
        {
            this.vReport = vReport;
            this.experimentID = experimentID;
            this.minTimeToLive = minTimeToLive;
            this.wait = wait;
        }
    }

    //-------------------------------------------------------------------------------------------------//

    /// <summary>
    /// ValidationReport
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    public class ValidationReport
    {
        /// <summary>
        /// True if the experiment specification is acceptable for execution.
        /// </summary>
        public bool accepted;

        /// <summary>
        /// Domain-dependent human-readable text containing non-fatal warnings about the experiment.
        /// </summary>
        public string[] warningMessages;

        /// <summary>
        /// Domain-dependent human-readable text describing why the experiment specification
        /// would not be accepted (if accepted == false).
        /// </summary>
        public string errorMessage;

        /// <summary>
        /// Estimated runtime (in seconds) of this experiment. [OPTIONAL, &lt; 0 if not supported].
        /// </summary>
        public double estRuntime;

        public ValidationReport()
        {
            this.accepted = false;
            this.warningMessages = null;
            this.errorMessage = null;
            this.estRuntime = 0.0;
        }

        /// <summary>
        /// Create a validation report by specifying only the 'accepted' value.
        /// </summary>
        /// <param name="accepted"></param>
        public ValidationReport(bool accepted)
        {
            this.accepted = accepted;
            this.warningMessages = null;
            this.errorMessage = null;
            this.estRuntime = 0.0;
        }

        /// <summary>
        /// Create a validation report by specifying all values.
        /// </summary>
        /// <param name="accepted"></param>
        /// <param name="warningMessages"></param>
        /// <param name="errorMessage"></param>
        /// <param name="estRuntime"></param>
        public ValidationReport(bool accepted, string[] warningMessages, string errorMessage,
            double estRuntime)
        {
            this.accepted = accepted;
            this.warningMessages = warningMessages;
            this.errorMessage = errorMessage;
            this.estRuntime = estRuntime;
        }
    }

    //-------------------------------------------------------------------------------------------------//

    /// <summary>
    /// WaitEstimate
    /// </summary>
    [SerializableAttribute()]
    [XmlTypeAttribute(Namespace = "http://ilab.mit.edu")]
    public class WaitEstimate
    {
        /// <summary>
        /// Number of experiments currently in the execution queue that would run before the
        /// hypothetical new experiment.
        /// </summary>
        public int effectiveQueueLength;

        /// <summary>
        /// Estimated wait time (in seconds) until the hypothetical new experiment would begin,
        /// based on the other experiments currently in the execution queue.
        /// [OPTIONAL, &lt; 0 if not supported].
        /// </summary>
        public double estWait;

        public WaitEstimate()
        {
            this.effectiveQueueLength = 0;
            this.estWait = 0;
        }

        /// <summary>
        /// Constructor specifying all values.
        /// </summary>
        /// <param name="effectiveQueueLength"></param>
        /// <param name="estWait"></param>
        public WaitEstimate(int effectiveQueueLength, double estWait)
        {
            this.effectiveQueueLength = effectiveQueueLength;
            this.estWait = estWait;
        }
    }
}
