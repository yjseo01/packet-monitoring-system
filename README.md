# packet-monitoring-system# ModbusTCP Packet Capture and Analysis

This project captures ModbusTCP packets from a specified pcap file, filters and subscribes to ModbusTCP protocol messages, and aggregates the data by function codes. The aggregated results are stored in a database and displayed on a Blazor web application for easy visualization.

## Features

- **Packet Capture**: Reads and analyzes ModbusTCP packets from pcap files.
- **Data Subscription**: Subscribes to ModbusTCP messages based on function codes.
- **Data Aggregation**: Aggregates received data by function code for statistical analysis.
- **Web Visualization**: Displays aggregated results on a Blazor web page.

## Technologies Used

- C#
- MQTTnet
- PacketDotNet
- Blazor
- SQLite

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/yourusername/yourrepository.git

---

### To-Do
- function code 선택 드롭다운 리스트 구현하기 -------------------------------------------> ok (근데 각종 버튼들이 작동하지 않음)
- publisher에서 commandline argument 받아서 처리하기 ------------------------------------> ok
- publisher에서 nic에서 패킷 캡쳐하는 기능 만들기 ---------------------------------------> ok (cancellation token 구현 필요)
- publisher에서 pcap 파일 만들기
  
- modbusTCP 서버/클라이언트 테스트 하는 테스트 코드 만들고 실행
- 위의 서버/클라이언트가 주고 받는 modbustcp 패킷 탐지하기
  
- 이상현상 탐지하는 방법 알아보기
- subscriberwebapp에 signal R 도입해서 이상현상 탐지
- signalR 이용해서 대시보드 만들기
- subscriberwebapp에 blazorize 이용해서 분석 차트 띄우기
- mqtt 패킷 암호화해서 보내기
- 로그인 구현하기
- modbustcp 말고도 다양한 프로토콜 추가하기

