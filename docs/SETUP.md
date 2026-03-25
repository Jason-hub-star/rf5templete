# 세팅 가이드

## 목적

이 문서는 동료가 `robottemplete`를 처음 받아도 바로 열고,  
분홍색 머티리얼이나 기본 실행 문제를 빠르게 해결할 수 있도록 정리한 세팅 가이드입니다.

## 준비물

- Windows
- Unity `6000.0.64f1`
- 인터넷 연결

첫 import 때 `Packages/manifest.json`에 들어 있는 패키지를 내려받기 때문에, 처음 열 때는 네트워크가 필요할 수 있습니다.
또한 `com.unityctl.bridge`는 Git 릴리스 `v0.3.5`를 사용하므로, 첫 오픈에서는 패키지 resolve 때문에 평소보다 시간이 조금 더 걸릴 수 있습니다.

## 프로젝트 열기

1. Unity Hub에서 `Add project from disk`를 선택합니다.
2. 프로젝트 루트로 `robottemplete` 폴더를 지정합니다.
3. Unity Editor 버전은 반드시 `6000.0.64f1`을 사용합니다.
4. 첫 import가 끝날 때까지 기다립니다.

## 처음 열었을 때 확인할 것

1. 씬이 열리지 않으면 `Assets/Scenes/FR5_Template_Demo.unity`를 직접 엽니다.
2. 로봇이 분홍색이면 `RobotTemplate > Fix URP Pipeline`을 실행합니다.
3. 스크립트 에러가 있으면 먼저 콘솔 에러를 해결한 뒤 다시 확인합니다.

## URP 분홍색 문제 해결

이 프로젝트는 URP 기반입니다.  
처음 열었을 때 로봇이 분홍색으로 보이면 Unity 상단 메뉴에서 아래 항목을 실행합니다.

- `RobotTemplate > Fix URP Pipeline`

이 메뉴는 다음 자산과 설정을 자동으로 맞춥니다.

- `Assets/Settings/URP/RobotTemplate-Renderer.asset`
- `Assets/Settings/URP/RobotTemplate-URP.asset`
- `ProjectSettings/GraphicsSettings.asset`
- `ProjectSettings/QualitySettings.asset`

## 실기기 smoke를 위한 추가 준비

실기기 연동 전체는 아직 포함하지 않지만, 연결 신호만 보는 `read-only live smoke`는 포함되어 있습니다.

아래 DLL이 `Assets/Plugins/Fairino/`에 있어야 합니다.

- `libfairino.dll`
- `CookComputing.XmlRpcV2.dll`

현재 저장소에는 이미 포함되어 있으므로, 별도 삭제만 하지 않았다면 추가 작업 없이 smoke를 시도할 수 있습니다.

## 꼭 확인할 주요 파일

- `Assets/Scenes/FR5_Template_Demo.unity`
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5.prefab`
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control.prefab`
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Preview.mat`
- `Assets/Runtime/Robots/FAIRINO_FR5/fairino5_v6.urdf`
- `Assets/Plugins/Fairino/libfairino.dll`
- `Assets/Plugins/Fairino/CookComputing.XmlRpcV2.dll`

## 열기 실패 시 점검 순서

1. Unity 버전이 `6000.0.64f1`인지 확인합니다.
2. 패키지 import가 아직 끝나지 않았는지 확인합니다.
3. 로봇이 분홍색이면 `RobotTemplate > Fix URP Pipeline`을 실행합니다.
4. 콘솔에 compile error가 없는지 확인합니다.
5. 필요하면 Unity를 완전히 껐다가 프로젝트만 다시 엽니다.

추가 장애 대응은 [TROUBLESHOOTING.md](TROUBLESHOOTING.md)를 참고하세요.
