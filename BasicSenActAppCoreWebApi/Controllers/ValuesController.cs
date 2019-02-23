using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace BasicSenActAppCoreWebApi.Controllers
{
    [Route("api/ReadRGB")]
    public class ValuesController : Controller
    {
        string path = "./data/led.txt";
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
