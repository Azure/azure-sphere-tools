/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using Json.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace TestDeviceRestAPI.ManufacturingTests
{
    /// <summary>
    /// A test class for the get manufacturing state api endpoint.
    /// </summary>
    [TestClass]
    public class GetManufacturingStateTests
    {
        /// <summary>
        /// Tests if get manufacturing returns a response of the correct format.
        /// </summary>
        [TestMethod]
        public void GetManufacturingState_Get_ReturnsValidManufacturingState()
        {
            string response = Manufacturing.GetManufacturingState();

            JsonSchema expectedTopLevelSchema =
                JsonSchema.FromText("{\"type\": \"object\",\"properties\": {\"manufacturingState\": {\"type\":\"string\"}}}");

            string[] validManufacturingStates = { "Blank", "Module1Complete", "DeviceComplete", "Unknown" };

            JsonNode parsedObject = JsonNode.Parse(response);
            Assert.IsTrue(expectedTopLevelSchema.Evaluate(parsedObject).IsValid);

            string state = JsonSerializer.Deserialize<Dictionary<string, string>>(response)["manufacturingState"];

            Assert.IsTrue(validManufacturingStates.Any(validState => validState.Equals(state)));
        }
    }
}
