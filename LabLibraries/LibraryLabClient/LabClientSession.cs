using System;
using System.Xml;

namespace Library.LabClient
{
    public class LabClientSession
    {
        public bool multiSubmit;
        public string bannerTitle;
        public string statusVersion;
        public string navmenuPhotoUrl;
        public string labCameraUrl;
        public string labInfoText;
        public string labInfoUrl;
        public string mailtoUrl;
        public XmlNode xmlNodeLabConfiguration;
        public XmlNode xmlNodeConfiguration;
        public XmlNode xmlNodeValidation;
        public XmlNode xmlNodeSpecification;
        public LabClientToSbAPI labClientToSbAPI;
    }
}
