//using Microsoft.AspNetCore.Authentication.OpenIdConnect;
//using Microsoft.Identity.Web;

//var builder = WebApplication.CreateBuilder(args);

//// Add services to the container.
//builder.Services.AddControllersWithViews();

//builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme).AddMicrosoftIdentityWebApp(
//builder.Configuration.GetSection("AzureAd"));

////
//builder.Services.Configure<
//    Microsoft.AspNetCore.Authentication.OpenIdConnect.OpenIdConnectOptions>(
//    Microsoft.AspNetCore.Authentication.OpenIdConnect
//        .OpenIdConnectDefaults.AuthenticationScheme,
//    options =>
//    {
//        options.BackchannelHttpHandler = new SocketsHttpHandler
//        {
//            ConnectCallback = async (context, cancellationToken) =>
//            {
//                var addresses = await System.Net.Dns.GetHostAddressesAsync(
//                    context.DnsEndPoint.Host, cancellationToken);

//                var ipv4 = addresses.FirstOrDefault(
//                    a => a.AddressFamily ==
//                         System.Net.Sockets.AddressFamily.InterNetwork);

//                if (ipv4 is null)
//                    throw new Exception("IPv4 address not found.");

//                var socket = new System.Net.Sockets.Socket(
//                    System.Net.Sockets.AddressFamily.InterNetwork,
//                    System.Net.Sockets.SocketType.Stream,
//                    System.Net.Sockets.ProtocolType.Tcp);

//                try
//                {
//                    await socket.ConnectAsync(
//                        new System.Net.IPEndPoint(
//                            ipv4, context.DnsEndPoint.Port),
//                        cancellationToken);

//                    return new System.Net.Sockets.NetworkStream(
//                        socket, ownsSocket: true);
//                }
//                catch
//                {
//                    socket.Dispose();
//                    throw;
//                }
//            }
//        };
//    });
////
//var app = builder.Build();

//// Configure the HTTP request pipeline.
//if (!app.Environment.IsDevelopment())
//{
//    app.UseExceptionHandler("/Home/Error");
//    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
//    app.UseHsts();
//}

//app.UseHttpsRedirection();
//app.UseStaticFiles();

//app.UseRouting();
//app.UseAuthentication();

//app.UseAuthorization();


//app.MapControllerRoute(
//    name: "default",
//    pattern: "{controller=Home}/{action=Index}/{id?}");

//app.Run();



using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Identity.Web;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();

builder.Services
    .AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(
        builder.Configuration.GetSection("AzureAd"));

// IPv4 handler‌ను మిగతా options configuration తర్వాత అమర్చండి.
//builder.Services.PostConfigure<OpenIdConnectOptions>(options =>
//{
//    options.BackchannelHttpHandler = new SocketsHttpHandler
//    {
//        ConnectCallback = async (context, cancellationToken) =>
//        {
//            var addresses = await Dns.GetHostAddressesAsync(
//                context.DnsEndPoint.Host, cancellationToken);

//            var ipv4 = addresses.FirstOrDefault(
//                address => address.AddressFamily == AddressFamily.InterNetwork);

//            if (ipv4 is null)
//                throw new HttpRequestException("IPv4 address not found.");

//            var socket = new Socket(
//                AddressFamily.InterNetwork,
//                SocketType.Stream,
//                ProtocolType.Tcp);

//            try
//            {
//                await socket.ConnectAsync(
//                    new IPEndPoint(ipv4, context.DnsEndPoint.Port),
//                    cancellationToken);

//                return new NetworkStream(socket, ownsSocket: true);
//            }
//            catch
//            {
//                socket.Dispose();
//                throw;
//            }
//        }
//    };
//});


builder.Services.PostConfigure<OpenIdConnectOptions>(options =>
{
    var handler = new SocketsHttpHandler
    {
        ConnectCallback = async (context, cancellationToken) =>
        {
            var addresses = await Dns.GetHostAddressesAsync(
                context.DnsEndPoint.Host, cancellationToken);

            var ipv4 = addresses.FirstOrDefault(
                address => address.AddressFamily == AddressFamily.InterNetwork);

            if (ipv4 is null)
                throw new HttpRequestException("IPv4 address not found.");

            var socket = new Socket(
                AddressFamily.InterNetwork,
                SocketType.Stream,
                ProtocolType.Tcp);

            try
            {
                await socket.ConnectAsync(
                    new IPEndPoint(ipv4, context.DnsEndPoint.Port),
                    cancellationToken);

                return new NetworkStream(socket, ownsSocket: true);
            }
            catch
            {
                socket.Dispose();
                throw;
            }
        }
    };

    options.Backchannel = new HttpClient(handler)
    {
        Timeout = TimeSpan.FromSeconds(60)
    };
});
var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();