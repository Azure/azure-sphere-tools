/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using System.Management.Automation;
using Json.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TestDeviceRestAPI.WifiTests
{
    /// <summary>
    /// A test class for the get wifi interface state endpoint.
    /// </summary>
    [TestClass]
    public class GetWifiInterfaceStateTests
    {
        /// <summary>Tests if getting the wifi interface state returns an interface of the expected format.<summary>
        [TestMethod]
        public void GetWifiInterface_Get_ReturnsInterfaceExpectedFormat()
        {
            JsonSchema stateSchema = null;
            if (SemanticVersion.Parse(SinceDeviceAPIVersion.GetDeviceApiVersion()) >= SemanticVersion.Parse("4.6.0"))
            {
                stateSchema =
                    JsonSchema.FromText("{\"type\":\"object\", \"properties\": {\"configState\":{\"type\":\"string\"}, \"connectionState\":{\"type\":\"string\"}, \"securityState\":{\"type\":\"string\"}, \"mode\":{\"type\":\"string\"}, \"key_mgmt\":{\"type\":\"string\"}, \"wpa_state\":{\"type\":\"string\"}, \"address\":{\"type\":\"string\"}, \"id\":{\"type\":\"integer\"}, \"powerSavingsState\":{\"type\":\"string\"}}}");
            }
            else
            {
                stateSchema =
                    JsonSchema.FromText("{\"type\":\"object\", \"properties\": {\"configState\":{\"type\":\"string\"}, \"connectionState\":{\"type\":\"string\"}, \"securityState\":{\"type\":\"string\"}, \"mode\":{\"type\":\"string\"}, \"key_mgmt\":{\"type\":\"string\"}, \"wpa_state\":{\"type\":\"string\"}, \"address\":{\"type\":\"string\"}, \"id\":{\"type\":\"integer\"}}}");
            }


            string response = Wifi.GetWiFiInterfaceState();

            JsonNode parsedObject = JsonNode.Parse(response);
            Assert.IsTrue(stateSchema.Evaluate(parsedObject).IsValid);
        }
    }
}
