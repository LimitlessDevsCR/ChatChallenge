using Chat.Bot;
using Chat.Infrastructure;

var builder = Host.CreateApplicationBuilder(args);
builder.Services.AddRabbitMqMessaging(builder.Configuration);
builder.Services.AddHostedService<Worker>();

var host = builder.Build();
host.Run();
