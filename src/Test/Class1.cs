using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using lightinject_issue;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Xunit;

namespace Test
{
    public class Class1
    {
        private HttpClient client;

        [Fact]
        public async void Test1()
        {
            TestServer server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            client = server.CreateClient();

            Task<HttpResponseMessage>[] tasks = new[]
            {
                client.GetAsync("/1/scope_error"),
                client.GetAsync("/1/scope_error"),
                client.GetAsync("/1/scope_error"),
                client.GetAsync("/1/scope_error")
            };

            Task.WaitAll(tasks);
            Assert.True(tasks[0].Result.IsSuccessStatusCode);
            Assert.True(tasks[1].Result.IsSuccessStatusCode);
            Assert.True(tasks[2].Result.IsSuccessStatusCode);
            Assert.True(tasks[3].Result.IsSuccessStatusCode);
        }

        [Fact]
        public async void Test2()
        {
            TestServer server = new TestServer(new WebHostBuilder().UseStartup<Startup>());
            client = server.CreateClient();

            Task<HttpResponseMessage>[] tasks = new[]
            {
                client.GetAsync("/1/scope_error"),
                client.GetAsync("/1/scope_error"),
                client.GetAsync("/1/scope_error"),
                client.GetAsync("/1/scope_error")
            };

            Task.WaitAll(tasks);
            Assert.True(tasks[0].Result.IsSuccessStatusCode);
            Assert.True(tasks[1].Result.IsSuccessStatusCode);
            Assert.True(tasks[2].Result.IsSuccessStatusCode);
            Assert.True(tasks[3].Result.IsSuccessStatusCode);
        }
    }
}
