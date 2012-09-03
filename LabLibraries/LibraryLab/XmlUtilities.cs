using System;
using System.IO;
using System.Xml;

namespace Library.Lab
{
    public class XmlUtilities
    {
        //
        // String constants for error messages
        //
        private const string STRERR_XmlString = "xmlString";
        private const string STRERR_LoadXmlStringFailed = "Failed to load XML string -> ";
        private const string STRERR_LoadXmlFileFailed = "Failed to load XML file -> ";
        private const string STRERR_XmlNodeNotFound = "Xml node not found!";
        private const string STRERR_XmlNodeIsEmpty = "Xml node is empty!";
        private const string STRERR_XmlNodeListIsEmpty = "Xml node list is empty!";
        private const string STRERR_XmlInvalidNumber = "Invalid number!";
        private const string STRERR_XmlInvalidBoolean = "Invalid boolean!";
        private const string STRERR_XmlNodeListIndexInvalid = "Xml nodelist index is out of range!";
        private const string STRERR_ValueIsNull = "Value is null!";

        //-------------------------------------------------------------------------------------------------//

        public static XmlDocument GetXmlDocument(string xmlString)
        {
            //
            // Check that the XML string is not null
            //
            if (xmlString == null)
            {
                throw new ArgumentNullException(STRERR_XmlString);
            }

            //
            // Load the XML string into a document
            //
            XmlDocument xmlDocument = new XmlDocument();
            try
            {
                xmlDocument.LoadXml(xmlString);
            }
            catch (Exception ex)
            {
                throw new Exception(STRERR_LoadXmlStringFailed + ex.Message);
            }

            return xmlDocument;
        }

        //-------------------------------------------------------------------------------------------------//

        public static XmlDocument GetXmlDocumentFromFile(string filename)
        {
            //
            // Try to load the XML file, may not exist
            //
            XmlTextReader xmlTextReader = null;
            XmlDocument xmlDocument = null;
            try
            {
                xmlTextReader = new XmlTextReader(filename);
                xmlDocument = new XmlDocument();
                xmlDocument.Load(xmlTextReader);
            }
            catch (FileNotFoundException)
            {
                // File does not exist, but that's ok
                xmlDocument = null;
            }
            catch (XmlException ex)
            {
                throw new XmlException(STRERR_LoadXmlFileFailed + ex.Message);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (xmlTextReader != null)
                {
                    xmlTextReader.Close();
                }
            }

            return xmlDocument;
        }

        //-------------------------------------------------------------------------------------------------//

        public static XmlNode GetXmlRootNode(XmlDocument xmlDocument, string strXmlNode)
        {
            // Remove whitespace and prepend root character
            strXmlNode = "/" + strXmlNode.Trim();

            XmlNode xmlNodeTemp = xmlDocument.SelectSingleNode(strXmlNode);
            if (xmlNodeTemp == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            return xmlNodeTemp;
        }

        //-------------------------------------------------------------------------------------------------//

        public static XmlNode GetXmlNode(XmlDocument xmlDocument, string strXmlNode)
        {
            return GetXmlNode(xmlDocument, strXmlNode, null);
        }

        //-------------------------------------------------------------------------------------------------//

        public static XmlNode GetXmlNode(XmlDocument xmlDocument, string strXmlNode, XmlNamespaceManager xnsManager)
        {
            XmlNode xmlNodeTemp;
            if (xnsManager != null)
            {
                xmlNodeTemp = xmlDocument.SelectSingleNode(strXmlNode, xnsManager);
            }
            else
            {
                xmlNodeTemp = xmlDocument.SelectSingleNode(strXmlNode);
            }
            if (xmlNodeTemp == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            return xmlNodeTemp;
        }

        //-------------------------------------------------------------------------------------------------//

        public static XmlNode GetXmlNode(XmlNode xmlNode, string strXmlNode)
        {
            XmlNode xmlNodeTemp = xmlNode.SelectSingleNode(strXmlNode);
            if (xmlNodeTemp == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            return xmlNodeTemp;
        }

        //-------------------------------------------------------------------------------------------------//

        public static XmlNode GetXmlNode(XmlNode xmlNode, string strXmlNode, bool allowNull)
        {
            XmlNode xmlNodeTemp = xmlNode.SelectSingleNode(strXmlNode);
            if (allowNull == false && xmlNodeTemp == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            return xmlNodeTemp;
        }

        //-------------------------------------------------------------------------------------------------//

        public static XmlNodeList GetXmlNodeList(XmlNode xmlNode, string strXmlNode, bool allowEmpty)
        {
            XmlNodeList xmlNodeList = xmlNode.SelectNodes(strXmlNode);
            if (xmlNodeList == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            //
            // Check if list is empty
            //
            if (allowEmpty == false && xmlNodeList.Count == 0)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeListIsEmpty);
            }

            return xmlNodeList;
        }

        //-------------------------------------------------------------------------------------------------//

        public static string GetXmlValue(XmlNode xmlNode, string strXmlNode, bool allowEmpty)
        {
            return GetXmlValue(xmlNode, strXmlNode, null, allowEmpty);
        }

        //-------------------------------------------------------------------------------------------------//

        public static string GetXmlValue(XmlNode xmlNode, string strXmlNode, XmlNamespaceManager xnsManager, bool allowEmpty)
        {
            //
            // Get the specified node
            //
            XmlNode xmlNodeTemp;
            if (xnsManager != null)
            {
                xmlNodeTemp = xmlNode.SelectSingleNode(strXmlNode, xnsManager);
            }
            else
            {
                xmlNodeTemp = xmlNode.SelectSingleNode(strXmlNode);
            }
            if (xmlNodeTemp == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            //
            // Get node's value
            //
            string innerXml = xmlNodeTemp.InnerXml.Trim();

            //
            // Check if value is empty
            //
            if (allowEmpty == false && innerXml.Length == 0)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeIsEmpty);
            }

            return innerXml;
        }

        //-------------------------------------------------------------------------------------------------//

        public static string[] GetXmlValues(XmlNode xmlNode, string strXmlNode, bool allowEmpty)
        {
            string[] valueList = null;

            //
            // Get the list of nodes
            //
            XmlNodeList xmlNodeList = xmlNode.SelectNodes(strXmlNode);
            if (xmlNodeList == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            //
            // Fill in values from the node list
            //
            if (xmlNodeList.Count > 0)
            {
                valueList = new string[xmlNodeList.Count];
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    // Get node's value
                    string innerXml = xmlNodeList.Item(i).InnerXml.Trim();

                    //
                    // Check if value is empty
                    //
                    if (allowEmpty == false && innerXml.Length == 0)
                    {
                        throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeIsEmpty);
                    }

                    // Save value
                    valueList[i] = innerXml;
                }
            }

            return valueList;
        }

        //-------------------------------------------------------------------------------------------------//

        public static char GetCharValue(XmlNode xmlNode, string strXmlNode, char defaultValue)
        {
            char ch = defaultValue;
            try
            {
                ch = GetCharValue(xmlNode, strXmlNode);
            }
            catch
            {
            }

            return ch;
        }

        //-------------------------------------------------------------------------------------------------//

        public static char GetCharValue(XmlNode xmlNode, string strXmlNode)
        {
            //
            // Get node's string value
            //
            string value = GetXmlValue(xmlNode, strXmlNode, false);

            //
            // Convert to a char
            //
            char ch = value[0];

            return ch;
        }

        //-------------------------------------------------------------------------------------------------//

        public static int GetIntValue(XmlNode xmlNode, string strXmlNode, int defaultValue)
        {
            int number = defaultValue;
            try
            {
                number = GetIntValue(xmlNode, strXmlNode);
            }
            catch
            {
            }

            return number;
        }

        //-------------------------------------------------------------------------------------------------//

        public static int GetIntValue(XmlNode xmlNode, string strXmlNode)
        {
            //
            // Get node's string value
            //
            string value = GetXmlValue(xmlNode, strXmlNode, false);

            //
            // Convert to a number
            //
            int number = 0;
            try
            {
                number = Int32.Parse(value);
            }
            catch
            {
                throw new ArgumentException(strXmlNode, STRERR_XmlInvalidNumber);
            }

            return number;
        }

        //-------------------------------------------------------------------------------------------------//

        public static double GetRealValue(XmlNode xmlNode, string strXmlNode, double defaultValue)
        {
            double number = defaultValue;
            try
            {
                number = GetRealValue(xmlNode, strXmlNode);
            }
            catch
            {
            }

            return number;
        }

        //-------------------------------------------------------------------------------------------------//

        public static double GetRealValue(XmlNode xmlNode, string strXmlNode)
        {
            //
            // Get node's string value
            //
            string value = GetXmlValue(xmlNode, strXmlNode, false);

            //
            // Convert to a number
            //
            double number = 0.0;
            try
            {
                number = Double.Parse(value);
            }
            catch
            {
                throw new ArgumentException(strXmlNode, STRERR_XmlInvalidNumber);
            }

            return number;
        }

        //-------------------------------------------------------------------------------------------------//

        public static bool GetBoolValue(XmlNode xmlNode, string strXmlNode, bool allowEmpty)
        {
            //
            // Get node's string value
            //
            string value = GetXmlValue(xmlNode, strXmlNode, allowEmpty);

            //
            // Convert to a boolean
            //
            bool boolValue = false;
            try
            {
                boolValue = Boolean.Parse(value);
            }
            catch
            {
                throw new ArgumentException(strXmlNode, STRERR_XmlInvalidBoolean);
            }

            return boolValue;
        }

        //-------------------------------------------------------------------------------------------------//

        public static string GetXmlValue(XmlNodeList xmlNodeList, int index, string strXmlNode, bool allowEmpty)
        {
            if (index < 0 || index >= xmlNodeList.Count)
            {
                throw new ArgumentOutOfRangeException(STRERR_XmlNodeListIndexInvalid);
            }

            // Get node's value
            string innerXml = xmlNodeList.Item(index).InnerXml.Trim();

            //
            // Check if value is empty
            //
            if (allowEmpty == false && innerXml.Length == 0)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeIsEmpty);
            }

            return innerXml;
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetXmlValue(XmlNode xmlNode, string strXmlNode, string value, bool allowNull)
        {
            SetXmlValue(xmlNode, strXmlNode, null, value, allowNull);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetXmlValue(XmlNode xmlNode, string strXmlNode, XmlNamespaceManager xnsManager, string value, bool allowNull)
        {
            //
            // Get the specified node
            //
            XmlNode xmlNodeTemp;
            if (xnsManager != null)
            {
                xmlNodeTemp = xmlNode.SelectSingleNode(strXmlNode, xnsManager);
            }
            else
            {
                xmlNodeTemp = xmlNode.SelectSingleNode(strXmlNode);
            }
            if (xmlNodeTemp == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            //
            // Check if value is empty
            //
            if (allowNull == false && value == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_ValueIsNull);
            }

            //
            // Set node's value
            //
            if (value != null)
            {
                xmlNodeTemp.InnerXml = value.Trim();
            }
            else
            {
                xmlNodeTemp.InnerXml = string.Empty;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetXmlValue(XmlNode xmlNode, string strXmlNode, bool value)
        {
            SetXmlValue(xmlNode, strXmlNode, value.ToString(), false);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetXmlValue(XmlNode xmlNode, string strXmlNode, int value)
        {
            SetXmlValue(xmlNode, strXmlNode, value.ToString(), false);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetXmlValue(XmlNode xmlNode, string strXmlNode, double value)
        {
            SetXmlValue(xmlNode, strXmlNode, value.ToString(), false);
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetXmlValues(XmlNode xmlNode, string strXmlNode, string[] valueList, bool allowNull)
        {
            //
            // Check if value list is empty
            //
            if (allowNull == false && (valueList == null || valueList.Length == 0))
            {
                throw new ArgumentNullException(strXmlNode, STRERR_ValueIsNull);
            }

            // Get the list of nodes
            XmlNodeList xmlNodeList = XmlUtilities.GetXmlNodeList(xmlNode, strXmlNode, true);

            if (xmlNodeList.Count > 0 && valueList != null)
            {
                string outerXml = null;
                for (int i = 0; i < valueList.Length; i++)
                {
                    //
                    // Check if value is empty
                    //
                    if (allowNull == false && valueList[i] == null)
                    {
                        throw new ArgumentNullException(strXmlNode, STRERR_ValueIsNull);
                    }

                    //
                    // Set node's value
                    //
                    xmlNodeList.Item(0).InnerXml = valueList[i].Trim();
                    outerXml += xmlNodeList.Item(0).OuterXml;
                }

                // Update node
                xmlNode.InnerXml = outerXml;
            }
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetXmlValues(XmlNode xmlNode, string strXmlNode, int[] valueList, char splitter, bool allowNull)
        {
            //
            // Get the specified node
            //
            XmlNode xmlNodeTemp = xmlNode.SelectSingleNode(strXmlNode);
            if (xmlNodeTemp == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            //
            // Check if value list is empty
            //
            if (allowNull == false && (valueList == null || valueList.Length == 0))
            {
                throw new ArgumentNullException(strXmlNode, STRERR_ValueIsNull);
            }

            //
            // Create the vector string
            //
            StringWriter dataVector = new StringWriter();
            for (int i = 0; i < valueList.Length; i++)
            {
                if (i == 0)
                {
                    dataVector.Write(valueList[i].ToString());
                }
                else
                {
                    dataVector.Write("{0}{1}", splitter, valueList[i].ToString());
                }
            }

            //
            // Set node's value
            //
            xmlNodeTemp.InnerXml = dataVector.ToString();
        }

        //-------------------------------------------------------------------------------------------------//

        public static void SetXmlValues(XmlNode xmlNode, string strXmlNode, float[] valueList, string format, char splitter, bool allowNull)
        {
            //
            // Get the specified node
            //
            XmlNode xmlNodeTemp = xmlNode.SelectSingleNode(strXmlNode);
            if (xmlNodeTemp == null)
            {
                throw new ArgumentNullException(strXmlNode, STRERR_XmlNodeNotFound);
            }

            //
            // Check if value list is empty
            //
            if (allowNull == false && (valueList == null || valueList.Length == 0))
            {
                throw new ArgumentNullException(strXmlNode, STRERR_ValueIsNull);
            }

            //
            // Create the vector string
            //
            StringWriter dataVector = new StringWriter();
            for (int i = 0; i < valueList.Length; i++)
            {
                if (i == 0)
                {
                    dataVector.Write(valueList[i].ToString(format));
                }
                else
                {
                    dataVector.Write("{0}{1}", splitter, valueList[i].ToString(format));
                }
            }

            //
            // Set node's value
            //
            xmlNodeTemp.InnerXml = dataVector.ToString();
        }

        //-------------------------------------------------------------------------------------------------//

        public static string ToXmlString(XmlNode xmlNode)
        {
            string xmlString = string.Empty;

            StringWriter sw = new StringWriter();
            XmlTextWriter xtw = new XmlTextWriter(sw);
            xtw.Formatting = Formatting.Indented;
            xmlNode.WriteTo(xtw);
            xtw.Flush();

            xmlString = sw.ToString();

            return xmlString;
        }

    }
}
