using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicSenActAppCoreWebApi
{
    public class OperationLog
    {
        public List<Operation> Log { get; set; }

        public OperationLog()
        {
            Log = new List<Operation>();
        }
    }

    public class Operation
    {
        public string Name { get; set; }
        public int Number { get; set; } 
        public bool ResponseReady { get; set; } //da bi lider video da li da procita state

        public override bool Equals(object obj)
        {
            var operation = obj as Operation;
            return operation != null &&
                   Number == operation.Number &&
                   Name.Equals(operation.Name);
        }
    }

    //public class OperationDictionary
    //{
    //    Dictionary<string, Delegate> dictionary;

    //    public Delegate GetOperation(string key)
    //    {
    //        if (dictionary.ContainsKey(key))
    //            return dictionary[key];
    //        else
    //            return null;
    //    }

    //    public void SetOperation(string key, Delegate value)
    //    {
    //        if (dictionary.ContainsKey(key))
    //            dictionary[key] = value;
    //        else
    //            dictionary.Add(key, value);
    //    }
    //}
}
