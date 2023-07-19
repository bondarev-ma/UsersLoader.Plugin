using PhoneApp.Domain.DTO;
using PhoneApp.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using PhoneApp.Domain.Attributes;
using System.Xml.Linq;
using Newtonsoft.Json.Linq;

namespace UsersLoader.Plugin
{
    [Author(Name = "Max Bondarev")]
    public class Plugin : IPluggable
    {
        private static readonly NLog.Logger logger = NLog.LogManager.GetCurrentClassLogger();
        public IEnumerable<DataTransferObject> Run(IEnumerable<DataTransferObject> args)
        {
            logger.Info("Loading users");

            var usersList = new List<EmployeesDTO>();

            using (var client = new HttpClient(new HttpClientHandler { AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate }))
            {
                client.BaseAddress = new Uri("https://dummyjson.com/");
                HttpResponseMessage response = client.GetAsync("users?limit=5").Result;
                if (response.IsSuccessStatusCode)
                {
                    string result = response.Content.ReadAsStringAsync().Result;
                    var parsedObject = JObject.Parse(result);
                    foreach (var user in parsedObject["users"])
                    {
                        var newUser = new EmployeesDTO { Name = $"{user["firstName"]} {user["lastName"]}" };
                        newUser.AddPhone((string)user["phone"]);
                        usersList.Add(newUser);
                    }
                    logger.Info($"Loaded {usersList.Count()} users");
                }
                else
                {
                    logger.Info($"Can't get users. Response: {response.StatusCode}");
                }
            }

            return usersList.Cast<DataTransferObject>();
        }
    }
}
