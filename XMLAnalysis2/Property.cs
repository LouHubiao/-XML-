using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace XMLAnalysis2
{
    class PropertyList
    {
        public List<Property> propertyList;

        public PropertyList()
        {
            string[] type = { "type", "INT", "VARCHAR", "DATE" };
            string[] nullable = { "NULL", "NULL", "NOT NULL" };
            string[] isPK = { "isPK", "true", "false" };
            List<string[]> propertys = new List<string[]>();
            propertys.Add(type);
            propertys.Add(nullable);
            propertyList = new List<Property>();
            foreach (string[] property in propertys) {
                string propertyName = property[0];
                List<string> propertyValues = new List<string>();
                for (int i = 1; i < property.Length; i++)
                {
                    propertyValues.Add(property[i]);
                }
                Property newProperty = new Property(propertyName, propertyValues);
                propertyList.Add(newProperty);
            }
        }
    }

    class Property
    {
        public string propertyName;
        public List<string> propertyValues;

        public Property(string propertyName, List<string> propertyValues) {
            this.propertyName = propertyName;
            this.propertyValues = propertyValues;
        }
    }

}
