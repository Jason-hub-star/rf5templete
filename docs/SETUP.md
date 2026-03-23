# Setup Guide

## Goal

`robottemplete`는 FAIRINO FR5용 슬림 Unity 템플릿 프로젝트입니다.
이 프로젝트를 열면 FR5 데모 씬, 프리뷰 프리팹, 컨트롤 프리팹, URDF/mesh 자산을 바로 확인할 수 있습니다.

## Requirements

- Windows
- Unity `6000.0.64f1`
- 인터넷 연결
  `Packages/manifest.json`에 URDF Importer Git 패키지가 포함되어 있어 첫 import 때 내려받습니다.

## Open The Project

1. Unity Hub에서 `Add project from disk`를 선택합니다.
2. 프로젝트 루트로 `robottemplete` 폴더를 지정합니다.
3. Unity Editor 버전은 `6000.0.64f1`을 사용합니다.
4. 첫 import가 끝날 때까지 기다립니다.

## URP Fix

이 프로젝트는 URP 기반입니다.
처음 열었을 때 로봇이 분홍색으로 보이면 Unity 상단 메뉴에서 아래 항목을 실행합니다.

- `RobotTemplate > Fix URP Pipeline`

이 메뉴는 다음을 자동으로 맞춥니다.

- `Assets/Settings/URP/RobotTemplate-Renderer.asset`
- `Assets/Settings/URP/RobotTemplate-URP.asset`
- `ProjectSettings/GraphicsSettings.asset`
- `ProjectSettings/QualitySettings.asset`

## Important Files

- `Assets/Scenes/FR5_Template_Demo.unity`
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab`
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab`
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Preview.mat`
- `Assets/Runtime/Robots/FAIRINO_FR5/fairino5_v6.urdf`

## If Opening Fails

1. Unity 버전이 `6000.0.64f1`인지 확인합니다.
2. 패키지 import가 끝날 때까지 기다립니다.
3. 로봇이 분홍색이면 `RobotTemplate > Fix URP Pipeline`을 실행합니다.
4. 씬이 비어 보이면 `Assets/Scenes/FR5_Template_Demo.unity`를 직접 엽니다.
