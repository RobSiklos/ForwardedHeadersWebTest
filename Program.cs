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
            options.KnownNetworks.Add(new IPNetwork(System.Net.IPAddress.Parse("0.0.0.0"), 0));
            options.KnownNetworks.Add(new IPNetwork(System.Net.IPAddress.Parse("::"), 0));
        });

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        app.Use((context, next) =>
        {
            Console.WriteLine(context.Request.Scheme); // http
            Console.WriteLine(context.Request.Headers["X-Forwarded-Proto"]); // https
            Console.WriteLine("----");
            return next();
        });

        app.UseForwardedHeaders();

        app.Use((context, next) =>
        {
            Console.WriteLine(context.Request.Scheme); // https
            Console.WriteLine(context.Request.Headers["X-Forwarded-Proto"]); // empty
            Console.WriteLine(context.Request.Headers["X-Original-Proto"]); // http *** SHOULD BE https ***
            Console.WriteLine("====================");
            return next();
        });

        app.MapControllers();

        app.Run();
    }
}
}
