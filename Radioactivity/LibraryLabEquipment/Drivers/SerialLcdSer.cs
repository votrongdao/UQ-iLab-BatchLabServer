using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Ports;
using System.Text;
using System.Threading;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class SerialLcdSer : SerialLcd
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "SerialLcdSer";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_SerialPort = " SerialPort: ";
        private const string STRLOG_BaudRate = " BaudRate: ";
        private const string STRLOG_CreatingSerialPort = " Creating SerialPort ...";
        private const string STRLOG_OpeningSerialPort = " Opening serial port...";
        private const string STRLOG_ReceiveHandlerThreadIsStarting = " ReceiveHandler thread is starting...";
        private const string STRLOG_ReceiveHandlerThreadIsRunning = " ReceiveHandler thread is running.";

        //
        // String constants for error messages
        //
        private const string STRERR_ReceiveHandlerThreadFailedToStart = "ReceiveHandler thread failed to start!";

        //
        // Local variables
        //
        private bool disposed;
        private SerialPort serialPort;
        private Thread receiveThread;
        private bool receiveRunning;

        #endregion

        #region Properties

        private bool timedOut;

        public bool IsOpen
        {
            get
            {
                if (serialPort == null)
                {
                    return false;
                }
                else
                {
                    return serialPort.IsOpen;
                }
            }
        }

        public bool TimedOut
        {
            get { return timedOut; }
            set { timedOut = value; }
        }

        #endregion

        //-------------------------------------------------------------------------------------------------//

        public SerialLcdSer(XmlNode xmlNodeEquipmentConfig)
            : base(xmlNodeEquipmentConfig)
        {
            const string STRLOG_MethodName = "SerialLcdSer";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise local variables
            //
            this.disposed = true;
            this.receiveRunning = false;
            this.timedOut = false;

            //
            // Get the serial port to use and the baud rate
            //
            XmlNode xmlNodeSerialLcd = XmlUtilities.GetXmlNode(xmlNodeEquipmentConfig, Consts.STRXML_serialLcd);
            XmlNode xmlNodeSerial = XmlUtilities.GetXmlNode(xmlNodeSerialLcd, Consts.STRXML_serial, false);
            string serialport = XmlUtilities.GetXmlValue(xmlNodeSerial, Consts.STRXML_port, false);
            int baudrate = XmlUtilities.GetIntValue(xmlNodeSerial, Consts.STRXML_baud);

            Logfile.Write(STRLOG_SerialPort + serialport +
                Logfile.STRLOG_Spacer + STRLOG_BaudRate + baudrate.ToString());

            //
            // Create an instance of the serial port, set read and write timeouts
            //
            Logfile.Write(STRLOG_CreatingSerialPort);
            this.serialPort = new SerialPort(serialport, baudrate);
            this.serialPort.ReadTimeout = 1000;
            this.serialPort.WriteTimeout = 3000;

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //-------------------------------------------------------------------------------------------------//

        public override bool Initialise()
        {
            const string STRLOG_MethodName = "Initialise";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            bool success = false;

            //
            // Do base class initialisation first
            //
            base.Initialise();

            //
            // Check if this is first-time initialisation
            //
            if (this.initialised == false)
            {
                this.statusMessage = STRLOG_Initialising;

                //
                // Nothing to do here
                //

                //
                // First-time initialisation is complete
                //
                this.initialised = true;
            }

            //
            // Initialisation that must be done each time the equipment is powered up
            //
            try
            {
                //
                // Open the serial
                //
                Logfile.Write(STRLOG_OpeningSerialPort);
                this.serialPort.Open();

                //
                // Create and start the receive thread
                //
                Logfile.Write(STRLOG_ReceiveHandlerThreadIsStarting);
                this.receiveThread = new Thread(new ThreadStart(this.ReceiveHandler));
                this.receiveThread.Start();

                //
                // Give the thread a chance to start and then check that it has started
                //
                for (int i = 0; i < 5; i++)
                {
                    Thread.Sleep(500);
                    if ((success = this.receiveRunning) == true)
                    {
                        Logfile.Write(STRLOG_ReceiveHandlerThreadIsRunning);
                        break;
                    }
                    Trace.Write('?');
                }
                if (success == true)
                {
                    this.disposed = false;
                }
                else
                {
                    throw new ArgumentException(STRERR_ReceiveHandlerThreadFailedToStart);
                }

                //
                // Get the firmware version and display
                //
                string firmwareVersion = this.GetHardwareFirmwareVersion();
                this.WriteLine(1, STRLOG_ClassName);
                this.WriteLine(2, firmwareVersion);

                //
                // Ensure data capture is stopped
                //
                this.StopCapture();

                //
                // Initialisation is complete
                //
                this.online = true;
                this.statusMessage = StatusCodes.Ready.ToString();

                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
                this.Close();
            }

            string logMessage = STRLOG_Success + success.ToString();

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        #region Dispose

        /// <summary>
        /// Dispose(bool disposing) executes in two distinct scenarios:
        /// 1. If disposing equals true, the method has been called directly or indirectly by a user's code.
        ///    Managed and unmanaged resources can be disposed.
        /// 2. If disposing equals false, the method has been called by the runtime from inside the finalizer
        ///    and you should not reference other objects. Only unmanaged resources can be disposed.
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing)
        {
            const string STRLOG_MethodName = "Dispose";

            string logMessage = STRLOG_disposing + disposing.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_disposed + this.disposed.ToString();

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName, logMessage);

            //
            // Check to see if Dispose has already been called
            //
            if (this.disposed == false)
            {
                //
                // If disposing equals true, dispose all managed and unmanaged resources.
                //
                if (disposing == true)
                {
                    // Dispose managed resources here. Anything that has a Dispose() method.
                }

                //
                // Release unmanaged resources here. If disposing is false, only the following
                // code is executed.
                //

                //
                // Call base class before closing the serial port
                //
                base.Dispose(disposing);

                //
                // Stop the receive thread and close the serial port
                //
                if (this.receiveRunning == true)
                {
                    this.receiveRunning = false;
                    this.receiveThread.Join();
                }
                if (this.serialPort != null && this.serialPort.IsOpen)
                {
                    this.serialPort.Close();
                }

                this.disposed = true;
            }

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

        #endregion

        //=================================================================================================//

        protected override bool SendData(byte[] data, int dataLength)
        {
            bool success = false;

            try
            {
                //
                // Write the data to the serial LCD on the serial port
                //
                this.serialPort.Write(data, 0, dataLength);
                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private void ReceiveHandler()
        {
            const string STRLOG_MethodName = "ReceiveHandler";

            Logfile.WriteCalled(STRLOG_ClassName, STRLOG_MethodName);

            const int BUFFER_SIZE = 128;
            byte[] receiveBuffer = new byte[BUFFER_SIZE];

            this.receiveRunning = true;
            while (this.receiveRunning == true)
            {
                try
                {
                    //
                    // Read the data from the serial port
                    //
                    int bytesRead = this.serialPort.Read(receiveBuffer, 0, BUFFER_SIZE);
                    Trace.WriteLine("ReceiveHandler: bytesRead=" + bytesRead.ToString());

                    //
                    // Pass data on for processing
                    //
                    this.ReceiveData(receiveBuffer, bytesRead);
                }
                catch (TimeoutException)
                {
                }
            }

            Trace.WriteLine("ReceiveHandler(): Exiting");

            Logfile.WriteCompleted(STRLOG_ClassName, STRLOG_MethodName);
        }

    }
}
