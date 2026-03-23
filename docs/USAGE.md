# Usage Guide

## What This Template Includes

- FR5 데모 씬
- FR5 preview prefab
- FR5 control prefab
- FR5 URDF와 mesh 자산
- 최소 kinematics / controller 스크립트

## Recommended Flow

1. `Assets/Scenes/FR5_Template_Demo.unity`를 엽니다.
2. Play를 실행합니다.
3. 관절 링을 드래그해 로봇 자세가 바뀌는지 확인합니다.
4. 필요하면 preview prefab과 control prefab을 각각 다른 프로젝트로 복사하거나 참조합니다.

## Asset Roles

### Preview Prefab

- 파일: `Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab`
- 용도: 쇼룸, 카드 프리뷰, 비제어 표시용

### Control Prefab

- 파일: `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab`
- 용도: 3D 조작, joint drive, 제어 콘솔 연결용

### URDF Source

- 파일: `Assets/Runtime/Robots/FAIRINO_FR5/fairino5_v6.urdf`
- 용도: 재임포트, 구조 확인, 링크/조인트 기준 자산

## Reuse In Another Project

가장 단순한 방법:

1. `Assets/Runtime/Resources/Robots/`
2. `Assets/Runtime/Robots/FAIRINO_FR5/`
3. FR5 관련 `Assets/Scripts/`

를 대상 프로젝트에 복사합니다.

대상 프로젝트도 URP를 사용해야 합니다.
분홍색 머티리얼이 보이면 대상 프로젝트에서도 URP 파이프라인 자산이 연결되어 있는지 확인해야 합니다.

## Known Scope

이 템플릿은 slim 버전이라 아래는 포함하지 않습니다.

- 전체 RobotControlSceneCoordinator
- 광범위한 일반 UI 플로우
- onboarding / robot library / glossary
- 실기 SDK / live connection DLL
