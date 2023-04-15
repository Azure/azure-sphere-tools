/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using Json.Schema;
using System.Text.Json.Nodes;
namespace TestDeviceRestAPI.NetworkTests
{
    /// <summary>
    /// A test class for the get network status endpoint.
    /// </summary>
    [TestClass]
    public class GetNetworkStatusTests
    {
        /// <summary>Tests if getting the network status returns the expected values.</summary>
        [TestMethod]
        public void NetworkStatus_Get_ReturnsExpectedStatus()
        {
            string response = Network.GetNetworkStatus();

            JsonSchema responseSchema =
                JsonSchema.FromText("{\"type\": \"object\",\"properties\":{\"deviceAuthenticationIsReady\": {\"type\": \"boolean\"},\"networkTimeSync\": {\"type\": \"string\"},\"proxy\": {\"type\": \"string\"}}}");

            JsonNode parsedObject = JsonNode.Parse(response);
            Assert.IsTrue(responseSchema.Evaluate(parsedObject).IsValid);
        }
    }
}
