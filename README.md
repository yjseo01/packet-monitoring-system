# packet-monitoring-system
### migration 생성
```
dotnet ef migrations add InitialCreate
```

### migration 적용 및 DB 생성
```
dotnet ef database update
```

### To-Do
- function code 선택 드롭다운 리스트 구현하기 -------------------------------------------> ok (근데 각종 버튼이 작동하지 않음)
- publisher에서 commandline argument 받아서 처리하기
- publisher에서 nic에서 패킷 캡쳐하는 기능 만들기
- modbusTCP 서버/클라이언트 테스트 하는 테스트 코드 만들고 실행
- 위의 서버/클라이언트가 주고 받는 modbustcp 패킷 탐지하기
- 이상현상 탐지하는 방법 알아보기
- subscriberwebapp에 signal R 도입해서 이상현상 탐지
- signalR 이용해서 대시보드 만들기
- subscriberwebapp에 blazorize 이용해서 분석 차트 띄우기
- mqtt 패킷 암호화해서 보내기
- 로그인 구현하기
