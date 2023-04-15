/* Copyright (c) Microsoft Corporation. All rights reserved.
   Licensed under the MIT License. */

using Microsoft.Azure.Sphere.DeviceAPI;
using Json.Schema;
using System.Text.Json.Nodes;

namespace TestDeviceRestAPI.NetworkTests
{
    /// <summary>
    /// A test class for the get network firewall ruleset endpoint.
    /// </summary>
    [TestClass]
    public class GetNetworkFirewallRulesetTests
    {
        /// <summary>Tests if getting the firewall ruleset returns rules in the correct format.</summary>
        [TestMethod]
        public void FirewallRuleset_Get_ReturnsList()
        {
            JsonSchema expectedTopLevelSchema =
                JsonSchema.FromText("{\"type\": \"object\",\"properties\": {\"rulesets\": {\"type\":\"array\"}}}");
            JsonSchema expectedRulesetSchema =
                JsonSchema.FromText("{\"type\": \"object\",\"properties\": {\"hook\": {\"type\":\"string\"}, \"isValid\":{\"type\": \"boolean\"}, \"rules\": {\"type\": \"array\"}}}");
            JsonSchema expectedRuleSchema =
                JsonSchema.FromText("{\"type\": \"object\",\"properties\": {\"sourceIP\":{\"type\":\"string\"},\"sourceMask\":{\"type\":\"string\"},\"destinationIP\":{\"type\":\"string\"},\"destinationMask\":{\"type\":\"string\"},\"uid\":{\"type\":\"integer\"},\"action\":{\"type\":\"string\"},\"interfaceInName\":{\"type\":\"string\"},\"interfaceOutName\":{\"type\":\"string\"},\"state\":{\"type\":\"string\"},\"tcpMask\":{\"type\":\"array\"},\"tcpCmp\":{\"type\":\"array\"},\"tcpInv\":{\"type\":\"boolean\"},\"protocol\":{\"type\":\"string\"},\"sourcePortRange\":{\"min\":{\"type\":\"integer\"},\"max\":{\"type\":\"integer\"}},\"destinationPortRange\":{\"min\":{\"type\":\"integer\"},\"max\":{\"type\":\"integer\"}},\"packets\":{\"type\":\"integer\"},\"bytes\":{\"type\":\"integer\"}}}");
            string response = Network.GetNetworkFirewallRuleset();

            JsonNode rulesets = JsonNode.Parse(response);
            Assert.IsTrue(expectedTopLevelSchema.Evaluate(rulesets).IsValid);

            foreach (JsonObject ruleset in (JsonArray)rulesets["rulesets"])
            {
                Assert.IsTrue(expectedRulesetSchema.Evaluate(ruleset).IsValid);
                foreach (JsonObject rule in (JsonArray)ruleset["rules"])
                {
                    Assert.IsTrue(expectedRuleSchema.Evaluate(rule).IsValid);
                }
            }
        }
    }
}
