using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace APItests
{
    [TestClass]
    public class EnvironmentSystemTests
    {
        private HttpClient _client;
        private string _baseUrl = "https://tijlswebsite-dygxf0d5dcehd9fg.northeurope-01.azurewebsites.net";
        private string _token;

        [TestInitialize]
        public async Task Setup()
        {
            _client = new HttpClient();
            _token = await LoginAndGetToken();
            _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
        }

        private async Task<string> LoginAndGetToken()
        {
            var loginUrl = $"{_baseUrl}/accounts/login";
            var loginPayload = new
            {
                email = "SystemTestsUser123@mail.com",
                password = "SystemTestsUser123!",
                twoFactorCode = "string",
                twoFactorRecoveryCode = "string"
            };
            var content = new StringContent(JsonConvert.SerializeObject(loginPayload), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(loginUrl, content);
            response.EnsureSuccessStatusCode();

            var jsonString = await response.Content.ReadAsStringAsync();
            dynamic json = JsonConvert.DeserializeObject(jsonString);

            // Assuming the token is returned as json.token or json.accessToken
            return json.token ?? json.accessToken;
        }

        [TestMethod]
        public async Task CreateEnvironment_ReturnsCreated()
        {
            var uniqueName = "SystemTestEnv_" + Guid.NewGuid().ToString("N").Substring(0, 8);
            var newEnv = new
            {
                Name = uniqueName,
                MaxHeight = 5,
                MaxLength = 5
            };
            var url = $"{_baseUrl}/api/environments";
            var content = new StringContent(JsonConvert.SerializeObject(newEnv), Encoding.UTF8, "application/json");

            var response = await _client.PostAsync(url, content);

            Assert.IsTrue(response.IsSuccessStatusCode, $"Status Code: {response.StatusCode}");
            Assert.AreEqual(System.Net.HttpStatusCode.Created, response.StatusCode);

            var responseData = await response.Content.ReadAsStringAsync();
            Assert.IsFalse(string.IsNullOrEmpty(responseData));
        }

        [TestMethod]
        public async Task GetEnvironments_ReturnsOk()
        {
            var url = $"{_baseUrl}/api/environments";

            var response = await _client.GetAsync(url);

            Assert.IsTrue(response.IsSuccessStatusCode, $"Status Code: {response.StatusCode}");
            Assert.AreEqual(System.Net.HttpStatusCode.OK, response.StatusCode);

            var responseData = await response.Content.ReadAsStringAsync();
            Assert.IsFalse(string.IsNullOrEmpty(responseData));
            // Optionally deserialize and assert specific data
        }
    }
}
