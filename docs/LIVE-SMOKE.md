# FAIRINO 라이브 스모크 가이드

## 목적

`robottemplete`에서 로봇을 움직이지 않고,  
FAIRINO FR5와의 연결 신호만 안전하게 확인하기 위한 문서입니다.

## 이 테스트가 하는 일

- `Connect`
- `GetVersion`
- `ReadState`
- `Disconnect`

## 이 테스트가 하지 않는 일

- `Enable`
- `MoveJ`
- `MoveL`
- `ServoJ`
- 실제 모션 명령

즉, 이 문서는 "실기기 제어"가 아니라 "연결 여부 확인"에 초점을 둡니다.

## 실행 전 준비

아래 DLL이 `Assets/Plugins/Fairino/`에 있어야 합니다.

- `libfairino.dll`
- `CookComputing.XmlRpcV2.dll`

현재 저장소에는 포함되어 있지만, 누락되면 smoke가 정상 동작하지 않습니다.

## Unity 메뉴에서 실행하는 방법

가장 쉬운 방법은 데모 씬에 저장된 `FairinoLiveSmokeCanvas` 패널을 사용하는 것입니다.

패널에서 할 수 있는 것:

- IP 입력
- `연결 신호 확인` 버튼 실행
- 마지막 성공 / 실패 결과 확인

이 패널은 코드로 즉석 생성되는 UI가 아니라, 씬에 저장된 authored UI입니다.
따라서 Hierarchy에서 선택해서 위치, 크기, 색상, 텍스트를 직접 수정할 수 있습니다.

기존 Editor 메뉴도 계속 사용할 수 있습니다.

- `RobotTemplate > Run FAIRINO Live Smoke Test`

## 환경 변수로 IP 바꾸기

기본값 대신 환경 변수로 장비 주소를 바꿀 수 있습니다.

- `FAIRINO_IP`

예시:

- `FAIRINO_IP=192.168.58.2`

FAIRINO C# SDK의 연결 함수는 `RPC(ip)` 형태이므로 이 smoke에서는 포트 환경 변수를 사용하지 않습니다.

## 기대 결과

성공 시:

- `CONNECT_OK`
- version 정보 표시
- joints / tcp / state 값 표시

실패 시:

- `CONNECT_FAIL`
- IP / error code / message 표시

## 2026-03-25 실제 확인 결과

2026년 3월 25일 기준으로 템플릿 프로젝트에서 smoke를 실행한 결과는 아래와 같았습니다.

- smoke 코드 실행 성공
- SDK DLL 로드 경로 정상
- 결과: `CONNECT_FAIL ip=192.168.58.2 code=-2`

이 결과는 "코드가 실행되지 않는다"는 뜻이 아니라,  
"지정한 컨트롤러 주소에서 응답을 받지 못했다"는 뜻입니다.

## 실패했을 때 가장 먼저 볼 것

1. 컨트롤러 전원이 켜져 있는지 확인합니다.
2. 테스트 PC와 컨트롤러가 같은 네트워크 대역인지 확인합니다.
3. 실제 FR5 컨트롤러 IP가 맞는지 확인합니다.
4. PC에서 대상 IP로 네트워크 reachability가 되는지 확인합니다.
5. SimMachine 기본 IP `192.168.58.2`와 실제 장비 포트 IP가 다른지 확인합니다.

## 로그 위치

배치 실행 로그는 아래 파일에서 확인할 수 있습니다.

- `Logs/fairino-live-smoke.log`

문제 파악 순서는 [TROUBLESHOOTING.md](TROUBLESHOOTING.md)를 참고하세요.
