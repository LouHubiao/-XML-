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
            XMLAnalysisClass analysis = new XMLAnalysisClass("bookstore.xml");
            analysis.AddNode("extraPK", "type", "INT", "NULL", "NOT NULL", "isPK", "true");
            analysis.AddNode("name", "type", "VARCHAR", "NULL", "NOT NULL", "isPK", "false");
            analysis.AddNode("age", "type", "INT", "NULL", "NOT NULL", "isPK", "false");
            analysis.AddNodeValues("1", "louhubiao", "23");
            analysis.AddNodeValues("2", "cuiziki", "23");
            analysis.DeleteNode("1");
            analysis.UpdateNode("2", "age", "22");
            string findValue = analysis.SearchNode("2", "name");
            Console.WriteLine(findValue);
            analysis.CleanFile();
            analysis.document.Save("bookstore.xml");
        }

        class XMLAnalysisClass
        {
            #region 全局初始化
            //
            public XmlDocument document;
            //属性list，保存预设的属性名和属性值选项；
            public PropertyList propertys;
            //XML文档的根；
            public XmlElement rootElement;
            //常量主键列名
            public const string EXTRAPK = "extraPK";

            public XMLAnalysisClass(string filePath)
            {
                document = new XmlDocument();
                propertys = new PropertyList();
                document.Load(filePath);    //load时要小心，文件不对就会抛出异常；
                rootElement = document.DocumentElement;
            }

            #endregion

            #region 工具函数
            //根据列名获取对应xml中的元素节点，参数列名；
            XmlElement GetElementFromColumName(string theColumnName)
            {
                if (rootElement.FirstChild != null)
                {
                    XmlNodeList nodeList = rootElement.ChildNodes;
                    foreach (XmlNode node in nodeList)
                    {
                        if (node.Name == theColumnName)
                        {
                            return (XmlElement)node;
                        }
                    }
                }
                return null;
            }

            //根据属性名得到属性XmlAttribute，参数节点，属性名
            XmlAttribute GetAttributeFromPropName(XmlElement element, string propertyName)
            {
                foreach (XmlAttribute attribute in element.Attributes)
                {
                    if (attribute.Name == propertyName)
                    {
                        return attribute;
                    }
                }
                return null;
            }

            //验证属性名是否合法，参数属性名，属性值；
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

            //验证插入数据是否合法，参数插入节点，插入值；
            bool IsAddValueLegal(XmlElement element, string theValue)
            {
                foreach (XmlAttribute attribute in element.Attributes)
                {
                    if (attribute.Name == "type")
                    {
                        if (attribute.Value == "INT")
                        {
                            int result;
                            if (int.TryParse(theValue, out result) == false) { Console.WriteLine("IsAddValueLegal: error int value:%s", theValue); return false; }
                        }
                        else if (attribute.Name == "DATE")
                        {
                            DateTime dt;
                            if (DateTime.TryParse(theValue, out dt) == false) { Console.WriteLine("IsAddValueLegal: error date value:%s", theValue); return false; }
                        }
                    }
                    else if (attribute.Name == "NULL" && attribute.Value == "NOT NULL")
                    {
                        if (theValue == null || theValue == "NULL") { Console.WriteLine("IsAddValueLegal: error unable NULL"); return false; } 
                    }
                    else if (attribute.Name == "isPK"&&attribute.Value=="true")
                    {
                        foreach (XmlNode valueNode in element.ChildNodes)
                        {
                            if (valueNode.Value == theValue) { Console.WriteLine("IsAddValueLegal: error PK value:%s",theValue); return false; }
                        }
                    }
                }
                return true;
            }

            //根据传入的额外表主键找到XML文件中的位置（计数），参数主键节点，主键值；
            int GetCountByKeyExtraPK(string key)
            {
                XmlElement element = GetElementFromColumName(EXTRAPK);
                XmlNodeList nodelist = element.ChildNodes;
                int count = 0;
                for (int i = 0; i < nodelist.Count; i++)
                {
                    if (nodelist[i].InnerText == key)
                        break;
                    else
                        count++;
                }
                if (count == nodelist.Count) { Console.WriteLine("GetCountByKeyExtraPK: error key:%s", key); return -1; }
                return count;
            }

            //根据计数值找到其他节点中的对应数值
            XmlElement GetElementByCount(XmlElement element, int count)
            {
                XmlNodeList nodelist = element.ChildNodes;
                if (nodelist[count] != null)
                {
                    return (XmlElement)nodelist[count];
                }
                else { Console.WriteLine("GetValueByCount: error count:%d",count); return null; }
            }

            #endregion

            #region 属性处理
            //添加节点属性，包括类型，是否允许空，是否为主键
            public void AddProperty(string theColumnName, string propertyName, string propertyValue)
            {
                XmlElement element = GetElementFromColumName(theColumnName);
                if (element == null) { Console.WriteLine("AddProperty: error theColumnName:%s", theColumnName); return; }
                if (IsPropertyLegal(propertyName, propertyValue) == true)
                {
                    foreach (XmlAttribute attribute in element.Attributes)
                    {
                        if (attribute.Name == propertyName) { Console.WriteLine("AddProperty: error propertyName:%s",propertyName); return; }
                    }
                    element.SetAttribute(propertyName, propertyValue);
                }
                else { Console.WriteLine("AddProperty:error propertyName:%s or propertyValue:%s", propertyName, propertyValue); return; }
            }

            //修改属性值
            public void UpdateProperty(XmlNode root, string theColumnName, string propertyName, string propertyValue)
            {
                XmlElement element = GetElementFromColumName(theColumnName);
                if (element == null) { Console.WriteLine("UpdateProperty: error theColumnName:%s", theColumnName); return; }
                if (IsPropertyLegal(propertyName, propertyValue) == true)
                {
                    XmlAttribute attribute = GetAttributeFromPropName(element, propertyName);
                    if (attribute == null) { Console.WriteLine("UpdateProperty:error propertyName:%s", propertyName); return; }
                    attribute.InnerText = propertyValue;
                }
                else { Console.WriteLine("UpdateProperty:error propertyName:%s or propertyValue:%s", propertyName, propertyValue); return; }
            }

            //删除节点属性
            public void DeleteProperty(XmlNode root, string theColumnName, string propertyName)
            {
                XmlElement element = GetElementFromColumName(theColumnName);
                if (element == null) { Console.WriteLine("DeleteProperty: error theColumnName:%s", theColumnName); return; }
                XmlAttribute attribute = GetAttributeFromPropName(element, propertyName);
                if (attribute == null) { Console.WriteLine("DeleteProperty:error propertyName:%s", propertyName); return; }
                element.RemoveAttribute(propertyName);
            }

            #endregion

            #region 增删改查

            //添加节点，参数节点名和节点属性；
            public void AddNode(string theColumnName, params string[] propertyParams)
            {
                if (theColumnName == null) { Console.WriteLine("AddNodeName: error the theColumnName is null"); return; }
                XmlElement column = document.CreateElement(theColumnName);
                rootElement.AppendChild(column);
                //XmlElement value = document.CreateElement("Value");
                //column.AppendChild(value);

                if (propertyParams == null) return;
                if (propertyParams.Length % 2 != 0) { Console.WriteLine("AddNode: error propertys.Length % 2 != 0 "); return; }
                for (int i = 0; i < propertyParams.Length; i = i + 2)
                {
                    AddProperty(theColumnName, propertyParams[i], propertyParams[i + 1]);
                }
            }
            
            //添加一行
            public void AddNodeValues(params string[] valuePropertys)
            {
                XmlNodeList nodeList = rootElement.ChildNodes;
                if (valuePropertys.Length != nodeList.Count) { Console.WriteLine("AddNodeValues: error valuePropertys.Length:%d", valuePropertys.Length); return; }
                for(int i=0;i<valuePropertys.Length;i++)
                {
                    AddNodeValue(nodeList[i].Name, valuePropertys[i]);
                }
            }

            //添加节点的值，参数新增列名，新增值；
            public void AddNodeValue(string theColumnName, string theValue)
            {
                XmlElement element = GetElementFromColumName(theColumnName);
                if (IsAddValueLegal(element, theValue) == true)
                {
                    XmlElement value = document.CreateElement("Value");
                    value.InnerText = theValue;
                    element.AppendChild(value);                    
                }
                else { Console.WriteLine("AddNodeValue:error theValue:%s", theValue); return; }
            }

            //删除节点，参数主键值；
            public void DeleteNode(string key)
            {
                int count = GetCountByKeyExtraPK(key);
                if (count == -1) { Console.WriteLine("DeleteNode: error key:%s", key); }
                XmlNodeList nodeList = rootElement.ChildNodes;
                foreach (XmlNode node in nodeList)
                {
                    node.RemoveChild(node.ChildNodes[count]);
                }
            }

            //更新节点，参数主键值，列名，更新值；
            public void UpdateNode(string key, string theColumnName, string theValue)
            {
                int count = GetCountByKeyExtraPK(key);
                if (count == -1) { Console.WriteLine("UpdateNode: error key:%s", key); return; }
                XmlElement element = GetElementFromColumName(theColumnName);
                if (element == null) { Console.WriteLine("UpdateNode:error theColumnName:%s", theColumnName); return; }
                if (IsAddValueLegal(element, theValue) == true)
                {
                    element.ChildNodes[count].InnerText = theValue;
                }
                else { Console.WriteLine("UpdateNode:error theValue:%s", theValue); return; }
            }

            //查找值，参数列名；
            public string SearchNode(string key, string theColumnName)
            {
                int count = GetCountByKeyExtraPK(key);
                if (count == -1) { Console.WriteLine("SearchNode: error key:%s", key); return null; }
                XmlElement element = GetElementFromColumName(theColumnName);
                if (element == null) { Console.WriteLine("SearchNode:error theColumnName:%s", theColumnName); return null; }
                return element.ChildNodes[count].InnerText;
            }

            #endregion

            //清除xml文件中除了根节点以外的所有节点，用于测试；
            public void CleanFile()
            {
                rootElement.RemoveAll();
            }

        }
    }
}
