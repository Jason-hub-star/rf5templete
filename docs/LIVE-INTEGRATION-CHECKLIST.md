# FAIRINO FR5 실기기 연동 체크리스트

## 목적

이 체크리스트는 `robotapp2`에서 실기 연동을 검증하고,  
검증된 범위만 `robottemplete`로 이식할 때 빠뜨리지 않도록 돕기 위한 문서입니다.

## 1. 하드웨어 확인

- FR5 컨트롤러 전원 ON
- 로봇 본체 전원 및 상태 확인
- Emergency stop 해제 상태 확인
- 현장 safety perimeter 확보
- 수동 개입 가능 인원 대기

## 2. 네트워크 확인

- 테스트 PC와 컨트롤러가 같은 네트워크 대역인지 확인
- 실제 컨트롤러 IP 확인
- 테스트 포트 확인
- PC에서 ping 또는 TCP reachability 확인

## 3. SDK 확인

- `libfairino.dll` 존재
- `CookComputing.XmlRpcV2.dll` 존재
- Unity 재시작 후 assembly load 문제 없는지 확인

## 4. read-only smoke 확인

아래 네 단계만 먼저 확인합니다.

- `Connect`
- `GetVersion`
- `ReadState`
- `Disconnect`

실패 시 반드시 아래를 기록합니다.

- IP
- port
- error code
- error message
- controller power 상태
- network 상태

## 5. 안전 모션 확인

`read-only smoke`가 성공한 뒤에만 진행합니다.

- `Enable`
- joint limit 검증
- speed / acc low preset 확인
- 작은 `MoveJ` 1회
- `StopMotion` 테스트

## 6. 진단 품질 확인

- version text 표시 확인
- latest state 표시 확인
- last error 표시 확인
- retry hint 표시 확인

## 7. 템플릿 이식 가능 여부 판단

아래가 모두 만족될 때만 `robottemplete` 정식 이식을 시작합니다.

- read-only smoke 성공
- enable 성공
- small `MoveJ` 성공
- 오류 메시지 품질 확인
- 현장 절차 문서화 완료

## 8. 템플릿으로 나중에 옮길 파일

- `Assets/Scripts/App/Fairino/IFairinoRobotClient.cs`
- `Assets/Scripts/App/Fairino/FairinoResult.cs`
- `Assets/Scripts/App/Fairino/FairinoErrorTranslator.cs`
- `Assets/Scripts/App/Fairino/FairinoVersionInfo.cs`
- `Assets/Scripts/App/Fairino/FairinoRobotState.cs`
- `Assets/Scripts/App/Fairino/LiveFairinoClient.cs`
- `Assets/Scripts/App/Fairino/MockFairinoClient.cs`
- `Assets/Scripts/App/Fairino/FairinoConnectionService.cs`

## 9. robotapp2에 남겨둘 것

- RobotControl scene shell
- full connection panel UX
- diagnostics drawer UX
- product flow specific scene routing
- `robotapp2` 전용 onboarding / library 연결
