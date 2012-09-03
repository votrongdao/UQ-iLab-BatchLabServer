using System;
using System.Diagnostics;
using System.Threading;
using System.IO;
using System.Xml;
using Library.Lab;
using Library.LabServerEngine;

namespace Library.LabServer.Drivers.Module
{
    public class SimActivity
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "SimActivity";

        //
        // Constants
        //
        private const int DELAYMS_NoSimulateDelays = 100;

        //
        // String constants for logfile messages
        //
        private const string STRLOG_Filename = " Filename: ";
        private const string STRLOG_ParsingSimulationConfig = " Parsing SimulationConfig...";
        private const string STRLOG_Title = " Title: ";
        private const string STRLOG_Version = " Version: ";
        private const string STRLOG_SimulateDelays = " SimulateDelays: ";

        private const string STRLOG_SimDistance = " SimDistance: ";
        private const string STRLOG_SimDuration = " SimDuration: ";
        private const string STRLOG_SimMean = " SimMean: ";
        private const string STRLOG_SimPower = " SimPower: ";
        private const string STRLOG_SimDeviation = " SimDeviation: ";
        private const string STRLOG_TubeOffsetDistance = " TubeOffsetDistance: ";
        private const string STRLOG_TubeHomeDistance = " TubeHomeDistance: ";
        private const string STRLOG_TubeMoveRate = " TubeMoveRate: ";

        //
        // Local variables
        //
        private Random random;
        private double simDistance;
        private int simDuration;
        private int simMean;
        private double simPower;
        private double simDeviation;
        private bool hasAbsorberTable;
        private int tubeOffsetDistance;
        private double tubeMoveRate;
        private int tubeDistance;   // Remembers the current tube distance
        private char sourceLocation;
        private char absorberLocation;

        private struct AxisInfo
        {
            public double[] selectTimes;
            public double[] returnTimes;
        }

        private AxisInfo sourceAxisInfo;
        private AxisInfo absorberAxisInfo;

        #endregion

        #region Properties

        private string filename;
        private string title;
        private string version;
        private bool simulateDelays;

        private int tubeHomeDistance;
        private char sourceFirstLocation;
        private char sourceLastLocation;
        private char sourceHomeLocation;
        private char absorberFirstLocation;
        private char absorberLastLocation;
        private char absorberHomeLocation;

        public string Filename
        {
            get { return this.filename; }
        }

        public string Title
        {
            get { return this.title; }
        }

        public string Version
        {
            get { return this.version; }
        }

        public bool SimulateDelays
        {
            get { return this.simulateDelays; }
            set { this.simulateDelays = value; }
        }

        public int TubeHomeDistance
        {
            get { return this.tubeHomeDistance; }
            set { this.tubeHomeDistance = value; }
        }

        public char SourceFirstLocation
        {
            get { return this.sourceFirstLocation; }
        }

        public char SourceLastLocation
        {
            get { return this.sourceLastLocation; }
        }

        public char SourceHomeLocation
        {
            get { return this.sourceHomeLocation; }
        }

        public char AbsorberFirstLocation
        {
            get { return this.absorberFirstLocation; }
        }

        public char AbsorberLastLocation
        {
            get { return this.absorberLastLocation; }
        }

        public char AbsorberHomeLocation
        {
            get { return this.absorberHomeLocation; }
        }

        #endregion

        //---------------------------------------------------------------------------------------//

        public SimActivity(Configuration configuration, bool simulateDelays)
            : this(configuration, simulateDelays, null, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public SimActivity(Configuration configuration, bool simulateDelays, string xmlSimulationConfig)
            : this(configuration, simulateDelays, xmlSimulationConfig, null)
        {
        }

        //---------------------------------------------------------------------------------------//

        public SimActivity(Configuration configuration, bool simulateDelays, string xmlSimulationConfig, string simulationConfigFilename)
        {
            const string STRLOG_MethodName = "SimActivity";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise local variables
            //
            this.simulateDelays = simulateDelays;
            this.hasAbsorberTable = false;

            try
            {
                XmlDocument xmlDocument;

                //
                // Check if an XML simulation configuration string is specified
                //
                if (xmlSimulationConfig != null)
                {
                    //
                    // Load the simulation configuration from an XML string
                    //
                    xmlDocument = XmlUtilities.GetXmlDocument(xmlSimulationConfig);
                }
                else
                {
                    //
                    // Check if an XML simulation configuration filename is specified
                    //
                    if (simulationConfigFilename == null)
                    {
                        //
                        // Get simulation configuration filename from Application's configuration file
                        //
                        this.filename = Utilities.GetAppSetting(Consts.STRCFG_XmlSimulationConfigFilename);
                        this.filename = Path.Combine(configuration.RootFilePath, this.filename);
                    }
                    else
                    {
                        // Prepend full file path
                        this.filename = Path.Combine(configuration.RootFilePath, simulationConfigFilename);
                    }

                    Logfile.Write(STRLOG_Filename + this.filename);

                    // Load the simulation configuration from the specified file
                    xmlDocument = XmlUtilities.GetXmlDocumentFromFile(this.filename);
                }

                // Get the simulation configuration XML node
                XmlNode xmlNodeSimulationConfig = XmlUtilities.GetXmlRootNode(xmlDocument, Consts.STRXML_simulationConfig);

                Logfile.Write(STRLOG_ParsingSimulationConfig);

                //
                // Get information from the simulation configuration node
                //
                this.title = XmlUtilities.GetXmlValue(xmlNodeSimulationConfig, Consts.STRXMLPARAM_title, false);
                this.version = XmlUtilities.GetXmlValue(xmlNodeSimulationConfig, Consts.STRXMLPARAM_version, false);

                string logMessage = STRLOG_Title + this.title +
                    Logfile.STRLOG_Spacer + STRLOG_Version + this.version +
                    Logfile.STRLOG_Spacer + STRLOG_SimulateDelays + this.simulateDelays;
                Logfile.Write(logMessage);

                this.simDistance = XmlUtilities.GetRealValue(xmlNodeSimulationConfig, Consts.STRXML_simDistance);
                this.simDuration = XmlUtilities.GetIntValue(xmlNodeSimulationConfig, Consts.STRXML_simDuration);
                this.simMean = XmlUtilities.GetIntValue(xmlNodeSimulationConfig, Consts.STRXML_simMean);
                this.simPower = XmlUtilities.GetRealValue(xmlNodeSimulationConfig, Consts.STRXML_simPower);
                this.simDeviation = XmlUtilities.GetRealValue(xmlNodeSimulationConfig, Consts.STRXML_simDeviation);

                logMessage = STRLOG_SimDistance + this.simDistance +
                    Logfile.STRLOG_Spacer + STRLOG_SimDuration + this.simDuration +
                    Logfile.STRLOG_Spacer + STRLOG_SimMean + this.simMean +
                    Logfile.STRLOG_Spacer + STRLOG_SimPower + this.simPower +
                    Logfile.STRLOG_Spacer + STRLOG_SimDeviation + this.simDeviation;
                Logfile.Write(logMessage);

                //
                // Initialise tube settings
                //
                XmlNode xmlNode = XmlUtilities.GetXmlNode(xmlNodeSimulationConfig, Consts.STRXML_tube);
                this.tubeOffsetDistance = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_offsetDistance);
                this.tubeHomeDistance = XmlUtilities.GetIntValue(xmlNode, Consts.STRXML_homeDistance);
                this.tubeDistance = this.tubeHomeDistance;
                this.tubeMoveRate = XmlUtilities.GetRealValue(xmlNode, Consts.STRXML_moveRate);

                logMessage = STRLOG_TubeOffsetDistance + tubeOffsetDistance.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_TubeHomeDistance + tubeHomeDistance.ToString() +
                    Logfile.STRLOG_Spacer + STRLOG_TubeMoveRate + tubeMoveRate.ToString();
                Logfile.Write(logMessage);

                //
                // Initialise source settings
                //
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeSimulationConfig, Consts.STRXML_sources);
                this.sourceFirstLocation = XmlUtilities.GetCharValue(xmlNode, Consts.STRXML_firstLocation);
                this.sourceHomeLocation = XmlUtilities.GetCharValue(xmlNode, Consts.STRXML_homeLocation);
                this.sourceLastLocation = this.sourceFirstLocation;
                this.sourceLocation = this.sourceHomeLocation;

                //
                // Initialise source select times array
                //
                string sourceSelectTimes = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXML_selectTimes, false);
                string[] strSplit = sourceSelectTimes.Split(new char[] { LabServerEngine.Consts.CHR_CsvSplitterChar });
                this.sourceAxisInfo.selectTimes = new double[strSplit.Length];
                for (int i = 0; i < strSplit.Length; i++)
                {
                    this.sourceAxisInfo.selectTimes[i] = Double.Parse(strSplit[i]);
                }
                this.sourceLastLocation = (char)(this.sourceFirstLocation + this.sourceAxisInfo.selectTimes.Length - 1);

                //
                // Initialise source return times array
                //
                string sourceReturnTimes = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXML_returnTimes, false);
                strSplit = sourceReturnTimes.Split(new char[] { LabServerEngine.Consts.CHR_CsvSplitterChar });
                this.sourceAxisInfo.returnTimes = new double[strSplit.Length];
                for (int i = 0; i < strSplit.Length; i++)
                {
                    this.sourceAxisInfo.returnTimes[i] = Double.Parse(strSplit[i]);
                }

                //
                // Initialise absorber settings
                //
                xmlNode = XmlUtilities.GetXmlNode(xmlNodeSimulationConfig, Consts.STRXML_absorbers);
                this.absorberFirstLocation = XmlUtilities.GetCharValue(xmlNode, Consts.STRXML_firstLocation);
                this.absorberHomeLocation = XmlUtilities.GetCharValue(xmlNode, Consts.STRXML_homeLocation);
                this.absorberLastLocation = this.absorberFirstLocation;
                this.absorberLocation = this.absorberHomeLocation;

                try
                {
                    //
                    // Initialise absorber select times array
                    //
                    string absorberSelectTimes = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXML_selectTimes, false);
                    strSplit = absorberSelectTimes.Split(new char[] { LabServerEngine.Consts.CHR_CsvSplitterChar });
                    this.absorberAxisInfo.selectTimes = new double[strSplit.Length];
                    for (int i = 0; i < strSplit.Length; i++)
                    {
                        this.absorberAxisInfo.selectTimes[i] = Double.Parse(strSplit[i]);
                    }
                    this.absorberLastLocation = (char)(this.absorberFirstLocation + this.absorberAxisInfo.selectTimes.Length - 1);

                    //
                    // Initialise absorber return times array
                    //
                    string absorberReturnTimes = XmlUtilities.GetXmlValue(xmlNode, Consts.STRXML_returnTimes, false);
                    strSplit = absorberReturnTimes.Split(new char[] { LabServerEngine.Consts.CHR_CsvSplitterChar });
                    this.absorberAxisInfo.returnTimes = new double[strSplit.Length];
                    for (int i = 0; i < strSplit.Length; i++)
                    {
                        this.absorberAxisInfo.returnTimes[i] = Double.Parse(strSplit[i]);
                    }

                    //
                    // There is an absorber table
                    //
                    this.hasAbsorberTable = true;
                }
                catch
                {
                    // No absorber table
                }
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                throw;
            }

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public int GetTubeHomeDistance()
        {
            return this.tubeHomeDistance;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetTubeMoveTime(int startDistance, int endDistance)
        {
            double seconds = 0.0;

            if (this.simulateDelays == true)
            {
                // Get absolute distance
                int distance = endDistance - startDistance;
                if (distance < 0)
                {
                    distance = -distance;
                }

                // Tube move rate is in ms per mm
                seconds = (distance * this.tubeMoveRate) / 1000;
            }
            else
            {
                seconds = (double)DELAYMS_NoSimulateDelays / 1000;
            }

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetSourceSelectTime(char toLocation)
        {
            double seconds = 0.0;

            if (this.simulateDelays == true)
            {
                int index = toLocation - this.sourceFirstLocation;
                if (index >= 0 && index < this.sourceAxisInfo.selectTimes.Length)
                {
                    seconds = this.sourceAxisInfo.selectTimes[index];
                }
            }
            else
            {
                seconds = (double)DELAYMS_NoSimulateDelays / 1000;
            }

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetSourceReturnTime(char fromLocation)
        {
            double seconds = 0.0;

            if (this.simulateDelays == true)
            {
                int index = fromLocation - this.sourceFirstLocation;
                if (index >= 0 && index < this.sourceAxisInfo.returnTimes.Length)
                {
                    seconds = this.sourceAxisInfo.returnTimes[index];
                }
            }
            else
            {
                seconds = (double)DELAYMS_NoSimulateDelays / 1000;
            }

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetAbsorberSelectTime(char toLocation)
        {
            double seconds = 0.0;

            if (this.hasAbsorberTable == true)
            {
                if (this.simulateDelays == true)
                {
                    int index = toLocation - this.absorberFirstLocation;
                    if (index >= 0 && index < this.absorberAxisInfo.selectTimes.Length)
                    {
                        seconds = this.absorberAxisInfo.selectTimes[index];
                    }
                }
                else
                {
                    seconds = (double)DELAYMS_NoSimulateDelays / 1000;
                }
            }

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetAbsorberReturnTime(char fromLocation)
        {
            double seconds = 0.0;

            if (this.hasAbsorberTable == true)
            {
                if (this.simulateDelays == true)
                {
                    int index = fromLocation - this.absorberFirstLocation;
                    if (index >= 0 && index < this.absorberAxisInfo.returnTimes.Length)
                    {
                        if (this.hasAbsorberTable == true)
                        {
                            seconds = this.absorberAxisInfo.returnTimes[index];
                        }
                    }
                }
                else
                {
                    seconds = (double)DELAYMS_NoSimulateDelays / 1000;
                }
            }

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public double GetCaptureDataTime(int duration)
        {
            double seconds = 0;

            if (this.simulateDelays == true)
            {
                seconds = duration;
            }
            else
            {
                seconds = (double)DELAYMS_NoSimulateDelays / 1000;
            }

            return seconds;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetTubeDistance(int targetDistance)
        {
            if (this.simulateDelays == true)
            {
                int seconds = (int)this.GetTubeMoveTime(this.tubeDistance, targetDistance);

                for (int i = 0; i < seconds; i++)
                {
                    Thread.Sleep(1000);
                    Trace.Write("T");
                }
                Trace.WriteLine("");
            }
            else
            {
                Thread.Sleep(DELAYMS_NoSimulateDelays);
            }

            this.tubeDistance = targetDistance;

            return true;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetSourceLocation(char location)
        {
            if (this.simulateDelays == true)
            {
                //
                // Determine if selecting or returning source
                //
                int seconds;
                if (location != this.sourceHomeLocation)
                {
                    seconds = (int)this.GetSourceSelectTime(location);
                }
                else
                {
                    seconds = (int)this.GetSourceReturnTime(this.sourceLocation);
                }

                for (int i = 0; i < seconds; i++)
                {
                    Thread.Sleep(1000);
                    Trace.Write("S");
                }
                Trace.WriteLine("");
            }
            else
            {
                Thread.Sleep(DELAYMS_NoSimulateDelays);
            }

            this.sourceLocation = location;

            return true;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool SetAbsorberLocation(char location)
        {
            if (this.hasAbsorberTable == true)
            {
                if (this.simulateDelays == true)
                {
                    //
                    // Determine if selecting or returning absorber
                    //
                    int seconds;
                    if (location != this.absorberHomeLocation)
                    {
                        seconds = (int)this.GetAbsorberSelectTime(location);
                    }
                    else
                    {
                        seconds = (int)this.GetAbsorberReturnTime(this.absorberLocation);
                    }

                    for (int i = 0; i < seconds; i++)
                    {
                        Thread.Sleep(1000);
                        Trace.Write("A");
                    }
                    Trace.WriteLine("");
                }
                else
                {
                    Thread.Sleep(DELAYMS_NoSimulateDelays);
                }

                this.absorberLocation = location;
            }

            return true;
        }

        //-------------------------------------------------------------------------------------------------//

        public int[] GenerateData(int distance, int duration, int repeat)
        {
            if (this.random == null)
            {
                //
                // Randomise the random number generator seed
                //
                int seed = DateTime.Now.Millisecond;
                random = new Random(seed);
            }

            //
            // Generate Gaussian distribution of random numbers
            //
            double[] dataGaussian = new double[repeat];
            for (int i = 0; i < repeat; i++)
            {
                dataGaussian[i] = GetGaussian(random);
            }

            //
            // Adjust data
            //
            dataGaussian = AdjustData(dataGaussian, duration, distance);

            //
            // Convert the simulated data from 'double' to 'int'
            //
            int[] simDataGaussian = Array.ConvertAll(dataGaussian, new Converter<double, int>(DoubleToInt));

            return simDataGaussian;
        }

        private int DoubleToInt(double value)
        {
            int intValue;

            //
            // Value cannot be negative
            //
            if ((intValue = (int)(value + 0.5)) < 0)
            {
                intValue = 0;
            }

            return intValue;
        }

        //-------------------------------------------------------------------------------------------------//

        public bool CaptureData(int duration, int[] counts, int[] generatedData, int repeatIndex)
        {
            if (this.simulateDelays == true)
            {
                for (int i = 0; i < duration; i++)
                {
                    Thread.Sleep(1000);
                    Trace.Write("D");
                }
                Trace.WriteLine("");
            }
            else
            {
                Thread.Sleep(DELAYMS_NoSimulateDelays);
            }

            counts[0] = generatedData[repeatIndex];

            return true;
        }

        //=================================================================================================//

        private double[] AdjustData(double[] data, double duration, double distance)
        {
            //
            // Adjust the data for the mean, standard deviation, duration and distance
            //
            double adjustStdDev = this.simDeviation * distance / this.simDistance;
            double adjustDuration = duration / this.simDuration;
            double adjustDistance = Math.Pow(distance / this.simDistance, this.simPower);

            for (int i = 0; i < data.Length; i++)
            {
                // Adjust for the mean and standard deviation
                double value = data[i] * adjustStdDev + this.simMean;

                // Adjust for the duration
                value *= adjustDuration;

                // Adjust for the distance
                value /= adjustDistance;

                data[i] = value;
            }

            return data;
        }

        //-------------------------------------------------------------------------------------------------//

        /// <summary>
        /// Generate a Gaussian distribution of data with
		/// a mean of 0.0 and a standard deviation of 1.0 using the Box–Muller transform method
        /// </summary>
        /// <param name="random"></param>
        /// <returns></returns>
        private double GetGaussian(Random random)
        {
            double random1;
            while (true)
            {
                // random1 must be > 0.0 for Math.Log()
                random1 = random.NextDouble();
                if (random1 > 0.0)
                {
                    break;
                }
            }
            double random2 = random.NextDouble();

            double gaussian1 = Math.Sqrt(-2.0 * Math.Log(random1)) * Math.Cos(Math.PI * 2.0 * random2);

            // Don't need the second number
            //double gaussian2 = Math.Sqrt(-2.0 * Math.Log(random1)) * Math.Sin(Math.PI * 2.0 * random2);

            return gaussian1;
        }

    }

}
