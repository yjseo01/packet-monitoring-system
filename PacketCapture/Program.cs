using System;

using SharpPcap;
using PacketDotNet;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using PacketCapture;

var builder = Host.CreateDefaultBuilder(args);
builder.ConfigureServices((hostContext, services) =>
{
    services.AddHostedService<MainHostedService>();
    services.AddSingleton<MqttPublisher>();

});

await builder.RunConsoleAsync();
