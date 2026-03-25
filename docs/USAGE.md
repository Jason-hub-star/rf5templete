# 사용 가이드

## 이 템플릿에 들어 있는 것

- FR5 데모 씬
- FR5 preview prefab
- FR5 control prefab
- FR5 URDF와 mesh 자산
- 최소 kinematics / controller 스크립트
- 연결 신호 확인용 `read-only live smoke`

## 권장 확인 순서

1. `Assets/Scenes/FR5_Template_Demo.unity`를 엽니다.
2. Play를 실행합니다.
3. 관절 링을 드래그해 로봇 자세가 바뀌는지 확인합니다.
4. 필요하면 preview prefab과 control prefab을 각각 다른 프로젝트로 복사하거나 참조합니다.
5. 실기기 연결 신호만 보고 싶다면 Play 중 씬에 배치된 `FairinoLiveSmokeCanvas` 패널에서 `연결 신호 확인`을 누릅니다.

## 자산별 역할

### Preview Prefab

- 파일: `Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab`
- 용도: 쇼룸, 카드 프리뷰, 비제어 표시용

### Control Prefab

- 파일: `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab`
- 용도: 3D 조작, joint drive, 제어 콘솔 연결용

### URDF Source

- 파일: `Assets/Runtime/Robots/FAIRINO_FR5/fairino5_v6.urdf`
- 용도: 재임포트, 구조 확인, 링크/조인트 기준 자산

## 다른 프로젝트에서 재사용하는 방법

가장 단순한 방법은 아래 범위를 함께 옮기는 것입니다.

1. `Assets/Runtime/Resources/Robots/`
2. `Assets/Runtime/Robots/FAIRINO_FR5/`
3. FR5 관련 `Assets/Scripts/`

대상 프로젝트도 URP를 사용해야 합니다.  
분홍색 머티리얼이 보이면 대상 프로젝트에서도 URP 파이프라인 자산이 연결되어 있는지 확인해야 합니다.

## Live Smoke 사용 시 주의

`read-only live smoke`는 "연결 신호 확인"만 위한 기능입니다.

확인하는 것:

- `Connect`
- `GetVersion`
- `ReadState`
- `Disconnect`

씬에 실제로 저장된 `FairinoLiveSmokeCanvas` 패널에서 아래를 바로 확인할 수 있습니다.

- 현재 IP / port
- 실행 상태
- 마지막 성공 / 실패 메시지
- version / joints / tcp / safety 정보

이 패널은 씬 authored UI라서, Hierarchy에서 선택한 뒤 직접 수정할 수 있습니다.

하지 않는 것:

- `Enable`
- `MoveJ`
- `MoveL`
- `ServoJ`
- 실제 로봇 모션

## 현재 범위

이 템플릿은 slim 버전이라 아래는 포함하지 않습니다.

- 전체 `RobotControlSceneCoordinator`
- 광범위한 일반 UI 플로우
- onboarding / robot library / glossary
- full diagnostics shell
- 실기기 모션 제어 UX
