using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NightFisionAutomatedPrintAndPickList.Services
{
    internal class ConfigManager
    {
        private readonly string _configFile;

        private readonly ILogger<Worker> _logger;

        public ConfigManager(ILogger<Worker> logger, string configFile)
        {
            _logger = logger;
            _configFile = configFile;
        }

        public XmlNodeList GetTasks()
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_configFile);

            return doc.SelectNodes("//Tasks/Task");
        }

        public string GetEmailSettings(string name, string key)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_configFile);
            XmlNode clientNode = doc.SelectSingleNode($"//Email[Name='{name}']");

            if (clientNode != null)
            {
                return clientNode.SelectSingleNode(key)?.InnerText;
            }
            else
            {
                _logger.LogError("[CONFIG MANAGER] Email settings not found for {key} :", key);
                throw new Exception("[CONFIG_MANAGER] Email settings not found for : " + key);
            }

            return null;
        }

        public string GetClientSettings(string name, string key)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_configFile);
            XmlNode clientNode = doc.SelectSingleNode($"//Client[Name='{name}']");

            if (clientNode != null)
            {
                return clientNode.SelectSingleNode(key)?.InnerText;
            } 
            else
            {
                _logger.LogError("[CONFIG MANAGER] Client settings not found for {key} :", key);
                //throw new Exception("[CONFIG_MANAGER] Client settings not found for : " + key);
            }

            return null;
        }

        public string GetTaskSettings(string name, string key)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(_configFile);
            XmlNode clientNode = doc.SelectSingleNode($"//Tasks/Task[TaskName='{name}']");

            if (clientNode != null)
            {
                XmlNode taskActionNode = clientNode.SelectSingleNode(key);

                if (taskActionNode != null) 
                {
                    return taskActionNode.InnerText;
                }
                
            }
            else
            {
                throw new Exception("[CONFIG_MANAGER] Task client node not found for : " + name);
            }

            return "";
        }


        public void SetTimeRetrieved(string name, string value)
        {
            using (FileStream stream = new FileStream(_configFile, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(_configFile);
                XmlNode node = doc.SelectSingleNode($"//Tasks/Task[TaskName='{name}']");

                if (node != null)
                {
                    XmlNode lastOfficialTimeNode = node.SelectSingleNode("LastOfficialTimeRetrieved");
                    if (lastOfficialTimeNode != null)
                    {
                        lastOfficialTimeNode.InnerText = value;
                        doc.Save(_configFile);
                    }
                }
            }
        }
    }
}
