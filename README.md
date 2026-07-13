# Launch Control Console

로켓 발사대의 상태를 통합 감시하고 발사 준비 상태와 카운트다운 절차를 확인하기 위한 **WPF 기반 발사관제 GUI 데모 프로젝트**입니다.

현재 버전은 발사대 전체 Overview, Launch Readiness Matrix, Countdown/Sequencer, 추진제 시스템 P&ID, 가압·퍼지 시스템 화면을 구현하고 있으며, 좌측 내비게이션을 통해 각 화면으로 이동할 수 있습니다.

> [!IMPORTANT]
> 본 프로젝트는 UI/UX와 소프트웨어 구조를 검토하기 위한 데모입니다. 실제 밸브, 펌프, 점화장치 또는 안전설비를 직접 제어하지 않습니다. 위험 명령 버튼은 비활성화되어 있으며, 실제 발사대 적용 시 Safety PLC, 하드와이어드 인터록, 권한·승인 절차 및 독립적인 비상정지 체계를 별도로 구성해야 합니다.

---

## 1. 프로젝트 개요

### 1.1 목적

이 프로젝트의 목적은 다음과 같습니다.

- 발사대 주요 서브시스템의 상태를 하나의 콘솔에서 감시
- 발사 준비 조건을 Launch Commit Criteria 형태로 확인
- 발사 카운트다운 및 시퀀스 진행 상태 표시
- 추진제와 가압·퍼지 계통을 P&ID 형태로 시각화
- 향후 PLC, EGSE, OPC UA, 필드버스 또는 메시지 브로커와 연동할 수 있는 WPF/MVVM 기반 구조 제공

### 1.2 기술 스택

| 항목 | 내용 |
|---|---|
| UI 프레임워크 | WPF |
| 대상 프레임워크 | .NET 8 (`net8.0-windows`) |
| 언어 | C# |
| UI 패턴 | MVVM 기반 |
| 플랫폼 | Windows |
| 데이터 소스 | 현재는 내부 시뮬레이션 텔레메트리 |
| 기본 갱신 주기 | 약 250 ms |

### 1.3 현재 구현된 화면

좌측 내비게이션 레일에서 다음 화면을 선택할 수 있습니다.

| 내비게이션 | 화면 | 주요 내용 |
|---|---|---|
| `OVR` | 통합 콘솔 | 발사대 전체 Overview, Readiness Matrix, Sequencer를 동시에 표시 |
| `LRM` | 통합 콘솔 | 현재 버전에서는 통합 콘솔로 이동 |
| `SEQ` | 통합 콘솔 | 현재 버전에서는 통합 콘솔로 이동 |
| `PROP` | 추진제 시스템 P&ID | LOX/RP-1 저장·이송 계통, 탱크, 밸브, 펌프, 유량·압력, 엄빌리컬 상태 |
| `P/P` | 가압·퍼지 시스템 | GN₂ 퍼지 계통, 헬륨 가압 계통, 레귤레이터, 밸브, 유량, 압력, 산소농도, 노점 |
| `PWR` | 미구현 | 전력 시스템 화면 예정 |
| `TRND` | 미구현 | 실시간 및 이력 트렌드 화면 예정 |
| `ALM` | 미구현 | 알람 화면 예정 |
| `ILK` | 미구현 | 인터록·Inhibit 화면 예정 |

---

## 2. 간략한 요구사항

### 2.1 공통 UI 요구사항

- 애플리케이션은 발사대 운용 상태를 다크 테마 기반의 통합 콘솔 형태로 표시해야 합니다.
- 상단에는 임무명, 운용 모드, UTC 시각, 현재 운영자를 표시해야 합니다.
- 좌측 내비게이션을 통해 각 서브시스템 화면으로 이동할 수 있어야 합니다.
- 현재 선택된 내비게이션 항목은 활성 상태로 구분되어야 합니다.
- 모든 주요 계측값은 단위와 함께 표시되어야 합니다.
- 통신·데이터 연동 전 단계에서는 시뮬레이션 값을 사용해야 합니다.
- 데이터 갱신이나 화면 전환으로 인해 UI 스레드가 장시간 차단되어서는 안 됩니다.

### 2.2 발사대 전체 Overview

- 주요 발사대 서브시스템의 상태를 타일 형태로 표시해야 합니다.
- 각 서브시스템은 정상, 주의, Hold, No-Go 등의 상태를 시각적으로 구분해야 합니다.
- 시스템 전체 운용 준비 상태를 빠르게 파악할 수 있어야 합니다.

### 2.3 Launch Readiness Matrix

- Launch Commit Criteria를 항목별로 표시해야 합니다.
- 각 항목에는 ID, 그룹, 판정 기준, 담당, 측정값, 제한값 및 상태가 포함되어야 합니다.
- GO, HOLD, NO-GO 항목 수를 집계해야 합니다.
- 하나 이상의 NO-GO가 있으면 전체 상태를 NO-GO로 판정해야 합니다.
- NO-GO가 없고 HOLD가 있으면 전체 상태를 HOLD로 판정해야 합니다.
- 모든 항목이 GO이면 전체 상태를 GO로 판정해야 합니다.

### 2.4 Countdown/Sequencer

- T-0 기준 카운트다운 시간을 표시해야 합니다.
- 시퀀스 단계별 T-Mark와 작업 내용을 표시해야 합니다.
- 시퀀스 상태는 Idle, Running, Holding, Aborted, Complete를 지원해야 합니다.
- 다음 조작을 지원해야 합니다.
  - Start
  - Hold
  - Resume
  - Recycle
  - Abort
- 지정된 단계에서 자동 Hold를 수행할 수 있어야 합니다.
- 승인 필요 단계는 별도 표시해야 합니다.
- 현재 버전의 시퀀스는 데모 로직이며 실제 발사 시퀀스 제어기로 사용해서는 안 됩니다.

### 2.5 추진제 시스템 P&ID

- LOX 및 RP-1 저장탱크를 표시해야 합니다.
- 각 탱크의 충전율과 저장 압력을 표시해야 합니다.
- 추진제 이송 배관, 차단 밸브, 펌프, 유량계를 표시해야 합니다.
- 발사체 엄빌리컬 연결 상태를 표시해야 합니다.
- LOX와 RP-1의 모의 유량 및 압력 데이터를 실시간으로 갱신해야 합니다.
- 운용 상태, 인터록 상태 및 Control Permit을 표시해야 합니다.
- 실제 위험 명령은 데모 버전에서 비활성화 상태로 유지해야 합니다.

### 2.6 가압·퍼지 시스템

- GN₂ 퍼지 공급 계통과 헬륨 가압 계통을 구분해 표시해야 합니다.
- 고압 가스 뱅크, 차단 밸브, 레귤레이터, 유량제어 요소를 표시해야 합니다.
- 발사체 가스 엄빌리컬 연결 상태를 표시해야 합니다.
- 다음 계측값을 표시해야 합니다.
  - GN₂ 저장 압력
  - 헬륨 저장 압력
  - 헬륨 조절 압력
  - 발사체 가압 압력
  - 퍼지 유량
  - 산소 농도
  - 노점
  - 배관 온도
- Control Permit 및 주요 인터록 상태를 표시해야 합니다.
- 실제 위험 명령은 데모 버전에서 비활성화 상태로 유지해야 합니다.

### 2.7 안전 및 운용 제한

- WPF 애플리케이션은 Safety PLC나 하드와이어드 인터록을 대체해서는 안 됩니다.
- GUI 또는 네트워크 장애가 위험 설비를 자동 동작시키지 않아야 합니다.
- 통신 복구 후 이전 제어 명령이 자동 재전송되어서는 안 됩니다.
- 실제 제어 기능을 추가할 경우 사용자 인증, 역할 기반 권한, 이중 승인 및 감사 로그가 필요합니다.
- 실제 설비 상태는 명령 수락과 최종 피드백을 구분하여 처리해야 합니다.

---

## 3. 프로젝트 구조

```text
LaunchControl/
├─ App.xaml
├─ App.xaml.cs
├─ LaunchControl.csproj
├─ README.md
├─ preview.png
│
├─ Converters/
│  └─ Converters.cs
│
├─ Models/
│  └─ Domain.cs
│
├─ Services/
│  └─ TelemetryService.cs
│
├─ Themes/
│  ├─ Palette.xaml
│  └─ Controls.xaml
│
├─ ViewModels/
│  ├─ Mvvm.cs
│  └─ ConsoleViewModel.cs
│
└─ Views/
   ├─ MainWindow.xaml
   ├─ MainWindow.xaml.cs
   ├─ NavIcon.xaml
   ├─ NavIcon.xaml.cs
   ├─ OverviewPanel.xaml
   ├─ ReadinessPanel.xaml
   ├─ SequencerPanel.xaml
   ├─ PropellantPanel.xaml
   └─ PressurizationPurgePanel.xaml
```

### 주요 파일 설명

| 파일 | 역할 |
|---|---|
| `LaunchControl.csproj` | .NET 8 WPF 프로젝트 설정 |
| `Views/MainWindow.xaml` | 상단 헤더, 좌측 내비게이션, 화면 컨테이너, 하단 상태바 |
| `Views/MainWindow.xaml.cs` | 내비게이션 선택 및 화면 전환 처리 |
| `ViewModels/ConsoleViewModel.cs` | 텔레메트리 표시값, Readiness 판정, 카운트다운 및 시퀀스 상태 관리 |
| `Services/TelemetryService.cs` | 데모용 시뮬레이션 데이터 생성 |
| `Models/Domain.cs` | 서브시스템, Readiness, 시퀀스 관련 모델과 열거형 |
| `Themes/Palette.xaml` | 색상 및 폰트 리소스 |
| `Themes/Controls.xaml` | 공통 WPF 컨트롤 스타일 |
| `Views/PropellantPanel.xaml` | 추진제 시스템 P&ID 화면 |
| `Views/PressurizationPurgePanel.xaml` | 가압·퍼지 시스템 화면 |

---

## 4. 빌드와 실행 방법

### 4.1 사전 요구사항

다음 환경이 필요합니다.

- Windows 10 또는 Windows 11
- .NET 8 SDK
- Visual Studio 2022 사용 시 `.NET 데스크톱 개발` 워크로드

설치된 SDK 확인:

```powershell
dotnet --list-sdks
```

출력에 `8.x.x` 버전이 포함되어 있어야 합니다.

### 4.2 Visual Studio에서 빌드 및 실행

1. 압축파일을 원하는 폴더에 해제합니다.
2. Visual Studio 2022를 실행합니다.
3. `프로젝트 또는 솔루션 열기`를 선택합니다.
4. `LaunchControl/LaunchControl.csproj` 파일을 엽니다.
5. 상단 구성에서 `Debug` 또는 `Release`를 선택합니다.
6. `빌드 > 솔루션 빌드`를 실행합니다.
7. `F5`를 눌러 디버깅 실행하거나 `Ctrl+F5`로 디버깅 없이 실행합니다.

### 4.3 .NET CLI로 빌드

PowerShell 또는 명령 프롬프트에서 프로젝트 폴더로 이동합니다.

```powershell
cd LaunchControl
```

의존성 복원:

```powershell
dotnet restore
```

Debug 빌드:

```powershell
dotnet build -c Debug
```

Release 빌드:

```powershell
dotnet build -c Release
```

### 4.4 .NET CLI로 실행

```powershell
dotnet run
```

Release 구성으로 실행:

```powershell
dotnet run -c Release
```

### 4.5 빌드 결과 직접 실행

Debug 빌드 결과:

```text
LaunchControl/bin/Debug/net8.0-windows/LaunchControl.exe
```

Release 빌드 결과:

```text
LaunchControl/bin/Release/net8.0-windows/LaunchControl.exe
```

### 4.6 배포용 게시

대상 PC에 .NET 8 Desktop Runtime이 설치되어 있는 경우:

```powershell
dotnet publish -c Release -r win-x64 --self-contained false
```

.NET Runtime이 설치되지 않은 PC에서도 실행 가능한 자체 포함 배포:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true
```

자체 포함 단일 파일 배포 예시:

```powershell
dotnet publish -c Release -r win-x64 --self-contained true `
  -p:PublishSingleFile=true
```

게시 결과는 기본적으로 다음 경로에 생성됩니다.

```text
LaunchControl/bin/Release/net8.0-windows/win-x64/publish/
```

---

## 5. 실행 후 조작 방법

### 화면 이동

- `OVR`, `LRM`, `SEQ`: 기존 통합 콘솔 화면으로 이동
- `PROP`: 추진제 시스템 P&ID 화면으로 이동
- `P/P`: 가압·퍼지 시스템 화면으로 이동

### Sequencer 조작

| 버튼 | 동작 |
|---|---|
| `SEQUENCE START` 또는 `START / RESUME` | 카운트다운 시퀀스 시작 또는 재개 |
| `HOLD` | 현재 시퀀스 일시 정지 |
| `RECYCLE` | 데모 카운트다운을 T-10:00으로 초기화 |
| `ABORT` | 현재 시퀀스 중단 |

실행 중 텔레메트리 데이터는 약 250 ms 주기로 변경됩니다. 화면에 표시되는 값은 실제 센서 데이터가 아니라 동작 확인용 시뮬레이션 값입니다.

---

## 6. 실제 설비 연동 시 변경 지점

현재의 모의 텔레메트리를 실제 데이터로 교체하려면 다음 영역을 확장합니다.

- `Services/TelemetryService.cs`
  - OPC UA Client
  - PLC 통신 드라이버
  - TCP/UDP 또는 Serial 통신
  - MQTT/AMQP 메시지 구독
  - EGSE 또는 발사대 제어 게이트웨이 연동

권장 구조:

```text
WPF View
   ↓ Binding
ViewModel
   ↓
Application/Command Service
   ↓
Control & Telemetry Gateway
   ↓
PLC / Safety PLC / Field Equipment
```

WPF 화면에서 PLC나 장비 프로토콜을 직접 호출하기보다, 통신 게이트웨이와 명령 서비스를 별도 계층 또는 별도 프로세스로 분리하는 방식을 권장합니다.

---

## 7. 향후 개발 항목

- 전력 시스템 화면
- 발사대 기계설비 및 엄빌리컬 화면
- 방재·가스감지 화면
- 실시간 및 이력 트렌드
- 알람 목록과 First-Out Alarm
- 인터록·Inhibit Matrix
- 사용자 로그인과 역할 기반 접근제어
- 위험 명령의 Arm/Confirm/이중 승인
- 전자 체크리스트 및 GO/NO-GO Poll
- 명령·이벤트·감사 로그 저장
- 실제 PLC 및 텔레메트리 연동
- 통신 두절, Stale 데이터, 센서 품질 처리
- 시뮬레이터와 Hardware-in-the-Loop 시험환경

---

## 8. 참고 및 주의사항

- 이 프로젝트는 실제 발사 안전 인증을 받은 소프트웨어가 아닙니다.
- 카운트다운 로직은 UI 검증용 데모이며 실시간 제어를 보장하지 않습니다.
- 비상정지, 추진제 긴급 차단, 점화 최종 허가 등은 GUI와 독립적인 안전 계층에서 처리해야 합니다.
- 실제 운용 전에는 HAZOP, FMEA, 소프트웨어 요구사항 추적, 독립 검증 및 현장 통합시험이 필요합니다.

## 추가 구현 화면

### 전력 시스템 (`PWR`)

- 상용 전원, 주 배전반, UPS, 정류기/충전기, 28 VDC 제어 버스 및 비행 배터리 인터페이스를 단선도 형태로 표시합니다.
- 상용 전압·주파수, UPS 부하·예상 운전시간, DC 버스 전압, 비행 배터리 전압 및 발전기 시험 부하를 약 250 ms 주기로 갱신합니다.
- 급전 상태, 전력 품질, 전기 인터록 및 활성 알람 요약을 제공합니다.
- 차단기, 발전기, UPS 및 전원 차단 명령은 데모 빌드에서 비활성화되어 있습니다.

### 발사대 기계설비 (`MECH`)

- Transporter/Erector, 서비스 암, 엄빌리컬 암, 유압 동력장치, 방수(Deluge) 탱크·헤더 및 화염 편향기를 표시합니다.
- 기립대 각도, 유압 압력, 서비스 암 위치, 방수 탱크 수위·압력 및 화염 편향기 온도를 약 250 ms 주기로 갱신합니다.
- 기립대 잠금, 암 잠금핀, 엄빌리컬 접속, Hold-down clamp 및 릴리즈 Inhibit 상태를 표시합니다.
- 암 후퇴, 기립대 하강, HPU 기동, 방수 시험 및 릴리즈 명령은 데모 빌드에서 비활성화되어 있습니다.
