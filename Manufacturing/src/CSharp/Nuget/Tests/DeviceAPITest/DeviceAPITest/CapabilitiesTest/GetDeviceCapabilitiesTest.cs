/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using Json.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;
namespace TestDeviceRestAPI.ACapabilitiesTest
{
    /// <summary>
    /// A test class for the get device capabilities api endpoint.
    /// </summary>
    [TestClass]
    public class GetDeviceCapabilitiesTest
    {
        /// <summary>
        /// Tests if getting the device capabilities shows that the expected capabilities are present and are in the correct format.
        /// </summary>
        [TestMethod]
        public void GetDeviceCapabilities_Get_ReturnsExpectedFormatCapabilities()
        {
            string response = Capabilities.GetDeviceCapabilities();

            JsonSchema responseSchema =
                JsonSchema.FromText("{\"type\": \"object\",\"properties\": {\"device_capabilities\": {\"type\":\"array\"}}}");

            JsonNode parsedObject = JsonNode.Parse(response);
            Assert.IsTrue(responseSchema.Evaluate(parsedObject).IsValid);

            // Will throw exception and fail test if cannot deserialize into list of ints
            JsonSerializer.Deserialize<Dictionary<string, List<int>>>(response);
        }
    }
}
