using System;

using SharpPcap;
using PacketDotNet;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

using PacketCapture;
using Microsoft.Extensions.Configuration;

var builder = Host.CreateDefaultBuilder(args);

if (args.Length < 2)
{
    Console.WriteLine("[usage]: <mode> <file path>");
    Console.WriteLine("mode: ");
    Console.WriteLine("    1: packet capture");
    Console.WriteLine("    2: read .pcap file");
    return;
}

builder.ConfigureServices((hostContext, services) =>
{
    services.AddHostedService<MainHostedService>();
    services.AddSingleton<MqttPublisher>();

    // Configuration에 command line 인자 추가
    var config = new ConfigurationBuilder()
    .AddCommandLine(args)
    .Build();

    services.AddSingleton(config);
    services.AddSingleton(args);
});

await builder.RunConsoleAsync();
