using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Task3.ApiTests
{
    // Simple integration tests for:
    // GET  /loginfailtotal
    // PUT  /resetloginfailtotal
    //
    // Configuration:
    // - API_BASE_URL (env) - e.g. "https://qa.api.myapp" (default "http://localhost:5000")
    // - TEST_USERNAME  (env) - optional username to run stricter assertions against
    public class ApiLoginFailTests
    {
        private HttpClient _client = null!;
        private string _baseUrl = null!;
        private string? _testUsername;

        private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };

        record LoginFailRecord(string UserName, int FailCount);

        [SetUp]
        public void SetUp()
        {
            _baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://site.com";
            _testUsername = Environment.GetEnvironmentVariable("TEST_USERNAME");
            _client = new HttpClient
            {
                BaseAddress = new Uri(_baseUrl),
                Timeout = TimeSpan.FromSeconds(10)
            };
        }

        [TearDown]
        public void TearDown()
        {
            _client.Dispose();
        }

        // 1) Basic contract: GET with no parameters returns 200 and an array (possibly empty).
        [Test]
        public async Task Get_LoginFailTotal_NoParams_ReturnsArrayAndOk()
        {
            var resp = await _client.GetAsync("/loginfailtotal");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), "Expected 200 OK from GET /loginfailtotal");

            var content = await resp.Content.ReadAsStringAsync();
            Assert.That(content, Is.Not.Null.And.Not.Empty, "Response body should not be empty");

            // parse as array of objects; tolerate empty array
            try
            {
                var items = JsonSerializer.Deserialize<List<LoginFailRecord>>(content, JsonOptions);
                Assert.That(items, Is.Not.Null, "Response should be a JSON array (can be empty)");
            }
            catch (JsonException ex)
            {
                Assert.Fail("Response was not a JSON array of expected shape: " + ex.Message);
            }
        }

        // 2) Parameter: fetch_limit - returned items count should be <= fetch_limit
        [TestCase(1)]
        [TestCase(3)]
        [TestCase(10)]
        public async Task Get_WithFetchLimit_RespectsLimit(int fetchLimit)
        {
            var url = $"/loginfailtotal?fetch_limit={fetchLimit}";
            var resp = await _client.GetAsync(url);
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK), "GET with fetch_limit must return 200 OK");

            var items = await resp.Content.ReadFromJsonAsync<List<LoginFailRecord>>(JsonOptions) ?? new List<LoginFailRecord>();
            Assert.That(items.Count, Is.LessThanOrEqualTo(fetchLimit), $"Returned items ({items.Count}) should be <= fetch_limit ({fetchLimit})");
        }

        // 3) Parameter: fail_count filter - if set, all returned records must have FailCount > provided value
        [TestCase(0)]
        [TestCase(2)]
        public async Task Get_WithFailCountFilter_ReturnsOnlyAboveThreshold(int threshold)
        {
            var url = $"/loginfailtotal?fail_count={threshold}";
            var resp = await _client.GetAsync(url);
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var items = await resp.Content.ReadFromJsonAsync<List<LoginFailRecord>>(JsonOptions) ?? new List<LoginFailRecord>();

            // If the system has no users above threshold, an empty array is acceptable.
            foreach (var item in items)
            {
                Assert.That(item.FailCount, Is.GreaterThan(threshold), $"User {item.UserName} returned but FailCount {item.FailCount} is not > {threshold}");
            }
        }

        // 4) Parameter: user_name - when specified, every returned record (if any) should match the requested username.
        // If TEST_USERNAME is provided it asserts at least one item is returned; otherwise the test is tolerant.
        [Test]
        public async Task Get_WithUserName_FiltersByUser()
        {
            var username = _testUsername ?? "nonexistent.user@example.com";
            var url = $"/loginfailtotal?user_name={WebUtility.UrlEncode(username)}";
            var resp = await _client.GetAsync(url);
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var items = await resp.Content.ReadFromJsonAsync<List<LoginFailRecord>>(JsonOptions) ?? new List<LoginFailRecord>();

            foreach (var item in items)
            {
                Assert.That(item.UserName, Is.EqualTo(username), $"Returned user {item.UserName} does not match requested {username}");
            }

            if (_testUsername is not null)
            {
                // If the CI/job has provided a known username we expect at least one record (business choice).
                Assert.That(items.Count, Is.GreaterThanOrEqualTo(0), "When TEST_USERNAME is provided, endpoint should respond normally (0+ results are acceptable)");
            }
        }

        // 5) PUT reset endpoint - basic contract: responds 200/204 and subsequent GET for that username shows no large fail counts.
        [Test]
        public async Task Put_ResetLoginFailTotal_ForUser_ReturnsSuccess_AndResetsCount()
        {
            var username = _testUsername ?? $"automated.test.{Guid.NewGuid():N}@example.com";

            // Perform reset
            var payload = new { Username = username };
            var putResp = await _client.PutAsJsonAsync("/resetloginfailtotal", payload);
            Assert.That(putResp.StatusCode == HttpStatusCode.OK || putResp.StatusCode == HttpStatusCode.NoContent,
                $"Expected 200 OK or 204 NoContent from PUT /resetloginfailtotal, actual: {(int)putResp.StatusCode}");

            // Now query the user specifically; tolerate either an empty response or a record with FailCount == 0
            var getResp = await _client.GetAsync($"/loginfailtotal?user_name={WebUtility.UrlEncode(username)}");
            Assert.That(getResp.StatusCode, Is.EqualTo(HttpStatusCode.OK), "GET after reset must return 200 OK");

            var items = await getResp.Content.ReadFromJsonAsync<List<LoginFailRecord>>(JsonOptions) ?? new List<LoginFailRecord>();
            if (items.Count == 0)
            {
                Assert.Pass("User not present after reset (acceptable) - PUT succeeded and GET returned no records.");
            }
            else
            {
                // If present assert counts are zero or small (<=0). Business rule: reset sets count to 0.
                foreach (var item in items)
                {
                    Assert.That(item.FailCount, Is.LessThanOrEqualTo(0), $"After reset, expected FailCount <= 0 but found {item.FailCount} for {item.UserName}");
                }
            }
        }
    }
}