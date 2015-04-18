using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using uPLibrary.Networking.M2Mqtt;
using uPLibrary.Networking.M2Mqtt.Utility;
using ArchestrA.MxAccess;
using Newtonsoft.Json;
using aaMXItem;

[assembly: log4net.Config.XmlConfigurator(ConfigFile = "log.config", Watch = true)]

namespace aaMQTT
{
    class Program
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static MqttClient _mqttClient;

        // LMX Interface declarations
        private static ArchestrA.MxAccess.LMXProxyServerClass _LMX_Server;

        // handle of registered LMX server interface
        private static int _hLMX;

        //dictionary of tag handles and tag names
        private static Dictionary<int, string> _MXAccessTagDictionary;

        //object for MXAccess Settings
        private static localMXAccessSettings _MXAccessSettings; 

        static void Main(string[] args)
        {
            try
            {
                log.Info("Starting " + System.AppDomain.CurrentDomain.FriendlyName);

                _MXAccessTagDictionary = new Dictionary<int, string>();

                ConnectMQTT();
                ConnectMXAccess();

                Console.ReadKey();
                
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
            finally
            {
                // Always disconnect on shutdown
                DisconnectMQTT();
                DisconnectMXAccess();
            }            
        }

        private static void DisconnectMQTT()
        {
            try
            {
                log.Info("Disconnecting MQTT Client");
                _mqttClient.Disconnect();
            }
            catch(Exception ex)
            {
                log.Error(ex);
            }
        }

        static void ConnectMQTT()
        {
            try
            {

                localMQTTSettings mqttSettings = JsonConvert.DeserializeObject<localMQTTSettings>(System.IO.File.ReadAllText("mqttsettings.json"));

                log.Info("Connecting to MQTT " + mqttSettings.host + ":" + mqttSettings.port.ToString());
                _mqttClient = new MqttClient(mqttSettings.host, mqttSettings.port, false, null);

                if (mqttSettings.clientid == "")
                {                    
                    // Generate new client id if it is blank in the mqttSettings file
                    mqttSettings.clientid = Environment.MachineName + System.Guid.NewGuid().ToString();

                    log.Info("Generated new client id " + mqttSettings.clientid);

                    // Write the mqttSettings back after generating a new GUID
                    System.IO.File.WriteAllText("mqttsettings.json", JsonConvert.SerializeObject(mqttSettings));
                }

                // Make the connection by logging in and specifying client id
                log.Info("Logging in with client ID " + mqttSettings.clientid + " and username " + mqttSettings.username);
                _mqttClient.Connect(mqttSettings.clientid, mqttSettings.username, mqttSettings.password);
                
                _mqttClient.MqttMsgPublishReceived += _mqttClient_MqttMsgPublishReceived;

                log.Info("MQTT connection status is " + _mqttClient.IsConnected.ToString());
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        static void ConnectMXAccess()
        {

            int hitem;

            try
            {
                if (_LMX_Server == null)
                {
                    // instantiate an ArchestrA.MxAccess.LMXProxyServer
                    try
                    {
                        _LMX_Server = new ArchestrA.MxAccess.LMXProxyServerClass();
                    }
                    catch (Exception ex)
                    {
                        throw ex;
                    }
                }

                if ((_LMX_Server != null) && (_hLMX == 0))
                {
                    // Register with LMX and get the Registration handle
                    _hLMX = _LMX_Server.Register("aaDataForwarderApp");

                    // connect the event handlers
                    _LMX_Server.OnDataChange += new _ILMXProxyServerEvents_OnDataChangeEventHandler(LMX_OnDataChange);
                    _LMX_Server.OnWriteComplete += new _ILMXProxyServerEvents_OnWriteCompleteEventHandler(LMX_OnWriteComplete);
                }

                // Read in the MX Access Settings from the configuration file
                _MXAccessSettings = JsonConvert.DeserializeObject<localMXAccessSettings>(System.IO.File.ReadAllText("mxaccess.json"));

                // Loop through all of the tags and add
                foreach (string tagname in _MXAccessSettings.publishtags)
                {
                    log.Info("Adding Publish for " + tagname);
                    hitem = _LMX_Server.AddItem(_hLMX, tagname);

                    if(hitem > 0)
                    {
                        _MXAccessTagDictionary.Add(hitem, tagname);
                        _LMX_Server.Advise(_hLMX, hitem);
                    }
                }

                if (MQTTOK())
                {
                    // Loop through all of the tags we are subscribing to and get those on supervisory advise so we can perform writes
                    foreach (subscription sub in _MXAccessSettings.subscribetags)
                    {
                        log.Info("Adding Subscribe for " + sub.writetag);
                        hitem = _LMX_Server.AddItem(_hLMX, sub.writetag);

                        if (hitem > 0)
                        {
                            sub.hitem = hitem;
                            _MXAccessTagDictionary.Add(hitem, sub.writetag);
                            _LMX_Server.AdviseSupervisory(_hLMX, hitem);

                            // Add MQTT Subscription
                            if (_mqttClient.IsConnected)
                            {
                                byte[] qosLevels = { 1 };
                                string[] topics = { sub.topic };
                                _mqttClient.Subscribe(topics, qosLevels);
                            }
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        static void DisconnectMXAccess()
        {
            log.Info("Disconnecting MXAccess");

            // Remove all items 
            foreach(int hitem in _MXAccessTagDictionary.Keys)
            {
                _LMX_Server.UnAdvise(_hLMX, hitem);
                _LMX_Server.RemoveItem(_hLMX, hitem);                
            }

            // Unregister
            _LMX_Server.Unregister(_hLMX);
        }

        private static void LMX_OnWriteComplete(int hLMXServerHandle, int phItemHandle, ref MXSTATUS_PROXY[] pVars)
        {
            try
            {
                log.Debug("Write complete for " + _MXAccessTagDictionary[phItemHandle].ToString());
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }
        
        private static void LMX_OnDataChange(int hLMXServerHandle, int phItemHandle, object pvItemValue, int pwItemQuality, object pftItemTimeStamp, ref MXSTATUS_PROXY[] pVars)
        {
            try
            {
                DateTime ts;
                ts = DateTime.Parse(pftItemTimeStamp.ToString());

                aaMXItem.MXItem newItem = new aaMXItem.MXItem();

                newItem.ItemHandle = phItemHandle;
                newItem.Quality = pwItemQuality;
                newItem.ServerHandle = hLMXServerHandle;
                //newItem.TagName = _MXAccessTagDictionary[phItemHandle].Replace('[', '_').Replace(']', ' ').Trim();
                newItem.TagName = _MXAccessTagDictionary[phItemHandle];
                newItem.TimeStamp = DateTime.Parse(ts.ToString());
                newItem.Value = pvItemValue;

                log.Debug("Received update for " + newItem.TagName);

                // Verify we are itnerested in publishing this update
                if (_MXAccessSettings.publishtags.IndexOf(newItem.TagName) > -1)
                {
                    PublishMXItem(newItem);
                }

            }
            catch (Exception ex)
            {
                log.Error(ex);
            }
        }

        private static void PublishMXItem(MXItem mxItem)
        {
            string topic;
            try
            {   
                if(MQTTOK())
                {
                topic = "/" + _MXAccessSettings.roottopic + "/" + mxItem.TagName.Replace('[', '_').Replace(']', ' ').Trim() + "/value";

                // Publish the value change
                _mqttClient.Publish(topic, System.Text.Encoding.UTF8.GetBytes(mxItem.Value.ToString()));

                log.Debug("Published updated to " + topic);
                }

            }
            catch(Exception ex)
            {
                log.Error(ex);
            }

        }

        private static void _mqttClient_MqttMsgPublishReceived(object sender, uPLibrary.Networking.M2Mqtt.Messages.MqttMsgPublishEventArgs e)
        {
            try
            {
                // Extract topic
                string topic = e.Topic;

                //Find matching topic in subscription list and get the write tag
                int hitem = _MXAccessSettings.subscribetags.Find(x => x.topic.Equals(topic)).hitem;

                //Now find the tag in the dictionary to get the correct hitem
                _LMX_Server.Write(_hLMX, hitem, System.Text.ASCIIEncoding.UTF8.GetString(e.Message), 0);
            }
            catch (Exception ex)
            {
                log.Error(ex);
            }

        }

        private static bool MQTTOK()
        {
            return (_mqttClient != null) & (_mqttClient.IsConnected);
        }
    }
}
