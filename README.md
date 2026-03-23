# FR5 Slim Template

`robottemplete`는 FAIRINO FR5를 다른 Unity 프로젝트로 옮겨 쓰기 쉽게 정리한 슬림 템플릿 프로젝트입니다.
이 저장소는 데모 씬, preview prefab, control prefab, URDF/mesh 자산, 최소 스크립트만 남긴 재사용용 기준선입니다.

## What This Repo Is For

- FR5 데모를 빠르게 열어보기
- FR5 preview / control prefab을 다른 프로젝트로 재사용하기
- URDF와 mesh 자산을 기준 자산으로 보관하기
- 분홍색 머티리얼 문제 없이 URP 기준으로 템플릿을 유지하기

## Included

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

## Not Included

- full `RobotControlSceneCoordinator`
- 광범위한 `Assets/Scripts/UI`
- teaching / playback / diagnostics
- live SDK / DLL
- onboarding / robot library / glossary / 일반 앱 흐름

## Preview vs Control

- `FAIRINO_FR5.prefab`
  쇼룸이나 카드 프리뷰 같은 비제어 표시용 프리팹입니다.
- `FAIRINO_FR5_Control.prefab`
  관절 조작과 3D 제어용 프리팹입니다.

## Setup

환경 준비와 프로젝트 열기 방법은 [docs/SETUP.md](docs/SETUP.md)를 보세요.

중요 포인트:

- Unity `6000.0.64f1`
- 첫 import 후 로봇이 분홍색이면 `RobotTemplate > Fix URP Pipeline`

## Usage

실제 사용 흐름은 [docs/USAGE.md](docs/USAGE.md)를 보세요.

가장 빠른 확인 순서:

1. `Assets/Scenes/FR5_Template_Demo.unity` 열기
2. Play 실행
3. 관절 링 드래그
4. 로봇 자세와 3D 포즈 동기화 확인

## Repo Structure

- `Assets/Runtime/Resources/Robots/`
  런타임에서 직접 참조하는 preview / control 자산
- `Assets/Runtime/Robots/FAIRINO_FR5/`
  URDF, mesh, material 소스
- `Assets/Scripts/`
  템플릿 최소 스크립트
- `ProjectSettings/`
  Unity/URP 프로젝트 설정
- `docs/`
  세팅법과 사용법 문서

## Notes

- 이 저장소는 Unity 생성 산출물(`Library`, `Temp`, `Logs`) 없이 소스만 버전 관리하도록 구성했습니다.
- `Assets/Settings/URP/` 아래에 템플릿 전용 URP 자산을 생성해 분홍색 머티리얼 문제를 줄였습니다.
