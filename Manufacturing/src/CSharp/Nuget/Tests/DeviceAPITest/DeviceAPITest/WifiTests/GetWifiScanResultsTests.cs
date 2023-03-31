/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using Json.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TestDeviceRestAPI.WifiTests
{
    /// <summary>
    /// A test class for the get wifi scan endpoint.
    /// </summary>
    [TestClass]
    public class GetWifiScanResultsTests
    {

        /// <summary>Tests if get wifi scan returns wifi networks of the expected format.<summary>
        [TestMethod]
        public void GetWifiScan_Get_ReturnsValidFormat()
        {
            JsonSchema valuesSchema =
                JsonSchema.FromText("{\"type\":\"object\", \"properties\": {\"values\":{\"type\":\"array\"}}}");
            JsonSchema wifiSchema =
                JsonSchema.FromText("{\"type\":\"object\", \"properties\": {\"bssid\":{\"type\":\"string\"}, \"freq\":{\"type\":\"integer\"}, \"signal_level\":{\"type\":\"integer\"}, \"ssid\":{\"type\":\"string\"}, \"securityState\":{\"type\":\"string\"}}}");

            string response = Wifi.GetWiFiScan();
            JsonNode parsedObject = JsonNode.Parse(response);

            Assert.IsTrue(valuesSchema.Evaluate(parsedObject).IsValid);

            foreach (JsonObject wifis in (JsonArray)parsedObject["values"])
            {
                Assert.IsTrue(wifiSchema.Evaluate(wifis).IsValid);
            }
        }
    }
}
