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

namespace Samples.Datalayer.MQTT.Pub
{
    /// <summary>
    /// Handler for the Pub node
    /// </summary>
    internal class MqttPubNodeHandler : MqttBaseNodeHandler
    {
        /// <summary>
        /// Creates a handler
        /// </summary>
        /// <param name="root"></param>
        /// <param name="parent"></param>
        public MqttPubNodeHandler(MqttRootNodeHandler root, MqttBaseNodeHandler parent) :
            base(root, parent, "pub")
        {
            //Add a PUB configuration just for demonstration
            Handlers.Add(new MqttPubConfigNodeHandler(root, this, $"{Names.ConfigPrefix}{Handlers.Count}"));
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

            //create
            (result, node) = Root.Provider.CreateVariableNode(this, FullAddress, Names.Create, Variant.False, writeable: true);
            if (result.IsBad())
            {
                return DLR_RESULT.DL_FAILED;
            }
            Nodes.Add(node.Address, node);

            return base.Start();
        }

        /// <summary>
        /// OnWrite handler
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

            //We're only interested in changed values
            var trimmedWriteValue = writeValue.Trim();
            if (wrappedNode.Value == trimmedWriteValue)
            {
                result.SetResult(DLR_RESULT.DL_FAILED);
                return;
            }

            switch (wrappedNode.Name)
            {
                //create
                case Names.Create:

                    if (!writeValue.IsBool)
                    {
                        result.SetResult(DLR_RESULT.DL_FAILED);
                        return;
                    }

                    //Check for write value 'true'
                    if (!writeValue.ToBool())
                    {
                        result.SetResult(DLR_RESULT.DL_FAILED);
                        return;
                    }

                    //Create new Pub config handler
                    var pubNodeHandler = new MqttPubConfigNodeHandler(Root, this, $"{Names.ConfigPrefix}{Handlers.Count}");
                    Handlers.Add(pubNodeHandler);
                    var startResult = pubNodeHandler.Start();

                    //Reset
                    wrappedNode.Value = Variant.False;

                    //Success
                    result.SetResult(startResult.IsGood() ? DLR_RESULT.DL_OK : DLR_RESULT.DL_FAILED);
                    return;
            }

            //Failed
            base.OnWrite(address, writeValue, result);
        }
    }

    #endregion
}
