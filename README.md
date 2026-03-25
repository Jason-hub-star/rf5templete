# FR5 슬림 템플릿

`robottemplete`는 FAIRINO FR5를 다른 Unity 프로젝트로 옮겨 쓰기 쉽게 정리한 슬림 템플릿 프로젝트입니다.  
데모 씬, preview/control 프리팹, URDF/mesh 자산, 최소 스크립트만 남겨 재사용 기준선으로 쓰는 것이 목적입니다.

## 이 저장소로 할 수 있는 것

- FR5 데모를 빠르게 열어보기
- FR5 preview / control 프리팹을 다른 프로젝트로 재사용하기
- URDF와 mesh 자산을 기준 자산으로 보관하기
- URP 기준으로 분홍색 머티리얼 문제를 줄인 상태로 템플릿 유지하기
- 실기기 연동 전 단계로 `read-only live smoke`를 수행해 연결 신호만 확인하기

## 포함된 것

- `Assets/Scenes/FR5_Template_Demo.unity`
- `FR5TemplateMinimalController`
- `TemplateFAIRINO_FR5`
- `RobotKinematicsFacade`
- `FairinoUrdfJointDriver`
- `JointRotationHandle`
- `SharedLineMaterial`
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab`
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab`
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Preview.mat`
- `Assets/Runtime/Robots/FAIRINO_FR5/` 전체
- 최소 실기 확인용 `read-only live smoke`

## 포함되지 않은 것

- full `RobotControlSceneCoordinator`
- 광범위한 `Assets/Scripts/UI`
- teaching / playback / diagnostics shell
- 실기기 motion 제어 UI
- onboarding / robot library / glossary / 일반 앱 흐름

## 빠른 시작

1. Unity `6000.0.64f1`로 프로젝트를 엽니다.
2. `Assets/Scenes/FR5_Template_Demo.unity`를 엽니다.
3. Play를 실행합니다.
4. 관절 링을 드래그해 로봇 자세가 바뀌는지 확인합니다.
5. 로봇이 분홍색이면 `RobotTemplate > Fix URP Pipeline`을 실행합니다.

세팅 자세한 내용은 [docs/SETUP.md](C:/Users/ezen601/Desktop/Jason/robottemplete/docs/SETUP.md), 사용 흐름은 [docs/USAGE.md](C:/Users/ezen601/Desktop/Jason/robottemplete/docs/USAGE.md)를 보면 됩니다.

## 실기기 연동 관련 현재 상태

이 템플릿은 아직 "실기 제어" 템플릿이 아닙니다.  
대신 안전한 사전 점검용으로 `read-only live smoke`만 포함합니다.

`read-only live smoke`에서 확인하는 것:

- `Connect`
- `GetVersion`
- `ReadState`
- `Disconnect`

`read-only live smoke`에서 하지 않는 것:

- `Enable`
- `MoveJ`
- `MoveL`
- `ServoJ`
- 실제 모션 명령

데모 씬에는 `FairinoLiveSmokeCanvas` 오브젝트가 실제로 저장되어 있으며,
씬 화면에서 직접 위치, 색, 텍스트, 버튼 구성을 수정할 수 있습니다.
Play 중에는 이 패널에서 IP/포트를 입력한 뒤 `연결 신호 확인` 버튼으로 결과를 바로 볼 수 있습니다.

사용법은 [docs/LIVE-SMOKE.md](C:/Users/ezen601/Desktop/Jason/robottemplete/docs/LIVE-SMOKE.md)를 보세요.

## 2026-03-25 기준 확인 결과

2026년 3월 25일 기준으로 템플릿 프로젝트에서 아래 항목을 확인했습니다.

- 컴파일 오류 0건
- `read-only live smoke` 실행 성공
- 다만 실제 연결은 `192.168.58.2:8080`에서 응답이 없어 `CONNECT_FAIL` 발생

즉 현재 상태는 "코드와 DLL 이식은 완료, 실제 컨트롤러 네트워크 응답은 아직 미확인"입니다.

문제 파악은 [docs/TROUBLESHOOTING.md](C:/Users/ezen601/Desktop/Jason/robottemplete/docs/TROUBLESHOOTING.md),  
이식 준비 문서는 [docs/LIVE-INTEGRATION-PREP.md](C:/Users/ezen601/Desktop/Jason/robottemplete/docs/LIVE-INTEGRATION-PREP.md),  
현장 체크리스트는 [docs/LIVE-INTEGRATION-CHECKLIST.md](C:/Users/ezen601/Desktop/Jason/robottemplete/docs/LIVE-INTEGRATION-CHECKLIST.md)를 참고하세요.

## 프리팹 역할

- `FAIRINO_FR5.prefab`
  쇼룸, 카드 프리뷰 같은 비제어 표시용 프리팹입니다.
- `FAIRINO_FR5_Control.prefab`
  관절 조작과 3D 제어용 프리팹입니다.

## 저장소 구조

- `Assets/Runtime/Resources/Robots/`
  런타임에서 직접 참조하는 preview / control 자산
- `Assets/Runtime/Robots/FAIRINO_FR5/`
  URDF, mesh, material 소스
- `Assets/Scripts/`
  템플릿 최소 스크립트
- `Assets/Plugins/Fairino/`
  실기 smoke용 SDK DLL 위치
- `ProjectSettings/`
  Unity/URP 프로젝트 설정
- `docs/`
  세팅법, 사용법, smoke, 문제 해결 문서

## 참고

- 이 저장소는 Unity 생성 산출물(`Library`, `Temp`, `Logs`) 없이 소스만 버전 관리하도록 구성했습니다.
- `Assets/Settings/URP/` 아래 템플릿 전용 URP 자산을 생성해 분홍색 머티리얼 문제를 줄였습니다.
- 실기 제어 기능 전체는 아직 포함하지 않습니다.
- `com.unityctl.bridge`는 로컬 `file:` 경로가 아니라 Git 릴리스 `v0.3.5`로 고정해 두었습니다. 첫 오픈 시 패키지 resolve 때문에 약간 더 느릴 수 있습니다.
