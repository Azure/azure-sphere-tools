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
    /// A test class for the get device OS version api endpoint.
    /// </summary>
    [TestClass]
    public class GetDeviceOSVersionTests
    {
        /// <summary>
        /// Tests if getting the device OS version returns the expected format.
        /// </summary>
        [TestMethod]
        public void GetDeviceOSVersion_Get_ReturnsExpectedVersion()
        {
            string restApiVersion = Device.GetDeviceRestAPIVersion();
            JsonNode parsedObject = JsonNode.Parse(restApiVersion);
            Console.WriteLine(restApiVersion);
            Console.WriteLine(parsedObject);
            Console.WriteLine(parsedObject["REST-API-Version"]);

            SemanticVersion restSemVer = SemanticVersion.Parse(parsedObject["REST-API-Version"].ToString());
            Console.WriteLine(restSemVer);
            SemanticVersion validVersion = SemanticVersion.Parse("4.5.0");
            Console.WriteLine(validVersion);
            Console.WriteLine(restSemVer.CompareTo(validVersion));

            if (restSemVer.CompareTo(validVersion) >= 0)
            {
                JsonSchema responseSchema =
                    JsonSchema.FromText("{\"type\": \"object\",\"properties\": {\"osversion\":{\"type\": \"string\"}}}");
                string osVersionResp = Device.GetDeviceOSVersion();

                JsonNode osVersionJson = JsonNode.Parse(osVersionResp);

                Assert.IsTrue(responseSchema.Evaluate(osVersionJson).IsValid);
                SemanticVersion.Parse(osVersionJson["osversion"].ToString());
            }
            else
            {
                Assert.ThrowsException<AzureSphereException>(() => Device.GetDeviceOSVersion());
            }
        }
    }
}
