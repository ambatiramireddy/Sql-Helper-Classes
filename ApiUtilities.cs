using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace AddAppAPI.Helpers
{
    public class ApiUtilities
    {
        public static  T CreateInstance<T>() where T : ApiController, new()
        {
            var controller = new T();
            controller.Request = new HttpRequestMessage();
            controller.Configuration = new HttpConfiguration();

            return controller;
        }
    }
}