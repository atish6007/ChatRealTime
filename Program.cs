using MyChatApp;
using Microsoft.OpenApi.Models;
using MyChatApp.Service;

namespace MyChatApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddControllers();
            builder.Services.AddSingleton<ChatHistoryStore>();
            builder.Services.AddSingleton<WebSocketHandler>();

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo { Title = "MyChatAPI", Version = "v1" });
            });

            var app = builder.Build();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/swagger/v1/swagger.json", "MyChatAPI v1");
                    options.RoutePrefix = "swagger"; 
                });
            }

            app.UseWebSockets();

            app.Use(async (context, next) =>
            {
                if (context.Request.Path == "/chat")
                {
                    if (context.WebSockets.IsWebSocketRequest)
                    {
                        var userId = context.Request.Query["user_id"];
                        if (string.IsNullOrEmpty(userId))
                        {
                            context.Response.StatusCode = 400;
                            await context.Response.WriteAsync("Missing or invalid user_id query parameter");
                            return;
                        }

                        var socket = await context.WebSockets.AcceptWebSocketAsync();
                        var webSocketHandler = context.RequestServices.GetRequiredService<WebSocketHandler>();

                        try
                        {
                            await webSocketHandler.HandleConnection(socket, userId);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error handling WebSocket connection: {ex.Message}");
                            context.Response.StatusCode = 500; 
                        }
                    }
                    else
                    {
                        context.Response.StatusCode = 400;
                        await context.Response.WriteAsync("Non-WebSocket request made to /chat endpoint");
                    }
                }
                else
                {
                    await next();
                }
            });

            app.MapControllers();

            app.Run();
        }
    }
}
