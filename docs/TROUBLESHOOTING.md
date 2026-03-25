# 문제 해결 가이드

## 목적

이 문서는 동료가 `robottemplete`를 열었을 때 자주 만나는 문제를 빠르게 분류하고,  
무엇부터 확인해야 하는지 바로 알 수 있도록 만든 운영 문서입니다.

## 가장 빠른 점검 순서

1. Unity 버전이 `6000.0.64f1`인지 확인합니다.
2. 콘솔에 compile error가 있는지 확인합니다.
3. 로봇이 분홍색인지 확인합니다.
4. live smoke가 필요한 경우 DLL이 있는지 확인합니다.
5. `CONNECT_FAIL`이면 코드보다 네트워크/장비 상태를 먼저 봅니다.

## 증상별 대응

### 1. 로봇이 분홍색으로 보임

가능성이 큰 원인:

- URP 파이프라인 설정 누락
- 머티리얼이 Standard 계열로 남아 있음

먼저 할 일:

1. `RobotTemplate > Fix URP Pipeline` 실행
2. `ProjectSettings/GraphicsSettings.asset`와 `ProjectSettings/QualitySettings.asset`가 URP 자산을 가리키는지 확인
3. 다시 씬을 열어 확인

### 2. 씬은 열리는데 로봇이 이상하게 보임

가능성이 큰 원인:

- import가 아직 끝나지 않음
- mesh 또는 URDF 관련 asset import가 덜 끝남

먼저 할 일:

1. Asset import가 끝날 때까지 기다림
2. `Assets/Scenes/FR5_Template_Demo.unity` 다시 열기
3. 필요하면 Unity 재시작

### 3. live smoke에서 `CONNECT_FAIL` 발생

이 증상은 보통 "코드 이식 실패"보다 아래 원인일 가능성이 더 큽니다.

- 컨트롤러 전원 꺼짐
- IP 주소 불일치
- 포트 불일치
- PC와 컨트롤러 네트워크 대역 불일치
- 방화벽 또는 네트워크 차단

먼저 할 일:

1. 실제 컨트롤러 IP 재확인
2. 포트 재확인
3. PC에서 ping 확인
4. PC에서 TCP 연결 확인
5. 그래도 안 되면 현장 네트워크 상태 확인

## 2026-03-25 기준 실제 확인 결과

2026년 3월 25일 템플릿 프로젝트에서 확인된 상태는 아래와 같습니다.

- compile error 0건
- live smoke 실행 성공
- 결과는 `CONNECT_FAIL ip=192.168.58.2 port=8080 code=-2`
- ping 실패
- TCP `8080` 실패

이 결과는 현재 코드 경로와 DLL 배치는 완료되었고,  
실제 장비 응답만 아직 확인되지 않았다는 뜻입니다.

## 동료에게 바로 전달할 핵심 문장

- "템플릿 프로젝트 자체는 열리고 컴파일도 됩니다."
- "분홍색이면 URP Fix 메뉴를 먼저 실행하면 됩니다."
- "live smoke는 들어가 있지만, 현재 장비 IP 응답이 없어 연결 단계에서 실패했습니다."
- "지금 남은 문제는 코드보다 네트워크/장비 확인에 가깝습니다."
