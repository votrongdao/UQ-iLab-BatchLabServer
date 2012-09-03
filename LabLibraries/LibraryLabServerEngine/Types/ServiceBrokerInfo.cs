using System;

namespace Library.LabServerEngine
{
    public class ServiceBrokerInfo
    {
            /// <summary>
            /// ServiceBroker's name, typically an alias to identify the ServiceBroker by name.
            /// </summary>
            public string name;

            /// <summary>
            /// ServiceBroker's GUID, typically a 32 hexadecimal character string.
            /// </summary>
            public string guid;

            /// <summary>
            /// The passkey sent to the LabServer in the SOAP header object. The passkey
            /// identifies the calling ServiceBroker to the LabServer.
            /// </summary>
            public string outgoingPasskey;

            /// <summary>
            /// The passkey sent to the ServiceBroker in the SOAP header object. The passkey
            /// identifies the calling LabServer to the ServiceBroker.
            /// </summary>
            public string incomingPasskey;

            /// <summary>
            /// URL of the ServiceBroker that will be notified of experiment completion
            /// </summary>
            public string webServiceUrl;

            /// <summary>
            /// If true, the LabServer allows calls from the ServiceBroker to the LabServer's
            /// WebMethods.
            /// </summary>
            public bool isAllowed;

            public ServiceBrokerInfo()
            {
            }

            public ServiceBrokerInfo(string name, string guid, string outgoingPasskey,
                string incomingPasskey, string webServiceUrl, bool isAllowed)
            {
                this.name = name;
                this.guid = guid;
                this.outgoingPasskey = outgoingPasskey;
                this.incomingPasskey = incomingPasskey;
                this.webServiceUrl = webServiceUrl;
                this.isAllowed = isAllowed;
            }
    }
}
