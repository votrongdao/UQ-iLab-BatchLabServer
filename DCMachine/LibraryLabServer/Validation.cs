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
        private const string STRERR_MinimumField = " Minimum field (";
        private const string STRERR_MaximumField = " Maximum field (";
        private const string STRERR_FieldStep = " Field step (";
        private const string STRERR_MinimumLoad = " Minimum load (";
        private const string STRERR_MaximumLoad = " Maximum load (";
        private const string STRERR_LoadStep = " Load step (";
        private const string STRERR_MinimumSpeed = " Minimum speed (";
        private const string STRERR_MaximumSpeed = " Maximum speed (";
        private const string STRERR_SpeedStep = " Speed step (";
        private const string STRERR_IsLessThan = ") is less than ";
        private const string STRERR_IsGreaterThan = ") is greater than ";
        private const string STRERR_MaxFieldLessThanMinField = " Maximum field is less than minimum field";
        private const string STRERR_MaxLoadLessThanMinLoad = " Maximum load is less than minimum load";
        private const string STRERR_MaxSpeedLessThanMinSpeed = " Maximum speed is less than minimum speed";

        //
        // Local types
        //
        private struct VdnMinMaxStep
        {
            public int min;
            public int max;
            public int stepMin;
            public int stepMax;
        }

        //
        // Local variables
        //
        private VdnMinMaxStep speed;
        private VdnMinMaxStep field;
        private VdnMinMaxStep load;

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
                // Get speed range information from the lab configuration node
                //
                XmlNode xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeValidation, Consts.STRXML_vdnSpeed);
                this.speed = new VdnMinMaxStep();
                this.speed.min = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_minimum);
                this.speed.max = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_maximum);
                this.speed.stepMin = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_stepMin);
                this.speed.stepMax = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_stepMax);

                //
                // Get field range information from the lab configuration node
                //
                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeValidation, Consts.STRXML_vdnField);
                this.field = new VdnMinMaxStep();
                this.field.min = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_minimum);
                this.field.max = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_maximum);
                this.field.stepMin = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_stepMin);
                this.field.stepMax = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_stepMax);

                //
                // Get load range information from the lab configuration node
                //
                xmlNode = XmlUtilities.GetXmlNode(this.xmlNodeValidation, Consts.STRXML_vdnLoad);
                this.load = new VdnMinMaxStep();
                this.load.min = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_minimum);
                this.load.max = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_maximum);
                this.load.stepMin = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_stepMin);
                this.load.stepMax = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_stepMax);
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateField(MinMaxStep field)
        {
            try
            {
                //
                // Check minimum field
                //
                if (field.min < this.field.min)
                {
                    throw new ArgumentException(STRERR_MinimumField + field.min.ToString() +
                        STRERR_IsLessThan + this.field.min.ToString());
                }
                if (field.min > this.field.max)
                {
                    throw new ArgumentException(STRERR_MinimumField + field.min.ToString() +
                        STRERR_IsGreaterThan + this.field.max.ToString());
                }

                //
                // Check maximum field
                //
                if (field.max < this.field.min)
                {
                    throw new ArgumentException(STRERR_MaximumField + field.max.ToString() +
                        STRERR_IsLessThan + this.field.min.ToString());
                }
                if (field.max > this.field.max)
                {
                    throw new ArgumentException(STRERR_MaximumField + field.max.ToString() +
                        STRERR_IsGreaterThan + this.field.max.ToString());
                }
                if (field.max < field.min)
                {
                    throw new ArgumentException(STRERR_MaxFieldLessThanMinField);
                }

                //
                // Check field step
                //
                if (field.step < this.field.stepMin)
                {
                    throw new ArgumentException(STRERR_FieldStep + field.step.ToString() +
                        STRERR_IsLessThan + this.field.stepMin.ToString());
                }
                if (field.step > this.field.stepMax)
                {
                    throw new ArgumentException(STRERR_FieldStep + field.step.ToString() +
                        STRERR_IsGreaterThan + this.field.stepMax.ToString());
                }
            }
            catch (Exception)
            {
                // Throw error back to caller
                throw;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateLoad(MinMaxStep load)
        {
            try
            {
                //
                // Check minimum load
                //
                if (load.min < this.load.min)
                {
                    throw new ArgumentException(STRERR_MinimumLoad + load.min.ToString() +
                        STRERR_IsLessThan + this.load.min.ToString());
                }
                if (load.min > this.load.max)
                {
                    throw new ArgumentException(STRERR_MinimumLoad + load.min.ToString() +
                        STRERR_IsGreaterThan + this.load.max.ToString());
                }

                //
                // Check maximum load
                //
                if (load.max < this.load.min)
                {
                    throw new ArgumentException(STRERR_MaximumLoad + load.max.ToString() +
                        STRERR_IsLessThan + this.load.min.ToString());
                }
                if (load.max > this.load.max)
                {
                    throw new ArgumentException(STRERR_MaximumLoad + load.max.ToString() +
                        STRERR_IsGreaterThan + this.load.max.ToString());
                }
                if (load.max < load.min)
                {
                    throw new ArgumentException(STRERR_MaxLoadLessThanMinLoad);
                }

                //
                // Check load step
                //
                if (load.step < this.load.stepMin)
                {
                    throw new ArgumentException(STRERR_LoadStep + load.step.ToString() +
                        STRERR_IsLessThan + this.load.stepMin.ToString());
                }
                if (load.step > this.load.stepMax)
                {
                    throw new ArgumentException(STRERR_LoadStep + load.step.ToString() +
                        STRERR_IsGreaterThan + this.load.stepMax.ToString());
                }
            }
            catch (Exception)
            {
                // Throw error back to caller
                throw;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public void ValidateSpeed(MinMaxStep speed)
        {
            try
            {
                //
                // Check minimum speed
                //
                if (speed.min < this.speed.min)
                {
                    throw new ArgumentException(STRERR_MinimumSpeed + speed.min.ToString() +
                        STRERR_IsLessThan + this.speed.min.ToString());
                }
                if (speed.min > this.speed.max)
                {
                    throw new ArgumentException(STRERR_MinimumSpeed + speed.min.ToString() +
                        STRERR_IsGreaterThan + this.speed.max.ToString());
                }

                //
                // Check maximum speed
                //
                if (speed.max < this.speed.min)
                {
                    throw new ArgumentException(STRERR_MaximumSpeed + speed.max.ToString() +
                        STRERR_IsLessThan + this.speed.min.ToString());
                }
                if (speed.max > this.speed.max)
                {
                    throw new ArgumentException(STRERR_MaximumSpeed + speed.max.ToString() +
                        STRERR_IsGreaterThan + this.speed.max.ToString());
                }
                if (speed.max < speed.min)
                {
                    throw new ArgumentException(STRERR_MaxSpeedLessThanMinSpeed);
                }

                //
                // Check speed step
                //
                if (speed.step < this.speed.stepMin)
                {
                    throw new ArgumentException(STRERR_SpeedStep + speed.step.ToString() +
                        STRERR_IsLessThan + this.speed.stepMin.ToString());
                }
                if (speed.step > this.speed.stepMax)
                {
                    throw new ArgumentException(STRERR_SpeedStep + speed.step.ToString() +
                        STRERR_IsGreaterThan + this.speed.stepMax.ToString());
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
