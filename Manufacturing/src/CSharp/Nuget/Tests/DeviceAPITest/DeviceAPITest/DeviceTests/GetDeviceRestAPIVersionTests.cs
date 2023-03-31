/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using Json.Schema;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Management.Automation;

namespace TestDeviceRestAPI.DeviceTests
{
    /// <summary>
    /// A test class for the get device rest api version api endpoint.
    /// </summary>
    [TestClass]
    public class GetDeviceRestAPIVersionTests
    {
        /// <summary>
        /// Tests if getting the device rest api version returns the expected version in the expected format.
        /// </summary>
        [TestMethod]
        public void GetDeviceRestAPIVersion_Get_ReturnsExpectedVersion()
        {
            JsonSchema responseSchema =
                JsonSchema.FromText("{\"type\": \"object\",\"properties\": {\"REST-API-Version\": {\"type\":\"string\"}}}");

            string response = Device.GetDeviceRestAPIVersion();
            JsonNode parsedObject = JsonNode.Parse(response);
            Assert.IsTrue(responseSchema.Evaluate(parsedObject).IsValid);
            SemanticVersion.Parse(parsedObject["REST-API-Version"].ToString());
        }
    }
}
