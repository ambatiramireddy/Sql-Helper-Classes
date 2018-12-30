using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AddAppAPI.Helpers
{
    public class Parameter
    {
        public string Name { get; set; }
        public object Value { get; set; }
    }

    public class ParameterList
    {
        public List<Parameter> Parameters { get; set; } = new List<Parameter>();
        public void Add(string name, object value)
        {
            Parameters.Add(new Parameter { Name = name, Value = value });
        }

        public int Count
        {
            get
            {
                return Parameters.Count;
            }
        }
    }
}