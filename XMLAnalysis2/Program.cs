using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace XMLAnalysis2
{
    class Program
    {

        static void Main(string[] args)
        {
            XMLAnalysisClass analysis = new XMLAnalysisClass();
            XmlNode root = analysis.ReadXMLFile("bookstore.xml");
            analysis.AddNode(root, "column1", "123");
            analysis.AddNode(root, "column2", "hello");
            analysis.DeleteNode(root, "column1");
            analysis.UpdateNode(root, "column2", "lou");
            string findValue = analysis.SearchNode(root, "column2");
            Console.WriteLine(findValue);
            analysis.CleanFile(root);
            analysis.document.Save("bookstore.xml");
        }
        class XMLAnalysisClass
        {
            public XmlDocument document;
            public XMLAnalysisClass()
            {
                document = new XmlDocument();
            }
            public XmlNode ReadXMLFile(string filePath)
            {
                document.Load(filePath);
                XmlNode root = (XmlNode)document.DocumentElement;
                return root;
            }

            public void AddNode(XmlNode root, string theColumnName, string theValue)
            {
                XmlElement column = document.CreateElement(theColumnName);
                XmlElement value = document.CreateElement("Value");
                value.InnerText= theValue;
                column.AppendChild(value);
                root.AppendChild(column);
            }

            public void DeleteNode(XmlNode root, string theColumnName)
            {
                XmlNodeList nodeList = root.ChildNodes;
                foreach (XmlNode node in nodeList)
                {
                    XmlElement element = (XmlElement)node;
                    if (element.Name == theColumnName)
                    {
                        XmlElement value = (XmlElement)element.FirstChild;
                        if (value.InnerText != null)
                            value.InnerText = null;
                    }
                }
            }

            public void UpdateNode(XmlNode root, string theColumnName, string theValue)
            {
                XmlNodeList nodeList = root.ChildNodes;
                foreach (XmlNode node in nodeList)
                {
                    XmlElement element = (XmlElement)node;
                    if (element.Name == theColumnName)
                    {
                        XmlElement value = (XmlElement)element.FirstChild;
                        value.InnerText = theValue;
                    }
                }
            }

            public string SearchNode(XmlNode root, string theColumnName)
            {
                XmlNodeList nodeList = root.ChildNodes;
                foreach (XmlNode node in nodeList)
                {
                    XmlElement element = (XmlElement)node;
                    if (element.Name == theColumnName)
                    {
                        XmlElement value = (XmlElement)element.FirstChild;
                        return value.InnerText;
                    }
                }
                return null;
            }
            
            public void CleanFile(XmlNode root)
            {
                XmlElement rootElement = (XmlElement)root;
                rootElement.RemoveAll();
            }

        }
    }
}
