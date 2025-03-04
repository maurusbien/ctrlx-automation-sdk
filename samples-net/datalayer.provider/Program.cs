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

namespace Samples.Datalayer.Provider
{
    using comm.datalayer;
    using FlatBuffers;
    using sample.schema;
    using System;
    using System.IO;
    using System.Net;
    using System.Threading;

    /// <summary>
    /// Defines the <see cref="Program" />.
    /// </summary>
    internal class Program
    {
        // This is the connection string for TCP in the format: tcp://USER:PASSWORD@IP_ADDRESS:DATALAYER_PORT?sslport=SSL_PORT
        // Please check and change according your environment:
        // - USER:        Enter your user name here - default is boschrexroth
        // - PASSWORD:    Enter your password here - default is boschrexroth
        // - IP_ADDRESS:
        //   127.0.0.1    If you develop on your (Windows) host and you want to connect to a ctrlX CORE virtual with port forwarding
        //   10.0.2.2     If you develop on a VM (QEMU, Virtual Box) and you want to connect to a ctrlX virtual with port forwarding
        //   192.168.1.1  If you are using a ctrlX CORE or ctrlX CORE virtual with TAP adpater
        // - DATALAYER_PORT:
        //   2069         The ctrlX Data Layer client port
        //   2070         The ctrlX Data Layer provider port
        // - SSL_PORT:
        //   443          Default SSL Port if you are using a ctrlX CORE or ctrlX CORE virtual with TAP adpater
        //   8443         Default forwarded SSL Port if you are using a ctrlX CORE virtual

        // Please change the following constants according to your environment
        private const string USER = "boschrexroth";
        private const string PASSWORD = "boschrexroth";
        private const string IP_ADDRESS = "10.0.2.2";
        private const int SSL_PORT = 8443;

        // Please define the node root folder in ctrlX Data Layer
        private const string ROOT = "samples/dotnet";

        /// <summary>
        /// The Main.
        /// </summary>
        /// <param name="args">The args<see cref="string[]"/>.</param>
        internal static void Main(string[] args)
        {
            //Add app exit handler to handle optional clean up
            AppDomain.CurrentDomain.ProcessExit += CurrentDomain_ProcessExit;

            // Check if the process is running inside a snap 
            var isSnapped = !string.IsNullOrEmpty(Environment.GetEnvironmentVariable("SNAP"));
            Console.WriteLine(value: $"Running inside snap: {isSnapped}");

            // Create a new ctrlX Data Layer system
            using var system = new DatalayerSystem();

            // Starts the ctrlX Data Layer system without a new broker (startBroker = false) because one broker is already running on ctrlX device
            system.Start(startBroker: false);
            Console.WriteLine("ctrlX Data Layer system started.");

            // Set remote connection string for provider with inter-process communication (ipc) protocol if running in snap, otherwise tcp
            var remote = isSnapped ? "ipc://" : $"tcp://{USER}:{PASSWORD}@{IP_ADDRESS}:2070?sslport={SSL_PORT}";

            // Create the provider with remote connection string
            using var provider = system.Factory.CreateProvider(remote);
            Console.WriteLine("ctrlX Data Layer provider created.");

            // Create and register node on given address and read-only callbacks.
            var myIntNode = CreateReadOnlyNode($"{ROOT}/myInt", "types/datalayer/int32", "MyInt Description", new Variant(42));
            var (resultMyInt, _) = provider.RegisterNode(myIntNode.Address, new ReadOnlyNodeHandler(myIntNode));
            Console.WriteLine($"Registering Node with address='{myIntNode.Address}', result='{resultMyInt}'");

            // Create and register node on given address and read-only callbacks.
            var myDoubleNode = CreateReadOnlyNode($"{ROOT}/myDouble", "types/datalayer/float64", "MyDouble Description", new Variant(Math.PI));
            var (resultMyDouble, _) = provider.RegisterNode(myDoubleNode.Address, new ReadOnlyNodeHandler(myDoubleNode));
            Console.WriteLine($"Registering Node with address='{myDoubleNode.Address}', result='{resultMyDouble}'");

            // Create and register node on given address and read-write callbacks.
            var myStringNode = CreateReadWriteNode($"{ROOT}/myString", "types/datalayer/string", "MyDouble Description", new Variant("Hello ctrlX"));
            var (resultMyString, _) = provider.RegisterNode(myStringNode.Address, new ReadWriteNodeHandler(myStringNode));
            Console.WriteLine($"Registering Node with address='{myStringNode.Address}', result='{resultMyString}'");

            // Register type with binary flatbuffers schema file: sampleSchema.bfbs (auto generated from sampleSchema.fbs by flatc compiler)
            const string typeAddressInertialValue = "types/sample/schema/inertial-value";
            var resultRegisterType = provider.RegisterType(typeAddressInertialValue, Path.Combine(AppContext.BaseDirectory, "sampleSchema.bfbs"));
            Console.WriteLine($"Registering Type with address='{typeAddressInertialValue}', result='{resultRegisterType}'");

            // Create flatbuffers value with FlatbufferBuilder (flatbuffers c# API) and InertialValue (auto generated from sampleSchema.fbs by flatc compiler) 
            var builder = new FlatBufferBuilder(Variant.DefaultFlatbuffersInitialSize);
            var offset = InertialValue.CreateInertialValue(builder, 30, -442, 911);
            builder.Finish(offset.Value);
            var variantFlatbuffers = new Variant(builder);

            // Create and register node on given address and read-only callbacks.
            var myFlatbuffersNode = CreateReadOnlyNode($"{ROOT}/inertial-value", typeAddressInertialValue, "My Inertial Value Description", variantFlatbuffers);
            var (resultMyFlatbuffers, _) = provider.RegisterNode(myFlatbuffersNode.Address, new ReadOnlyNodeHandler(myFlatbuffersNode));
            Console.WriteLine($"Registering Node with address='{myFlatbuffersNode.Address}', result='{resultMyFlatbuffers}'");

            // Start the Provider
            var startResult = provider.Start();

            // Check if provider is connected.
            Console.WriteLine(value: $"Provider connected: {provider.IsConnected}");

            if (!provider.IsConnected)
            {
                //if not we exit and retry after app daemon restart-delay (see snapcraft.yaml)
                Console.WriteLine($"Restarting app after restart-delay of 10 s ...");
                return;
            }

            // Check if provider is started.
            Console.WriteLine(value: $"Provider started: {startResult}");

            // Just keep the process running
            while (true)
            {
                if (!provider.IsConnected)
                {
                    Console.WriteLine("Provider is disconnected: skip changing values of dynamic nodes.");
                }

                Thread.Sleep(millisecondsTimeout: 1000);
            }
        }

        /// <summary>
        /// The CurrentDomain_ProcessExit.
        /// </summary>
        /// <param name="sender">The sender<see cref="object"/>.</param>
        /// <param name="e">The e<see cref="EventArgs"/>.</param>
        private static void CurrentDomain_ProcessExit(object sender, EventArgs e)
        {
            Console.WriteLine("Application exit");
        }

        /// <summary>
        /// The CreateNode.
        /// </summary>
        /// <param name="address">The address<see cref="string"/>.</param>
        /// <param name="targetAddress">The targetAddress<see cref="string"/>.</param>
        /// <param name="description">The description<see cref="string"/>.</param>
        /// <param name="value">The value<see cref="IVariant"/>.</param>
        /// <returns>The <see cref="Node"/>.</returns>
        public static Node CreateReadOnlyNode(string address, string targetAddress, string description, IVariant value)
        {
            var metaData = new MetadataBuilder(AllowedOperationFlags.Read, description)
                .SetNodeClass(NodeClass.Variable)
                .AddReference(ReferenceType.ReadType, targetAddress)
                .Build();

            return new Node(address, value, metaData);
        }

        /// <summary>
        /// The CreateReadWriteNode.
        /// </summary>
        /// <param name="address">The address<see cref="string"/>.</param>
        /// <param name="targetAddress">The targetAddress<see cref="string"/>.</param>
        /// <param name="description">The description<see cref="string"/>.</param>
        /// <param name="value">The value<see cref="IVariant"/>.</param>
        /// <returns>The <see cref="Node"/>.</returns>
        public static Node CreateReadWriteNode(string address, string targetAddress, string description, IVariant value)
        {
            var metaData = new MetadataBuilder(AllowedOperationFlags.Read | AllowedOperationFlags.Write, description)
                .SetNodeClass(NodeClass.Variable)
                .AddReference(ReferenceType.ReadType, targetAddress)
                .AddReference(ReferenceType.WriteType, targetAddress)
                .Build();

            return new Node(address, value, metaData);
        }
    }
}
