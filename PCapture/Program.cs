using System;

using SharpPcap;
using PacketDotNet;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PCapture;

// 장치 목록 가져오기
var devices = CaptureDeviceList.Instance;
if (devices.Count < 1)
{
    Console.WriteLine("NIC를 찾을 수 없습니다.");
    return;
}

// 장치 목록 출력하기
Console.WriteLine("네트워크 장치 목록: ");
for (int i = 0; i < devices.Count; i++)
{
    Console.WriteLine($"div{i} {devices[i].Name}: {devices[i].Description}");
}

// 사용자가 장치 선택하게 하기
int devIdx = -1;
Console.WriteLine($"장치를 선택하세요(0 ~ {devices.Count - 1}): ");
string input = Console.ReadLine();

if (int.TryParse(input, out devIdx) && (devIdx < 0 || devIdx >= devices.Count))
{
    Console.WriteLine("잘못된 입력입니다.");
    return;
}

var device = devices[devIdx]; // 선택한 장치
Console.WriteLine($"선택한 장치: {device.Name}|{device.Description}");

var builder = Host.CreateDefaultBuilder(args); // 호스트 빌더 생성
builder.ConfigureServices((hostConext, services) =>
{
    // MainHostedService 클래스를 호스트의 서비스로 등록
    //services.AddHostedService(sp => new MainHostedService(devIdx));
    services.AddSingleton<MqttPublisher>();
    services.AddSingleton<MainHostedService>(sp => new MainHostedService(devIdx, sp.GetRequiredService<MqttPublisher>()));
});

await builder.RunConsoleAsync();