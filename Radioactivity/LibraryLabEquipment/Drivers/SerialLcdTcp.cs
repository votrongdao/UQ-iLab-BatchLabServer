using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Xml;
using Library.Lab;

namespace Library.LabEquipment.Drivers
{
    public class SerialLcdTcp : SerialLcd
    {
        #region Class Constants and Variables

        private const string STRLOG_ClassName = "SerialLcdTcp";

        //
        // String constants for logfile messages
        //
        private const string STRLOG_IPaddr = " IPaddr: ";
        private const string STRLOG_Port = " Port: ";
        private const string STRLOG_CreatingTcpClient = " Creating TcpClient ...";
        private const string STRLOG_ClosingTcpClient = " Closing TcpClient ...";

        //
        // Local variables
        //
        private bool disposed;
        private string ipaddr;
        private int port;
        private TcpClient tcpClient;
        private NetworkStream tcpClientStream;
        private AsyncObject asyncObject;

        #endregion

        //---------------------------------------------------------------------------------------//

        public SerialLcdTcp(XmlNode xmlNodeEquipmentConfig)
            : base(xmlNodeEquipmentConfig)
        {
            const string STRLOG_MethodName = "SerialLcdTcp";

            Logfile.WriteCalled(null, STRLOG_MethodName);

            //
            // Initialise local variables
            //
            this.disposed = true;

            //
            // Get the IP address and port number to use
            //
            XmlNode xmlNodeSerialLcd = XmlUtilities.GetXmlNode(xmlNodeEquipmentConfig, Consts.STRXML_serialLcd);
            XmlNode xmlNodeNetwork = XmlUtilities.GetXmlNode(xmlNodeSerialLcd, Consts.STRXML_network, false);
            this.ipaddr = XmlUtilities.GetXmlValue(xmlNodeNetwork, Consts.STRXML_ipaddr, false);
            IPAddress ipaddr = IPAddress.Parse(this.ipaddr);
            this.port = XmlUtilities.GetIntValue(xmlNodeNetwork, Consts.STRXML_port);

            Logfile.Write(STRLOG_IPaddr + this.ipaddr.ToString() +
                Logfile.STRLOG_Spacer + STRLOG_Port + this.port.ToString());

            Logfile.WriteCompleted(null, STRLOG_MethodName);
        }

        //---------------------------------------------------------------------------------------//

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
                // Create TCP client connection to the SerialLCD
                //
                Logfile.Write(STRLOG_CreatingTcpClient);
                this.tcpClient = new TcpClient(this.ipaddr, this.port);
                this.tcpClientStream = this.tcpClient.GetStream();

                this.disposed = false;

                //
                // Create async object for TCP client callback
                //
                this.asyncObject = new AsyncObject();
                this.asyncObject.tcpClientStream = this.tcpClientStream;

                //
                // Begin receiving data from the SerialLCD on the network
                //
                this.tcpClientStream.BeginRead(this.asyncObject.receiveBuffer, 0, AsyncObject.BUFFER_SIZE,
                    new AsyncCallback(ReceiveCallback), this.asyncObject);

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

            Logfile.WriteCalled(this.logLevel, STRLOG_ClassName, STRLOG_MethodName, logMessage);

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
                // Call base class before closing TCP client
                //
                base.Dispose(disposing);

                //
                // Close the TCP client
                //
                Logfile.Write(STRLOG_ClosingTcpClient);
                this.tcpClientStream.Close();
                this.tcpClient.Close();

                this.disposed = true;
            }

            Logfile.WriteCompleted(this.logLevel, STRLOG_ClassName, STRLOG_MethodName);
        }

        #endregion

        //=================================================================================================//

        protected override bool SendData(byte[] data, int dataLength)
        {
            bool success = false;

            try
            {
                //
                // Write the data to the serial LCD on the network
                //
                NetworkStream client = this.tcpClient.GetStream();
                client.Write(data, 0, dataLength);
                success = true;
            }
            catch (Exception ex)
            {
                Logfile.WriteError(ex.Message);
            }

            return success;
        }

        //-------------------------------------------------------------------------------------------------//

        private class AsyncObject
        {
            public NetworkStream tcpClientStream = null;
            public const int BUFFER_SIZE = 128;
            public byte[] receiveBuffer = new byte[BUFFER_SIZE];
        }

        //-------------------------------------------------------------------------------------------------//

        private void ReceiveCallback(IAsyncResult asyncResult)
        {
            try
            {
                //
                // Retrieve the state object and client TCP stream and read the data
                //
                AsyncObject obj = (AsyncObject)asyncResult.AsyncState;
                int bytesRead = obj.tcpClientStream.EndRead(asyncResult);
                Trace.WriteLine("ReceiveCallback: bytesRead=" + bytesRead.ToString());

                //
                // Pass data on for processing
                //
                this.ReceiveData(obj.receiveBuffer, bytesRead);

                //
                // Begin receiving more data from the SerialLCD
                //
                obj.tcpClientStream.BeginRead(obj.receiveBuffer, 0, AsyncObject.BUFFER_SIZE,
                    new AsyncCallback(ReceiveCallback), this.asyncObject);
            }
            catch (IOException)
            {
                // Network stream has been closed
            }
            catch (ObjectDisposedException)
            {
                // Network stream has been closed
            }
            catch (Exception ex)
            {
                Logfile.WriteError("ReceiveCallback: " + ex.ToString());
            }
        }

    }
}
