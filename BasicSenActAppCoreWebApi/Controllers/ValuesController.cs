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
        Algorithm dorado = new Algorithm(3);

        [Route("putKey")]
        [HttpPost]
        public string PutValue()
        {
            try
            {
                dorado.ExecuteRequest();
                return dorado.GenerateResponse().ToString();
            }
            catch(Exception e)
            {
                return e.Message + ":\n:" + e.InnerException + ":\n:" + e.StackTrace;
            }
        }

        [Route("getKey")]
        [HttpGet]
        public string GetValue()
        {
            string s = "";
            try
            {
                s = dorado.GetState().ToString();
            }
            catch (Exception e)
            {
                return e.Message + ":\n:" + e.InnerException + ":\n:" + e.StackTrace;
            }
            return s;
        }

        [Route("initialize")]
        [HttpGet]
        public string WatchRequest()
        {
            try
            {
                if (!dorado.ReqExists)
                    dorado.CreateRequest();
                dorado.WatchRequest();
                return "True";
            }
            catch (Exception e)
            {
                return e.Message + ":\n:" + e.InnerException + ":\n:" + e.StackTrace;
            }
        }
    }
}
