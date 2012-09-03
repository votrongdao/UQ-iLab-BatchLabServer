using System;
using System.Xml;
using Library.Lab;
using Library.LabServerEngine;
using Library.LabServer.Drivers;

namespace Library.LabServer
{
    public class Validation : ExperimentValidation
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "Validation";

        //
        // String constants for error messages
        //
        private const string STRERR_Distance = " Distance (";
        private const string STRERR_Duration = " Duration (";
        private const string STRERR_Repeat = " Repeat count (";
        private const string STRERR_TotalTime = " Total time in seconds (";
        private const string STRERR_IsLessThan = ") is less than ";
        private const string STRERR_IsGreaterThan = ") is greater than ";
        private const string STRERR_ReduceTotalTime = " - Reduce 'Duration', 'Trials' or number of distances.";

        //
        // Local types
        //
        private struct VdnMinMax
        {
            public int min;
            public int max;
        }

        //
        // Local variables
        //
        private VdnMinMax distance;
        private VdnMinMax duration;
        private VdnMinMax repeat;
        private VdnMinMax totalTime;

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public Validation(Configuration configuration)
            : base(configuration)
        {
            const string STRLOG_MethodName = "Validation";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Get information from the validation XML node
            //
            try
            {
                //
                // Get distance range information from the validation node
                //
                XmlNode xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeValidation, Consts.STRXML_vdnDistance);
                this.distance = new VdnMinMax();
                this.distance.min = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_minimum);
                this.distance.max = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_maximum);

                //
                // Get duration range information from the validation node
                //
                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeValidation, Consts.STRXML_vdnDuration);
                this.duration = new VdnMinMax();
                this.duration.min = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_minimum);
                this.duration.max = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_maximum);

                //
                // Get distance range information from the validation node
                //
                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeValidation, Consts.STRXML_vdnRepeat);
                this.repeat = new VdnMinMax();
                this.repeat.min = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_minimum);
                this.repeat.max = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_maximum);

                //
                // Get distance range information from the validation node
                //
                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeValidation, Consts.STRXML_vdnTotaltime);
                this.totalTime = new VdnMinMax();
                this.totalTime.min = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_minimum);
                this.totalTime.max = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_maximum);

            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateDistance(int distance)
        {
            try
            {
                if (distance < this.distance.min)
                {
                    throw new ArgumentException(STRERR_Distance + distance.ToString() +
                        STRERR_IsLessThan + this.distance.min.ToString());
                }
                if (distance > this.distance.max)
                {
                    throw new ArgumentException(STRERR_Distance + distance.ToString() +
                        STRERR_IsGreaterThan + this.distance.max.ToString());
                }
            }
            catch (Exception)
            {
                // Throw error back to caller
                throw;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateDuration(int duration)
        {
            try
            {
                if (duration < this.duration.min)
                {
                    throw new ArgumentException(STRERR_Duration + duration.ToString() +
                        STRERR_IsLessThan + this.duration.min.ToString());
                }
                if (duration > this.duration.max)
                {
                    throw new ArgumentException(STRERR_Duration + duration.ToString() +
                        STRERR_IsGreaterThan + this.duration.max.ToString());
                }
            }
            catch (Exception)
            {
                // Throw error back to caller
                throw;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateRepeat(int repeat)
        {
            try
            {
                if (repeat < this.repeat.min)
                {
                    throw new ArgumentException(STRERR_Repeat + repeat.ToString() +
                        STRERR_IsLessThan + this.repeat.min.ToString());
                }
                if (repeat > this.repeat.max)
                {
                    throw new ArgumentException(STRERR_Repeat + repeat.ToString() +
                        STRERR_IsGreaterThan + this.repeat.max.ToString());
                }
            }
            catch (Exception)
            {
                // Throw error back to caller
                throw;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateTotalTime(int totalTime)
        {
            try
            {
                if (totalTime < this.totalTime.min)
                {
                    throw new ArgumentException(STRERR_TotalTime + totalTime.ToString() +
                        STRERR_IsLessThan + this.totalTime.min.ToString());
                }
                if (totalTime > this.totalTime.max)
                {
                    throw new ArgumentException(STRERR_TotalTime + totalTime.ToString() +
                        STRERR_IsGreaterThan + this.totalTime.max.ToString() + STRERR_ReduceTotalTime);
                }
            }
            catch (Exception)
            {
                // Throw error back to caller
                throw;
            }
        }

    }
}
