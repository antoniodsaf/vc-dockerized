using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.Configuration;

namespace ObjectEdge.MyLoyalty.ExperienceApi.Queries
{
    public class CpqConnector
    {

        private readonly Dictionary<string, string> _config;

        public CpqConnector()
        {
            // Initialize _config with dummy values for illustrative purposes
            _config = new Dictionary<string, string>()
            {
                { "baseUrl", "https://objectedgeinc.bigmachines.com" }, 
                { "serviceIdentifier", "objectedgeinc" },
                { "username", "poc" },
                { "password", "Objectedge$24" }
            };
        }

        private string _createLoginTemplate()
        {
            return $@"<?xml version=""1.0"" encoding=""UTF-8""?>
                    <soapenv:Envelope xmlns:soapenv=""http://schemas.xmlsoap.org/soap/envelope/"">
                        <soapenv:Header>
                            <bm:category xmlns:bm=""urn:soap.bigmachines.com"">Security</bm:category>
                            <bm:xsdInfo xmlns:bm=""urn:soap.bigmachines.com"">
                                <bm:schemaLocation>{_config["baseUrl"]}/bmfsweb/{_config["serviceIdentifier"]}/schema/v1_0/security/Security.xsd</bm:schemaLocation>
                            </bm:xsdInfo>
                        </soapenv:Header>
                        <soapenv:Body>
                            <bm:login xmlns:bm=""urn:soap.bigmachines.com"">
                                <bm:userInfo>
                                    <bm:username>{_config["username"]}</bm:username>
                                    <bm:password>{_config["password"]}</bm:password>
                                </bm:userInfo>
                            </bm:login>
                        </soapenv:Body>
                    </soapenv:Envelope>";
        }

        public async Task<XElement> Login()
        {
            try
            {
                using var client = new HttpClient();

                var loginTemplate = _createLoginTemplate();
                var content = new StringContent(loginTemplate, Encoding.UTF8, "text/xml");

                var response = await client.PostAsync($"{_config["baseUrl"]}/v1_0/receiver", content);
                response.EnsureSuccessStatusCode();

                var xmlString = await response.Content.ReadAsStringAsync();
                var doc = XDocument.Parse(xmlString);
                XNamespace soapenv = "http://schemas.xmlsoap.org/soap/envelope/";
                XNamespace bm = "urn:soap.bigmachines.com";

                var namespaceSoap = doc?.Root?.Name.LocalName == "Envelope" ? "soapenv" : "soap";


                var soapBody = doc?.Root?.Element(soapenv + "Body");
                var soapFault = doc?.Root?.Element(soapenv + "Fault");

                if (soapFault != null)
                {
                    throw new Exception(soapFault.Element(XName.Get(namespaceSoap, "faultstring"))?.Value);
                }

                if (soapBody == null)
                {
                    // Handle parsing error (log, throw exception)
                    throw new Exception("Error parsing login response XML. ");
                }

                return soapBody;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw;
            }
        }

        public string BuildCpqPunchoutUrl(string sessionId, CPQConfigureQuery request)
        {// Check for null before accessing properties

            // Set actual values (replace with your logic)
            try
            {
                var baseUrl = _config["baseUrl"];
                var responseUrl = $"{baseUrl}/commerce/new_equipment/products/model_configs.jsp";

                var queryParams = new Dictionary<string, string>()
                {
                    { "username", _config["username"] ?? "" },
                    { "sso", "true" },
                    { "sessionId", sessionId },
                    { "segment", request?.Segment ?? ""},
                    { "product_line", request?.ProductLine ?? "" },
                    { "model", request?.Model ?? "" },
                    { "_bm_session_locale", "en" },
                    { "_bm_session_currency", "USD" },
                    { "_from_partner", "true" }
                };

                var url = $"{responseUrl}?{string.Join("&", queryParams.Select(kvp => $"{kvp.Key}={Uri.EscapeDataString(kvp.Value)}"))}";
                return url;
            }
            catch
            {
                throw new Exception("Error  BuildCpqPunchoutUrl");
            }
                // ... rest of the code for building the URL ...
              
            

        }

        public class RequestData
        {
            public required string Segment { get; set; }
            public required string ProductLine { get; set; }
            public required string Model { get; set; }
        }
    }

}
