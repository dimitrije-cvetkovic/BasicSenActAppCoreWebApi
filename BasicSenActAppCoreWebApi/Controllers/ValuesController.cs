using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using dotnet_etcd;

namespace BasicSenActAppCoreWebApi.Controllers
{
    [Route("api/etcd")]
    public class ValuesController : Controller
    {
        string path = "./data/led.txt";
        EtcdClient client = new EtcdClient("192.168.0.13", 2379);

        [Route("putKey")]
        [HttpPost]
        public bool PutValue()
        {
            try
            {
                client.Put("foo/bar", "barfoo");
            }
            catch(Exception)
            {
                return false;
            }
            return true;
        }

        [Route("getKey")]
        [HttpGet]
        public string GetValue()
        {
            return client.GetVal("foo/bar");
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            List<string> retList = new List<string>();
            StreamReader f = null;
            try
            {
                if (!System.IO.File.Exists(path))
                {
                    retList.Add("Actuator still has not created a file for reading.");
                    return retList;
                }
                f = new StreamReader(new FileStream(path, FileMode.Open));
                string R = f.ReadLine();
                string G = f.ReadLine();
                string B = f.ReadLine();
                retList.Add(R);
                retList.Add(G);
                retList.Add(B);
                return retList;
            }
            catch(Exception)
            {
                return retList;
            }
            finally
            {
                if(f != null)
                    f.Close();
            }
        }
    }
}
