﻿/*
MIT License

Copyright (c) 2021 Bosch Rexroth AG

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using Datalayer;
using Samples.Datalayer.MQTT.Base;
using System;
using System.Net;
using System.Threading.Tasks;

namespace Samples.Datalayer.MQTT.Client
{
    /// <summary>
    /// Handler for client node
    /// </summary>
    internal class MqttClientNodeHandler : MqttBaseNodeHandler
    {
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        // !!! CHANGE THIS TO YOUR ENVIRONMENT !!!
        // !!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!
        private static readonly IPAddress BrokerAddress = IPAddress.Parse("192.168.1.200");
        private static readonly string Username = "";
        private static readonly string Password = "";

        /// <summary>
        /// Creates a handler
        /// </summary>
        /// <param name="root"></param>
        /// <param name="parent"></param>
        public MqttClientNodeHandler(MqttRootNodeHandler root, MqttBaseNodeHandler parent)
            : base(root, parent, "client")
        {
            //Add MQTT Client connected event handler
            Root.MqttClient.Connected += args =>
            {
                UpdateStatus(DLR_RESULT.DL_OK);
            };

            //Add MQTT Client disconnected event handler
            Root.MqttClient.Disconnected += async args =>
            {
                UpdateStatus(DLR_RESULT.DL_CLIENT_NOT_CONNECTED);

                if (IsShutdown)
                {
                    return;
                }

                //Reconnect if not shutdown and connection lost
                Console.WriteLine($"Connection Lost. Reconnecting after {TimeoutMillis} [ms] ...");
                await Task.Delay(TimeSpan.FromMilliseconds(TimeoutMillis));
                await ConnectMqttAsync();
            };
        }

        #region Overrides

        /// <summary>
        /// Starts the handler
        /// </summary>
        /// <returns></returns>
        public override DLR_RESULT Start()
        {
            //Create, register and add the handled nodes here

            //Folder (self)
            var (result, node) = Root.Provider.CreateBranchNode(this, BaseAddress, Name);
            if (result.IsBad())
            {
                return DLR_RESULT.DL_FAILED;
            }
            Nodes.Add(node.Address, node);

            //...

            //server-address
            (result, node) = Root.Provider.CreateVariableNode(this, FullAddress, Names.ServerAddress, new Variant(BrokerAddress.ToString()));
            if (result.IsBad())
            {
                return DLR_RESULT.DL_FAILED;
            }
            Nodes.Add(node.Address, node);

            //port
            (result, node) = Root.Provider.CreateVariableNode(this, FullAddress, Names.Port, new Variant(Root.MqttClient.Port));
            if (result.IsBad())
            {
                return DLR_RESULT.DL_FAILED;
            }
            Nodes.Add(node.Address, node);

            //client-id
            (result, node) = Root.Provider.CreateVariableNode(this, FullAddress, Names.ClientId, new Variant(MqttClientWrapper.DefaultClientId()));
            if (result.IsBad())
            {
                return DLR_RESULT.DL_FAILED;
            }
            Nodes.Add(node.Address, node);

            //username
            (result, node) = Root.Provider.CreateVariableNode(this, FullAddress, Names.Username, new Variant(Username));
            if (result.IsBad())
            {
                return DLR_RESULT.DL_FAILED;
            }
            Nodes.Add(node.Address, node);

            //password
            (result, node) = Root.Provider.CreateVariableNode(this, FullAddress, Names.Password, new Variant(Password));
            if (result.IsBad())
            {
                return DLR_RESULT.DL_FAILED;
            }
            Nodes.Add(node.Address, node);

            //clean-session
            (result, node) = Root.Provider.CreateVariableNode(this, FullAddress, Names.CleanSession, Variant.False);
            if (result.IsBad())
            {
                return DLR_RESULT.DL_FAILED;
            }
            Nodes.Add(node.Address, node);

            //communication-timeout
            (result, node) = Root.Provider.CreateVariableNode(this, FullAddress, Names.CommunicationTimeoutMillis, new Variant(1_0000));
            if (result.IsBad())
            {
                return DLR_RESULT.DL_FAILED;
            }
            Nodes.Add(node.Address, node);

            //keep-alive-period
            (result, node) = Root.Provider.CreateVariableNode(this, FullAddress, Names.KeepAlivePeriodMillis, new Variant(1_0000));
            if (result.IsBad())
            {
                return DLR_RESULT.DL_FAILED;
            }
            Nodes.Add(node.Address, node);

            //status        
            (result, node) = Root.Provider.CreateVariableNode(this, FullAddress, Names.Status, ToStatus(DLR_RESULT.DL_FAILED), true);
            if (result.IsBad())
            {
                return DLR_RESULT.DL_FAILED;
            }
            Nodes.Add(node.Address, node);

            //Connect MQTT (async)          
            Task.Factory.StartNew(() => ConnectMqttAsync().Result);

            return base.Start();
        }

        /// <summary>
        /// Stops the handler
        /// </summary>
        /// <returns></returns>
        public override DLR_RESULT Stop()
        {
            //Disconnect MQTT (Sync)
            var result = DisconnectMqttAsync().Result;
            if (result.IsBad())
            {
                return result;
            }

            return base.Stop();
        }

        /// <summary>
        /// OnWrite event handler
        /// </summary>
        /// <param name="address"></param>
        /// <param name="writeValue"></param>
        /// <param name="result"></param>
        public override void OnWrite(string address, IVariant writeValue, IProviderNodeResult result)
        {
            //Fetch the node
            if (!Nodes.TryGetValue(address, out ProviderNodeWrapper wrappedNode))
            {
                result.SetResult(DLR_RESULT.DL_FAILED);
                return;
            }

            //Check for read-only nodes
            if (wrappedNode.IsReadOnly)
            {
                result.SetResult(DLR_RESULT.DL_FAILED);
                return;
            }

            var trimmedWriteValue = writeValue.Trim();
            if (wrappedNode.Value == trimmedWriteValue)
            {
                result.SetResult(DLR_RESULT.DL_FAILED);
                return;
            }

            switch (wrappedNode.Name)
            {
                case Names.ServerAddress:
                    if (writeValue.IsEmptyOrWhiteSpace())
                    {
                        result.SetResult(DLR_RESULT.DL_FAILED);
                        return;
                    }

                    wrappedNode.Value = trimmedWriteValue;
                    break;

                case Names.Port:
                    if (!writeValue.IsNumber)
                    {
                        result.SetResult(DLR_RESULT.DL_FAILED);
                        return;
                    }

                    //Validate write value
                    if (writeValue.ToInt32() <= 0)
                    {
                        result.SetResult(DLR_RESULT.DL_FAILED);
                        return;
                    }

                    wrappedNode.Value = writeValue;
                    break;

                case Names.ClientId:
                    if (writeValue.IsEmptyOrWhiteSpace())
                    {
                        result.SetResult(DLR_RESULT.DL_FAILED);
                        return;
                    }
                    wrappedNode.Value = trimmedWriteValue;
                    break;

                case Names.CleanSession:
                    if (!writeValue.IsBool)
                    {
                        result.SetResult(DLR_RESULT.DL_FAILED);
                        return;
                    }
                    wrappedNode.Value = writeValue;
                    break;

                case Names.Username:
                    if (!writeValue.IsString)
                    {
                        result.SetResult(DLR_RESULT.DL_FAILED);
                        return;
                    }
                    wrappedNode.Value = trimmedWriteValue;
                    break;

                case Names.Password:
                    if (!writeValue.IsString)
                    {
                        result.SetResult(DLR_RESULT.DL_FAILED);
                        return;
                    }
                    wrappedNode.Value = trimmedWriteValue;
                    break;

                case Names.KeepAlivePeriodMillis:
                    if (!writeValue.IsNumber)
                    {
                        result.SetResult(DLR_RESULT.DL_FAILED);
                        return;
                    }

                    //Validate write value
                    if (writeValue.ToInt32() <= 0)
                    {
                        result.SetResult(DLR_RESULT.DL_FAILED);
                        return;
                    }

                    wrappedNode.Value = writeValue;
                    break;

                case Names.CommunicationTimeoutMillis:
                    if (!writeValue.IsNumber)
                    {
                        result.SetResult(DLR_RESULT.DL_FAILED);
                        return;
                    }

                    //Validate write value
                    if (writeValue.ToInt32() <= 0)
                    {
                        result.SetResult(DLR_RESULT.DL_FAILED);
                        return;
                    }

                    wrappedNode.Value = writeValue;
                    break;
            }

            //Reconnect MQTT (async)        
            Task.Factory.StartNew(() => ReconnectMqttAsync().Result);

            //Success;
            result.SetResult(DLR_RESULT.DL_OK, writeValue);
        }

        #endregion

        #region Private

        /// <summary>
        /// Updates the status node, if existing
        /// </summary>
        /// <param name="result"></param>
        private DLR_RESULT UpdateStatus(DLR_RESULT result)
        {
            //Check if we have a status node
            if (!HasNode(Names.Status))
            {
                return result;
            }

            GetNode(Names.Status).Value = ToStatus(result);
            return result;
        }

        /// <summary>
        /// Connects to the MQTT broker
        /// </summary>
        /// <returns></returns>
        private async Task<DLR_RESULT> ConnectMqttAsync()
        {
            Root.MqttClient.ServerAddress = GetNode(Names.ServerAddress).Value.ToString();
            Root.MqttClient.Port = GetNode(Names.Port).Value.ToInt32();
            Root.MqttClient.ClientId = GetNode(Names.ClientId).Value.ToString();
            Root.MqttClient.CleanSession = GetNode(Names.CleanSession).Value.ToBool();
            Root.MqttClient.Username = GetNode(Names.Username).Value.ToString();
            Root.MqttClient.Password = GetNode(Names.Password).Value.ToString();
            Root.MqttClient.CommunicationTimeout = TimeSpan.FromMilliseconds(GetNode(Names.CommunicationTimeoutMillis).Value.ToInt32());
            Root.MqttClient.KeepAlivePeriod = TimeSpan.FromMilliseconds(GetNode(Names.KeepAlivePeriodMillis).Value.ToInt32());

            var task = Root.MqttClient.ConnectAsync();
            var result = await task;
            return UpdateStatus(result);
        }

        /// <summary>
        /// Disconnects from the MQTT broker
        /// </summary>
        /// <returns></returns>
        private async Task<DLR_RESULT> DisconnectMqttAsync()
        {
            var task = Root.MqttClient.DisconnectAsync();
            var result = await task;
            return UpdateStatus(result);
        }

        /// <summary>
        /// Reconnects to a MQTT broker
        /// </summary>
        /// <returns></returns>
        public async Task<DLR_RESULT> ReconnectMqttAsync()
        {
            //Disconnect (Discard result)
            var task = DisconnectMqttAsync();
            await task;

            //Connect
            task = ConnectMqttAsync();
            var result = await task;
            return result;
        }

        #endregion
    }
}
