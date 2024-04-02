using Microsoft.AspNetCore.HttpOverrides;
//using System.Net;

namespace ForwardedHeadersWebTest
{
public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers();

        builder.Services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.All;
            options.ForwardLimit = null;

            // Trust all known networks, for testing.
            options.KnownProxies.Clear();
            options.KnownNetworks.Clear();
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.Use((context, next) =>
        {
            Console.WriteLine($"Original scheme: {context.Request.Scheme}"); // http
            Console.WriteLine($"Original IP: [{context.Connection?.RemoteIpAddress}]:{context.Connection?.RemotePort}");
            Console.WriteLine($"Original X-Forwarded-For: {context.Request.Headers["X-Forwarded-For"]}"); // https
            Console.WriteLine($"Original X-Forwarded-Proto: {context.Request.Headers["X-Forwarded-Proto"]}"); // https
            Console.WriteLine("----");
            return next();
        });

        app.UseForwardedHeaders();

        app.Use((context, next) =>
        {
            Console.WriteLine($"New scheme: {context.Request.Scheme}"); // https
            Console.WriteLine($"New IP: [{context.Connection?.RemoteIpAddress}]:{context.Connection?.RemotePort}");
            Console.WriteLine($"New X-Forwarded-For: {context.Request.Headers["X-Forwarded-For"]}");
            Console.WriteLine($"New X-Forwarded-Proto: {context.Request.Headers["X-Forwarded-Proto"]}"); // empty
            Console.WriteLine($"New X-Original-For: {context.Request.Headers["X-Original-For"]}");

            // *** HERE IS THE PROBLEM ***
            Console.WriteLine($"New X-Original-Proto: {context.Request.Headers["X-Original-Proto"]}"); // http *** SHOULD BE https ***
            
            Console.WriteLine("====================");
            return next();
        });

        app.MapControllers();

        app.Run();
    }
}
}
