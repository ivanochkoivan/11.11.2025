using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using NUnit.Framework;

namespace Task_3_Myself_code
{
    public class Class1
    {
        private HttpClient _client = null!;
        private string _baseUrl = null!;

        private class LoginFail
        {
            public string UserName { get; set; } = string.Empty;
            public int FailCount { get; set; }
        }

        [SetUp]
        public void Init()
        {
            _baseUrl = Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://site.com";
            _client = new HttpClient { BaseAddress = new Uri(_baseUrl), Timeout = TimeSpan.FromSeconds(10) };
        }

        [TearDown]
        public void Cleanup()
        {
            _client.Dispose();
        }

        [Test]
        public async Task Get_LoginFailTotal_ReturnsJsonArray()
        {
            var resp = await _client.GetAsync("/loginfailtotal");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var items = await resp.Content.ReadFromJsonAsync<List<LoginFail>>();
            Assert.That(items, Is.Not.Null);
        }

        [TestCase(1)]
        [TestCase(5)]
        public async Task Get_WithFetchLimit_DoesNotExceedLimit(int limit)
        {
            var resp = await _client.GetAsync($"/loginfailtotal?fetch_limit={limit}");
            Assert.That(resp.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var items = await resp.Content.ReadFromJsonAsync<List<LoginFail>>();
            Assert.That(items.Count, Is.LessThanOrEqualTo(limit));
        }

        [Test]
        public async Task Put_ResetLoginFailTotal_ResetsForGivenUser()
        {
            var user = $"auto.user.{Guid.NewGuid():N}@example.com";
            var payload = new { Username = user };
            var put = await _client.PutAsJsonAsync("/resetloginfailtotal", payload);
            Assert.That(put.StatusCode == HttpStatusCode.OK);
            var getUrl = $"/loginfailtotal?user_name={Uri.EscapeDataString(user)}";
            var get = await _client.GetAsync(getUrl);
            Assert.That(get.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var items = await get.Content.ReadFromJsonAsync<List<LoginFail>>();
            foreach (var it in items)
            {
                Assert.That(it.FailCount, Is.LessThanOrEqualTo(0));
            }
        }
    }
}
