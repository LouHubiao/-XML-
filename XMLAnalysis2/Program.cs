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
            analysis.AddProperty(root, "column2", "type", "INT");
            analysis.UpdateProperty(root, "column2", "type", "VARCHAR");
            analysis.DeleteProperty(root, "column2", "type");
            string findValue = analysis.SearchNode(root, "column2");
            Console.WriteLine(findValue);
            //analysis.CleanFile(root);
            analysis.document.Save("bookstore.xml");
        }

        class XMLAnalysisClass
        {
            //
            public XmlDocument document;
            //属性list，保存预设的属性名和属性值选项；
            public PropertyList propertys;
            
            public XMLAnalysisClass()
            {
                document = new XmlDocument();
                propertys = new PropertyList();
            }
            
            //打开xml文件，获取xml文件的根节点；
            public XmlNode ReadXMLFile(string filePath)
            {
                document.Load(filePath);
                XmlNode root = (XmlNode)document.DocumentElement;
                return root;
            }

            #region 工具函数
            //根据列名获取对应xml中的元素节点；
            public XmlElement getElementFromColumName(XmlNode root, string theColumnName)
            {
                if (root.FirstChild != null) { }
                XmlNodeList nodeList = root.ChildNodes;
                foreach (XmlNode node in nodeList)
                {
                    XmlElement element = (XmlElement)node;
                    if (element.Name == theColumnName)
                    {
                        return element;
                    }
                }
                Console.WriteLine("no this column");
                //exit(0);
                return null;
            }

            //验证属性名是否合法
            public bool IsPropertyLegal(string propertyName, string propertyValue)
            {
                foreach (Property property in propertys.propertyList)
                {
                    if (propertyName == property.propertyName)
                    {
                        foreach (string value in property.propertyValues)
                        {
                            if (propertyValue == value)
                            {
                                return true;
                            }
                        }
                    }
                }
                return false;
            }
            #endregion

            #region 属性处理
            //添加节点属性，包括类型，是否允许空，是否为主键
            public void AddProperty(XmlNode root, string theColumnName, string propertyName, string propertyValue)
            {
                if (IsPropertyLegal(propertyName, propertyValue) == true)
                {
                    
                    XmlElement element = getElementFromColumName(root, theColumnName);
                    foreach (XmlAttribute attribute in element.Attributes)
                    {
                        if (attribute.Name == propertyName)
                        {
                            Console.WriteLine("AddProperty: the property name has exsit");
                            return;
                        }
                    }
                    element.SetAttribute(propertyName, propertyValue);
                }
                else
                {
                    Console.WriteLine("AddProperty:propertyName or propertyValue is illegal");
                }
            }

            //修改属性值
            public void UpdateProperty(XmlNode root, string theColumnName, string propertyName, string propertyValue)
            {
                if (IsPropertyLegal(propertyName, propertyValue) == true)
                {
                    XmlElement element = getElementFromColumName(root, theColumnName);
                    if (element.Attributes[propertyName] != null)
                    {
                        element.Attributes[propertyName].InnerText = propertyValue;
                    }
                    else
                    {
                        Console.WriteLine("UpdateProperty:no this property name");
                    }
                }
                else
                {
                    Console.WriteLine("UpdateProperty:propertyName or propertyValue is illegal");
                }
            }

            public void DeleteProperty(XmlNode root, string theColumnName, string propertyName)
            {
                XmlElement element = getElementFromColumName(root, theColumnName);
                foreach (XmlAttribute attribute in element.Attributes)
                {
                    if (attribute.Name == propertyName)
                    {
                        element.RemoveAttribute(propertyName);
                        return;
                    }
                }
                Console.WriteLine("DeleteProperty: no this property");
            }

            #endregion

            #region 增删改查
            //添加节点的主干，参数节点值，后续添加节点属性；
            public void AddNode(XmlNode root, string theColumnName, string theValue)
            {
                XmlElement column = document.CreateElement(theColumnName);
                XmlElement value = document.CreateElement("Value");
                value.InnerText= theValue;
                column.AppendChild(value);
                root.AppendChild(column);
            }

            //删除节点，参数列名；
            public void DeleteNode(XmlNode root, string theColumnName)
            {
                XmlElement element = getElementFromColumName(root, theColumnName);
                XmlElement value = (XmlElement)element.FirstChild;
                if (value.Name == "Value")
                {
                    if (value.InnerText != null)
                        value.InnerText = null;
                }
                else
                {
                    Console.WriteLine("DeleteNode:no the VALUE element");
                }
            }

            //更新节点，参数列名；
            public void UpdateNode(XmlNode root, string theColumnName, string theValue)
            {
                XmlElement element = getElementFromColumName(root, theColumnName);
                if (element != null)
                {
                    XmlElement value = (XmlElement)element.FirstChild;
                    value.InnerText = theValue;
                }
                else
                {
                    Console.WriteLine("UpdateNode:no this node");
                }
            }

            //查找值，参数列名；
            public string SearchNode(XmlNode root, string theColumnName)
            {
                XmlElement element = getElementFromColumName(root, theColumnName);
                if (element != null)
                {
                    XmlElement value = (XmlElement)element.FirstChild;
                    return value.InnerText;
                }
                else
                {
                    Console.WriteLine("SearchNode:no this node");
                    return null;
                }
            }

            //清除xml文件中除了根节点以外的所有节点，用于测试；
            #endregion

            public void CleanFile(XmlNode root)
            {
                XmlElement rootElement = (XmlElement)root;
                rootElement.RemoveAll();
            }

        }
    }
}
