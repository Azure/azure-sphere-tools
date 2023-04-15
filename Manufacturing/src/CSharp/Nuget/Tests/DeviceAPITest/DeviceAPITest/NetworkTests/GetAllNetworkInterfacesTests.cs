/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using Json.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TestDeviceRestAPI.NetworkTests
{
    /// <summary>
    /// A test class for the get all network interfaces endpoint.
    /// </summary>
    [TestClass]
    public class GetAllNetworkInterfacesTests
    {
        /// <summary>Tests if getting all network interfaces, returns interfaces in the correct format.</summary>
        [TestMethod]
        public void GetNetworkInterfaces_Get_ReturnsInterfacesInCorrectFormat()
        {
            JsonSchema interfacesSchema = JsonSchema.FromText("{\"type\": \"object\",\"properties\": {\"interfaceName\": {\"type\":\"string\"}, \"interfaceUp\": {\"type\":\"boolean\"},\"connectedToNetwork\": {\"type\":\"boolean\"},\"ipAcquired\": {\"type\":\"boolean\"},\"connectedToInternet\": {\"type\":\"boolean\"},\"ipAddresses\": {\"type\":\"array\"}, \"hardwareAddress\": {\"type\":\"string\"}, \"ipAssignment\": {\"type\":\"string\"}}, \"oneOf\": [{\"required\": [\"interfaceName\", \"interfaceUp\", \"connectedToNetwork\", \"ipAcquired\", \"connectedToInternet\", \"ipAddresses\"]}, {\"required\": [\"interfaceName\", \"interfaceUp\", \"connectedToNetwork\", \"ipAcquired\", \"connectedToInternet\", \"hardwareAddress\", \"ipAssignment\"]}]}");

            Network.SetNetworkInterfaces("wlan0", true);
            string response =
                Network.GetAllNetworkInterfaces();

            foreach (JsonObject inter in (JsonArray)JsonNode.Parse(response).AsObject()["interfaces"])
            {
                Assert.IsTrue(interfacesSchema.Evaluate(inter).IsValid);
            }
        }
    }
}
