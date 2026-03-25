# 실기기 연동 이식 준비 문서

## 목적

이 문서는 `robotapp2`에서 정리한 FAIRINO FR5 실기 연동 기능 중,  
나중에 `robottemplete`로 어떤 범위를 옮길지 정리하기 위한 기준 문서입니다.

핵심 원칙은 단순합니다.

- 먼저 `robotapp2`에서 실기 연동을 검증합니다.
- 검증된 공통층만 `robottemplete`로 옮깁니다.
- `robottemplete`는 가능한 한 슬림 템플릿 상태를 유지합니다.

## 현재 상태

2026년 3월 25일 기준 상태:

- `robotapp2`: 실기 연동 공통층과 smoke 경로가 준비되어 있음
- `robottemplete`: `read-only live smoke`만 이식 완료
- `robottemplete` smoke 결과: `CONNECT_FAIL`이며, 현재는 네트워크/장비 응답 미확인 상태

즉, 템플릿에는 "연결 신호 확인용 최소 기능"만 들어간 상태입니다.

## 나중에 이식 후보가 되는 공통층

`robotapp2`에서 충분히 검증되면 아래 공통층은 정식 이식 후보가 됩니다.

- `IFairinoRobotClient`
- `FairinoResult`
- `FairinoErrorTranslator`
- `FairinoVersionInfo`
- `FairinoRobotState`
- `LiveFairinoClient`
- `MockFairinoClient`
- `FairinoConnectionService`
- live smoke test helper

## 그대로 옮기면 안 되는 것

아래는 현장 의존성이 강해서 그대로 템플릿화하면 안 됩니다.

- 특정 현장 기본 IP
- 특정 컨트롤러 포트 가정
- 현장 safety 절차 문구
- 운영 승인 dialog 정책
- 현장 로그 수집 정책
- `robotapp2` 전체 RobotControl UI 셸

## 권장 구조

### 기본 템플릿

`robottemplete`는 계속 아래 범위를 유지합니다.

- FR5 scene
- preview / control prefab
- URDF / mesh / material
- 최소 interaction
- read-only live smoke

### 나중에 붙일 live add-on

실기 연동 전체는 별도 add-on 또는 2차 템플릿으로 관리하는 것을 권장합니다.

예상 구조:

- `Assets/Scripts/Live/Fairino/*`
- `Assets/Editor/Live/FairinoLiveSmokeTools.cs`
- `Assets/Plugins/Fairino/*`

## 정식 이식 시작 조건

`robotapp2`에서 아래가 완료되어야 합니다.

1. `Connect` 성공
2. `GetVersion` 성공
3. `ReadState` 성공
4. `Enable` 성공
5. 작은 `MoveJ` 성공
6. 실패 시 사용자 메시지가 충분히 이해 가능

## 이식 경계 규칙

`robottemplete`로는 "검증된 공통 코드"만 옮깁니다.

- OK: adapter, DTO, translator, smoke helper
- HOLD: scene-specific UI, product-specific flow, onboarding, library, diagnostics shell
