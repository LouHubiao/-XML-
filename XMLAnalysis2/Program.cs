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
            analysis.AddNode(root, "column1", "123", "type", "INT", "NULL", "NOT NULL");
            analysis.AddNode(root, "column2", "hello", "type", "VARCHAR", "NULL", "NULL");
            analysis.DeleteNode(root, "column1");
            analysis.UpdateNode(root, "column2", "lou");
            string findValue = analysis.SearchNode(root, "column2");
            Console.WriteLine(findValue);
            analysis.CleanFile(root);
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
                document.Load(filePath);    //load时要小心，文件不对就会抛出异常；
                XmlNode root = (XmlNode)document.DocumentElement;
                return root;
            }

            #region 工具函数
            //根据列名获取对应xml中的元素节点；
            XmlElement getElementFromColumName(XmlNode root, string theColumnName)
            {
                if (root.FirstChild != null)
                {
                    XmlNodeList nodeList = root.ChildNodes;
                    foreach (XmlNode node in nodeList)
                    {
                        XmlElement element = (XmlElement)node;
                        if (element.Name == theColumnName)
                        {
                            return element;
                        }
                    }
                }
                return null;
            }

            //验证属性名是否合法
            bool IsPropertyLegal(string propertyName, string propertyValue)
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

            //验证插入数据是否合法
            bool IsAddValueLegal(XmlElement element, string theValue)
            {
                foreach (XmlAttribute attribute in element.Attributes)
                {
                    if (attribute.Name == "type")
                    {
                        if (attribute.Value == "INT")
                        {
                            int result;
                            if (int.TryParse(theValue, out result) == false)
                            {
                                return false;
                            }
                        }
                        else if (attribute.Name == "DATE")
                        {
                            DateTime dt;
                            if (DateTime.TryParse(theValue, out dt) == false)
                            {
                                return false;
                            }
                        }
                    }
                    else if (attribute.Name == "NULL" && attribute.Value == "NOT NULL")
                    {
                        if (theValue == null || theValue == "NULL")
                        {
                            return false;
                        } 
                    }
                    else if (attribute.Name == "isPK"&&attribute.Value=="true")
                    {
                        foreach (XmlNode valueNode in element.ChildNodes)
                        {
                            if (valueNode.Value == theValue)
                            {
                                return false;
                            }
                        }
                    }
                }
                return true;
            }

            #endregion

            #region 属性处理
            //添加节点属性，包括类型，是否允许空，是否为主键
            public void AddProperty(XmlNode root, string theColumnName, string propertyName, string propertyValue)
            {
                XmlElement element = getElementFromColumName(root, theColumnName);
                if (element == null) { Console.WriteLine("AddProperty: error theColumnName:%s", theColumnName); return; }
                if (IsPropertyLegal(propertyName, propertyValue) == true)
                {
                    foreach (XmlAttribute attribute in element.Attributes)
                    {
                        if (attribute.Name == propertyName) { Console.WriteLine("AddProperty: error the propertyName:%s has exsit",propertyName); return; }
                    }
                    element.SetAttribute(propertyName, propertyValue);
                }
                else { Console.WriteLine("UpdateProperty:propertyName:%s or propertyValue:%s is illegal", propertyName, propertyValue); return; }
            }

            //修改属性值
            public void UpdateProperty(XmlNode root, string theColumnName, string propertyName, string propertyValue)
            {
                XmlElement element = getElementFromColumName(root, theColumnName);
                if (element == null) { Console.WriteLine("UpdateProperty: error theColumnName:%s", theColumnName); return; }
                if (IsPropertyLegal(propertyName, propertyValue) == true)
                {
                    if (element.Attributes[propertyName] != null)
                    {
                        element.Attributes[propertyName].InnerText = propertyValue;
                    }
                    else { Console.WriteLine("UpdateProperty:error propertyName:%s", propertyName); }
                }
                else { Console.WriteLine("UpdateProperty:propertyName:%s or propertyValue:%s is illegal", propertyName, propertyValue); return; }
            }

            //删除节点属性
            public void DeleteProperty(XmlNode root, string theColumnName, string propertyName)
            {
                XmlElement element = getElementFromColumName(root, theColumnName);
                if (element == null) { Console.WriteLine("DeleteProperty: error theColumnName:%s", theColumnName); return; }
                foreach (XmlAttribute attribute in element.Attributes)
                {
                    if (attribute.Name == propertyName)
                    {
                        element.RemoveAttribute(propertyName);
                        return;
                    }
                }
                Console.WriteLine("DeleteProperty: error propertyName:%s", propertyName);
            }

            #endregion

            #region 增删改查

            //添加节点，分三个步骤；
            public void AddNode(XmlNode root, string theColumnName, string theValue, params string[] propertyParams)
            {
                AddNodeName(root, theColumnName);
                if (propertyParams.Length % 2 != 0) { Console.WriteLine("AddNode: error propertys.Length % 2 != 0 "); return; }
                for (int i = 0; i < propertyParams.Length; i = i + 2)
                {
                    AddProperty(root, theColumnName, propertyParams[i], propertyParams[i + 1]);
                }
                AddNodeValue(root, theColumnName, theValue);
            }

            //添加节点的主干；
            void AddNodeName(XmlNode root, string theColumnName)
            {
                if (theColumnName == null){ Console.WriteLine("AddNodeName: error the theColumnName is null"); return; }   
                XmlElement column = document.CreateElement(theColumnName);
                XmlElement value = document.CreateElement("Value");
                column.AppendChild(value);
                root.AppendChild(column);
            }

            //添加节点的值；
            void AddNodeValue(XmlNode root, string theColumnName, string theValue)
            {
                XmlElement element = getElementFromColumName(root, theColumnName);
                if (IsAddValueLegal(element, theValue) == true)
                {
                    XmlNode lastNode = element.LastChild;
                    lastNode.InnerText = theValue;
                }
                else { Console.WriteLine("AddNodeValue:the value:%s is illegal", theValue); return; }
            }

            //删除节点，参数列名；
            public void DeleteNode(XmlNode root, string theColumnName)
            {
                XmlElement element = getElementFromColumName(root, theColumnName);
                if (element == null){ Console.WriteLine("DeleteNode:error theColumnName:%s", theColumnName); return;}
                XmlElement value = (XmlElement)element.FirstChild;
                if (value.Name == "Value")
                {
                    if (value.InnerText != null)
                        value.InnerText = null;
                }
                else { Console.WriteLine("DeleteNode:no the Value element"); return; }
            }

            //更新节点，参数列名，更新值；
            public void UpdateNode(XmlNode root, string theColumnName, string theValue)
            {
                XmlElement element = getElementFromColumName(root, theColumnName);
                if (element != null)
                {
                    XmlElement value = (XmlElement)element.FirstChild;
                    value.InnerText = theValue;
                }
                else{ Console.WriteLine("UpdateNode:error theColumnName:%s", theColumnName); return;}
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
                else{ Console.WriteLine("SearchNode:error theColumnName:%s", theColumnName); return null;}
            }

            #endregion

            //清除xml文件中除了根节点以外的所有节点，用于测试；
            public void CleanFile(XmlNode root)
            {
                XmlElement rootElement = (XmlElement)root;
                rootElement.RemoveAll();
            }

        }
    }
}
