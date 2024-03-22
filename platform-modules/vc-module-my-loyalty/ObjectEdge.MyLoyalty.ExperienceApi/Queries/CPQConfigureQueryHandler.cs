using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ObjectEdge.MyLoyalty.ExperienceApi.Aggregates;
using VirtoCommerce.ExperienceApiModule.Core.Infrastructure;
using ObjectEdge.MyLoyalty.ExperienceApi.Queries;
using System.Xml.Linq;

namespace ObjectEdge.MyLoyalty.ExperienceApi.Queries
{
    public class CPQConfigureQueryHandler : IQueryHandler<CPQConfigureQuery, CPQConfigureAggregate>
    {

        private readonly HttpClient _httpClient;

        public CPQConfigureQueryHandler(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public class CPQResponse
        {
            public Boolean loginSuccess { get; set; }
            public string? url{ get; set; }

        }

        /*public async Task<CPQConfigureAggregate> Handle(CPQConfigureQuery request, CancellationToken cancellationToken)
        {

            if (string.IsNullOrEmpty(request.ProductLine))
            {
                throw new ArgumentException("ProductFamily field is required.");
            }

            // 2. Construct the POST request
            var url = "http://localhost:3001/connect-cpq"; // Replace with the actual external server URL

            var content = new StringContent(
                $"{{ \"productLine\": \"{request.ProductLine}\" }}",
                Encoding.UTF8,
                "application/json"); // Ensure content type is set to JSON

            // 3. Send the POST request asynchronously
            using (var response = await _httpClient.PostAsync(url, content, cancellationToken))
            {
                // 4. Check for successful response status code
                if (response.IsSuccessStatusCode)
                {
                    // 5. Deserialize the response content into a BalanceResponse object (assuming JSON response)
                    var responseContent = await response.Content.ReadAsStringAsync(cancellationToken);
                    var cpqResponse = JsonConvert.DeserializeObject<CPQResponse>(responseContent);


                    // 7. Create and return the BalanceAggregate object
                    var result = new CPQConfigureAggregate
                    {
                        LoginSuccess = cpqResponse?.loginSuccess?? false,
                        Url = cpqResponse?.url
                    };
                    return result;
                }
                else
                {
                    // Handle unsuccessful response (log error, throw exception, etc.)
                    throw new HttpRequestException($"External server responded with status code: {response.StatusCode}");
                }
            }

        }*/

        public async Task<CPQConfigureAggregate> Handle(CPQConfigureQuery request, CancellationToken cancellationToken)
        {

            // Replace with your actual configuration
            try
            {
                var connection = new CpqConnector();

                // Login and handle potential errors
                var loginData = await connection.Login();

                // Extract success status and session ID
                XNamespace bm = "urn:soap.bigmachines.com";
                var loginSuccess = loginData?.Descendants(bm + "success")?.FirstOrDefault()?.Value;
                if (loginSuccess == null)
                {
                    throw new Exception("Login failed. Check the response for details.");
                }


                // Access elements using namespaces
                var sessionId = loginData?.Descendants(bm + "sessionId")?.FirstOrDefault()?.Value;

                if (string.IsNullOrEmpty(sessionId))
                {
                    throw new Exception("Session ID not found in login response.");
                }

                // Build CPQ punchout URL // Replace with actual request data
                var url = connection.BuildCpqPunchoutUrl(sessionId, request);

                // Construct JSON response
                var response = new CPQConfigureAggregate
                {
                    Url = url,
                    LoginSuccess = true
                };

                return response;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                throw;
            }
            //return Task.FromResult(JsonConvert.SerializeObject(response)); // Assuming JsonConvert is available 

        }



    }

    

}
