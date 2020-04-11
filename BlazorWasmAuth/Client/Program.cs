using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using System.Threading;

namespace BlazorWasmAuth.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("app");

            //builder.Services.AddBaseAddressHttpClient();

            //builder.Services.AddSingleton<HttpClient>();
            //builder.Services.AddSingleton(new HttpClient());
            //var client=new HttpClientHandler
            builder.Services.AddSingleton(new HttpClient(new MyCustomClientHandler()));

            await builder.Build().RunAsync();
        }
    }

    public class MyCustomClientHandler : HttpClientHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("my", "token cool");
            return base.SendAsync(request, cancellationToken);
        }
    }

    //class CustomMessageHandler : DelegatingHandler
    //{
    //    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    //    {
    //        Console.WriteLine("Hnadle executed");
    //        return base.SendAsync(request, cancellationToken);
    //    }
    //}
}
