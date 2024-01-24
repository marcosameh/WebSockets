using System;
using System.Net;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls("http://localhost:6969");
var app = builder.Build();

app.UseWebSockets();

app.Map("/ws", async context =>
{
    if (context.WebSockets.IsWebSocketRequest)
    {
        using var ws = await context.WebSockets.AcceptWebSocketAsync();

        while (ws.State == WebSocketState.Open)
        {
            var currentDate = DateTime.Now;
            var message = "The current time is : " + currentDate.ToString("HH:mm:ss");
            var bytes = Encoding.UTF8.GetBytes(message);
            var arraySegment = new ArraySegment<byte>(bytes, 0, bytes.Length);

            await ws.SendAsync(arraySegment, WebSocketMessageType.Text, true, CancellationToken.None);

            if (currentDate.Minute == 49)
            {
                // Properly close the WebSocket
                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing WebSocket", CancellationToken.None);
            }

            Thread.Sleep(1000);
        }
    }
    else
    {
        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
    }
});

await app.RunAsync();
