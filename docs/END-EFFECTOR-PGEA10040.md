# PGEA-100-40 End Effector Setup

## 목적

`robottemplete`에서 FR5 + PGEA-100-40 조합을 팀 공용 기준선으로 고정한다.

- Unity 런타임에는 `STL`만 사용한다.
- `STEP`은 원본 CAD 보관본으로만 둔다.
- 메쉬 원점과 실제 TCP는 분리한다.
- 팀원은 "이미 그리퍼가 붙은 FR5 prefab"과 "같은 TCP 기준점"을 바로 사용할 수 있어야 한다.

## 문서 근거

`robotapp2` 기준으로 아래 문서를 우선 따른다.

- `docs/ref/product/robotcontrol-fr5-v2/fr5-command-ssot.md`
- `docs/ref/product/robotcontrol-fr5-v2/fr5-gripper-tcp-calibration-spec.md`
- `docs/ref/product/robotcontrol-fr5-v2/fr5-ui-capability-map.md`
- `docs/ref/product/ux/robotcontrol-soft-teaching-pad.md`

핵심 잠금 규칙:

1. 실기 기준 TCP는 bare flange가 아니라 `gripper TCP`다.
2. `STL` / `STEP` 원점은 시각화 참고 자료일 뿐, TCP SSOT가 아니다.
3. scene preview, TCP marker, point save, MoveL 해석은 같은 TCP 기준을 공유해야 한다.
4. 팀이 편하게 쓰려면 "엔드이펙터 prefab"보다 먼저 "장착 prefab + TCP frame"이 준비돼 있어야 한다.
5. 기본 장착 방향은 `그리퍼 작동부가 로봇 바깥쪽`을 향하고, `반대쪽 마운트 면이 wrist3 / flange`에 붙는 상태로 잠근다.

## 저장소 배치

### 런타임 시각 자산 (3파트 STL, FreeCAD 분리)

- `Assets/Runtime/EndEffectors/PGEA_100_40/Source/PGEA-100-40_body.stl` (본체, 61,774 tri)
- `Assets/Runtime/EndEffectors/PGEA_100_40/Source/PGEA-100-40_finger_left.stl` (좌 핑거, 4,164 tri)
- `Assets/Runtime/EndEffectors/PGEA_100_40/Source/PGEA-100-40_finger_right.stl` (우 핑거, 4,164 tri)
- `Assets/Runtime/Resources/EndEffectors/PGEA_100_40.prefab`

### 레거시 (교체됨, 참고용)

- `Assets/Runtime/EndEffectors/PGEA_100_40/Source/PGEA-100-40.stl` (단일 STL, 이전 기준선)
- `Assets/Runtime/EndEffectors/PGEA_100_40/Source/PGEA-100-40.fbx` (FBX 비교본)

### 원본 CAD

- `archive/EndEffectors/PGEA_100_40/CAD/` (STEP 파일 보관 예정)
- FreeCAD 프로젝트에서 STEP → body/finger_left/finger_right로 분리 후 STL 내보내기

### 팀 공용 즉시 사용 prefab

- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control_PGEA10040.prefab`
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_PGEA10040.prefab`

## 팀 우선순위

### 1. FreeCAD 3파트 STL을 Unity visual source로 고정

- Unity에서 실제로 붙는 것은 FreeCAD에서 분리한 3파트 `STL` (body + finger_left + finger_right)
- `STEP`은 치수/원점 검토용, FreeCAD 파트 분리의 소스
- 팀원은 `Resources/EndEffectors/PGEA_100_40.prefab`만 직접 참조하면 된다
- Inspector에서 `Gripper Open Ratio` 슬라이더로 개폐 즉시 확인 가능

### 2. FR5 control/preview variant를 기본 사용

- 팀원이 직접 wrist3에 붙이지 않게 한다
- `FAIRINO_FR5_Control_PGEA10040.prefab`를 장착값 SSOT로 둔다
- `FAIRINO_FR5_PGEA10040.prefab`와 에디터 preview는 Control의 엔드이펙터 포즈를 따라간다
- 데모 씬과 런타임 로더는 attached variant를 먼저 찾고, 없을 때만 bare FR5로 fallback 한다
- attached variant의 기본 장착 방향은 `ToolMount -> 바깥쪽 = gripper side` 규칙을 따른다

### 3. TCP frame을 prefab 안에 별도 child로 둔다

- 이름: `TcpFrame`
- 이것이 preview, marker, future MoveL 기준점이 된다
- mesh origin은 TCP가 아니다
- `TcpFrame`은 장착 방향을 바꿔도 계속 `실제 작업점` 기준으로 유지한다

### 3-1. 방향 규칙

- `wrist3_link/ToolMount`에 붙는 면은 그리퍼의 `마운트/후면`이다
- `jaw / gripping side`는 로봇 바깥 방향을 향해야 한다
- `ToolMount`는 identity(무회전)로 둔다 — wrist3의 좌표계를 그대로 따른다
- 방향 정렬은 `PGEA_100_40` 인스턴스의 로컬 Transform에서 한다
- 현재 authored 값: `rotation Z -91.6°`, `position (0.003, 0.1676, 0.031)`
- 이 값은 STL 메쉬 원점과 wrist3 좌표계의 차이를 보정하기 위한 **시각적 정렬 값**이다
- 2026-04-03 기준으로 X축을 1mm 줄여, 기존 STL 배치감을 유지하면서도 보고된 미세 편심만 완화했다
- 현재 기준선은 `FAIRINO_FR5_Control_PGEA10040.prefab`에 저장된 장착값이며, preview는 로드 시 이 값을 복사한다
- 실기 TCP calibration 값과는 별개이며, calibration 후 `TcpFrame`을 다시 잠궈야 한다

### 4. 조정 기준은 Tool/Base/World 모두 허용

- `FR5EndEffectorAttachment` inspector에서 `Tool / Base / World` 기준으로 TCP를 nudge 할 수 있다
- 첫 배치는 대략 맞추고, 실제 calibration 값은 나중에 `TcpFrame`으로 옮긴다

## 사용 방법

1. 최초 설치 또는 메쉬 재임포트가 필요할 때 `RobotTemplate/End Effector/Install PGEA-100-40 On FR5`를 실행한다
2. 장착 미세 조정은 `FAIRINO_FR5_Control_PGEA10040.prefab`에서 한다
3. `PGEA_100_40`의 `FR5EndEffectorAttachment`를 선택해 `Tool / Base / World` 기준으로 `TcpFrame`을 조정한다
4. Scene/Game view에서 TCP marker와 gizmo를 보며 위치를 맞춘다
5. preview asset을 즉시 저장 동기화하려면 `Sync Preview Variant From Control` 메뉴 또는 Inspector 버튼을 사용한다
6. 에디터 preview와 preview reference는 로드 시 Control 엔드이펙터 포즈를 다시 복사한다

## 좌표계 주의사항

### 시각적 정렬 ≠ TCP 좌표계

현재 `PGEA_100_40` 인스턴스의 rotation/position은 **시각적 정렬 값**이다.

- STL 메쉬의 원점 방향이 wrist3 좌표계와 일치하지 않아서 수동 보정한 값이다
- 이 값은 실기 `tool coordinate calibration` 결과와 **다를 수 있다**
- `TcpFrame`의 world position도 이 보정에 종속되어 있다

### 실기 연동 시 필수 작업

1. pendant 또는 SDK로 tool TCP를 calibration한다
2. calibration 결과 `[x,y,z,rx,ry,rz]`를 `TcpFrame`의 localPosition/localRotation으로 옮긴다
3. 아래 3개가 일치하는지 확인한다 (must match 규칙):
   - pendant/current application의 tool coordinate
   - `GetActualTCPNum` / `GetCurToolCoord` 읽기 값
   - preview/EE marker가 사용하는 `TcpFrame` offset

### 아직 구현되지 않은 유기적 연결 항목

| 항목 | 설명 | 필요 시점 |
|---|---|---|
| TCP 읽기 명령 | `GetActualTCPPose`, `GetCurToolCoord` 호출 | 실기 연동 시 |
| 좌표 문맥 저장 | `FairinoCoordContext` (tool ID, TCP offset) | teaching 기능 추가 시 |
| Live preview 동기화 | 실기 TCP readback 기반 ghost target / marker offset 반영 | preview 기능 추가 시 |
| Tool coordinate 검증 | "must match" 규칙 자동 확인 | live 운용 시 |

## FK와 TCP offset의 관계

`RobotKinematicsFacade`는 DH 기반 FK 후 TCP 오프셋을 합성할 수 있다.

- `EndEffectorTransform = T_0n × T_tcp`
- `SetTcpOffset(Mat4D)` 메서드로 TCP 변환을 주입한다
- 기본값은 `Mat4D.Identity` — flange 좌표와 동일
- calibration 값이 확정되면 `TemplateFAIRINO_FR5.TcpOffsetMeters`를 업데이트하고 facade에 주입한다
- `TcpCalibrated` 상수가 `false`인 동안은 TCP marker가 노란색 gizmo로 표시된다

## robotapp2 이관 시 필요한 작업

이 슬림 템플릿에서는 read-only live smoke만 지원한다. robotapp2에서 아래 항목을 추가로 구현해야 한다:

1. `SetToolCoord(id, coord[6], type, install)` 호출로 컨트롤러에 TCP 오프셋 등록
2. `FairinoCoordContext` 연동 (ToolId, ToolPose, UserId, WObjPose)
3. `MoveJ`/`MoveL`에 `tool=N`, `user=N` 파라미터 명시 전달
4. `WaypointStore`에 tool context 메타데이터 보관
5. `GetActualTCPPose` / `GetCurToolCoord`를 주기적으로 읽어 preview와 동기화

## 아키텍처 결정 기록

### ADR-001: URDF에 gripper fixed link를 추가하지 않는다

- **결정**: wrist3_link에서 URDF 종료, 엔드이펙터는 prefab hierarchy로 관리
- **이유**: URDF에 link를 추가하면 URDF Importer가 ArticulationBody를 재구성하고, mesh 경로/material 재설정이 필요하다. prefab hierarchy 분리가 부수효과 없이 동일한 결과를 달성한다.
- **대안**: URDF에 `<joint type="fixed">` + `<link name="gripper_base_link">` 추가 — 시각화는 동일하나 importer 의존성이 높아진다

### ADR-002: SetToolCoord를 이 템플릿에서 호출하지 않는다

- **결정**: 슬림 템플릿은 read-only, 쓰기 명령 없음
- **이유**: SetToolCoord는 컨트롤러 상태를 변경하는 쓰기 명령이며, 이 템플릿의 범위(read-only live smoke)를 벗어난다
- **대안**: robotapp2에서 connection service 내에 SetToolCoord 호출 구현

### ADR-003: 시각적 정렬과 TCP 좌표계를 분리한다

- **결정**: PGEA_100_40 인스턴스의 authored transform은 시각적 정렬, TcpFrame은 TCP 좌표계 기준
- **이유**: STL 메쉬 원점과 실제 TCP 위치는 다르다. 시각적으로 맞는 위치가 calibration된 TCP와 일치하지 않을 수 있다.
- **후속**: calibration 완료 시 `TcpFrame` localPosition/localRotation을 calibration 값으로 덮어쓰고, `tcpCalibrated`를 true로 변경

## ROS 연동과 엔드이펙터의 관계

### 현재 데이터 흐름 (hyun-su-kim 커밋 기준)

```
ROS2 /joint_states (라디안)
  → ROS-TCP-Endpoint (192.168.0.99:10000)
  → ROSConnection (Unity ROS-TCP-Connector)
  → RosJointStateSubscriber.OnJointState()
  → FR5TemplateMinimalController.SetJointAnglesDeg(deg[6])
  → FairinoUrdfJointDriver.ApplyJointAngles()     ← 3D 관절 회전
  → RobotKinematicsFacade.SetJointAnglesDegrees() ← FK 계산
```

### 엔드이펙터와 ROS 연동

- 그리퍼는 wrist3_link > ToolMount의 **Transform 자식**이므로, 관절이 움직이면 자동으로 따라감
- ROS와 별도 연동은 불필요 — Transform 계층 구조가 자동 처리
- 그리퍼 개폐는 별도 토픽(`/gripper_command`) 구독이 필요하나 현재 범위 밖

### hyun-su-kim 커밋 보호 규칙

- 커밋 `16784ab`: ROS TCP Connector + RosJointStateSubscriber 추가
- 이 버전으로 **실기기 연동 성공** — 절대 삭제 금지
- MCP for Unity(Assets/MCPForUnity/)는 별개이며 삭제 대상

## STL 메쉬 분석 결과

MeasureGripper.cs로 분석한 PGEA-100-40 STL 원본 좌표 (단위: mm):

| 항목 | 값 |
|---|---|
| 전체 크기 | 138 x 29 x 79.4 mm (X x Y x Z) |
| 가장 긴 축 | X (그리퍼 길이 방향) |
| STL 원점 | 메쉬에서 Y=-325mm 떨어진 곳 (비표준) |
| 플랜지 면 중심 | (-51.77, -325.66, 35.90) |
| Jaw 끝 중심 | (57.48, -325.64, -41.88) |
| Flange→Jaw 오프셋 | (109.25, 0.02, -77.78), 거리 134.11mm |
| X 편심 | 플랜지 X=-51.77 vs 전체 중심 X=2.98 → 54.75mm 차이 |

### 자동 계산 실패 이력

- `ComputeMountAlignedOffset` 플랜지 면 중심 기준 수정 시도 → 값 적용 안 됨
- `Quaternion.Euler(0,0,-90)` 자동 회전 시도 → Setup Tool exec 실패
- **결론**: STL 좌표계가 비표준이라 자동 계산 불안정. authored 수동 정렬이 유일하게 작동한 방식

## 알려진 문제

### 무한 도메인 리로드

- **증상**: Unity Play 모드 진입 시 "Reloading Domain"에서 로딩바 없이 멈춤
- **확인된 원인**: MCP for Unity (50,000줄+) → 삭제로 해결
- **미확인 원인**: 엔드이펙터 코드 복원 후에도 리로드가 느려짐. Editor 스크립트를 빼도 동일 → 런타임 스크립트 또는 ROS 패키지 resolve 지연 가능성
- **현재 조치**: Library 삭제 후 재빌드, Auto Refresh 끄기
- **추가 조사 필요**: Enter Play Mode Settings 활성화 (도메인 리로드 건너뛰기)

### 그리퍼 시각적 방향

- **증상**: authored 값으로 한 번 성공했으나, Setup Tool 재실행 시 방향이 틀어짐
- **원인**: Setup Tool이 재생성 시 tuned variant 값을 다시 참조하지 않으면 bootstrap 값으로 되돌아갈 수 있었음
- **현재 상태**: authored 값(`Z -91.6°, pos (0.003, 0.1676, 0.031)`)이 `FAIRINO_FR5_Control_PGEA10040` 기준으로 반영되고, preview는 Control 값을 복사하도록 보강됨
- **다음 단계**: Play에서 Control/preview가 동일 장착값으로 보이는지 재확인

## 추가 조사 결과 (2026-04-03)

### authored 장착값 유지의 장점

- 현재 authored 값은 STL 원점 비표준 문제를 우회한, 팀이 검증한 visual baseline이다.
- `ToolMount`를 identity로 두고 `PGEA_100_40` 인스턴스에만 authored 보정을 넣는 구조는 wrist3 좌표계를 보존해 ROS 관절 동기화와 attached variant 재생성에 안전하다.
- URDF를 수정하지 않아 FR5의 기존 `ArticulationBody` / mesh / material 구조를 보존한다.

### authored 장착값 유지의 리스크

- authored 값은 **시각 정렬 값**이지 실기 `tool coordinate`가 아니다. pendant/SDK TCP 값으로 재사용하면 live 운용 시 오차가 누적될 수 있다.
- 현재 `TcpFrame`은 calibration 전 placeholder이고, marker도 실제 작업점이 아니라 calibration 대기 기준점에 가깝다.
- `TemplateFAIRINO_FR5.TcpOffsetMeters`는 아직 zero placeholder라 calibration 전에는 FK/preview가 flange 기준으로만 해석된다.
- 즉, 지금 구조에서 authored 값은 반드시 `visual-only`로 잠그고, 실기 TCP는 `TcpFrame` + SDK readback으로 별도 확정해야 한다.

### TCP marker가 제위치에 없는 현재 이유

- `PGEA_100_40.prefab`의 `TcpFrame` localPosition / localRotation은 현재 zero다.
- `FR5EndEffectorAttachment`는 `TcpFrame` 위치 nudge와 gizmo 표시만 제공하고, 실기 TCP readback 동기화는 아직 없다.
- `FR5EndEffectorAttachmentEditor`는 현재 위치 이동 중심 UI이며, Control prefab 수정 시 preview variant를 다시 맞추는 버튼을 제공한다. 자세 보정과 live 비교는 여전히 별도 구현이 필요하다.
- 따라서 현재 노란색 marker는 calibrated TCP가 아니라 `calibration pending` 상태를 보여주는 보조 지표다.

### 현재 구조에서 확인된 확장성 문제

- `RosJointStateSubscriber`는 현재 `/joint_states`에서 `j1~j6`만 읽는다. gripper open/close 명령 경로는 아직 없다.
- `FairinoUrdfJointDriver`는 6개 arm link만 캐싱하고, non-root `ArticulationBody`를 비활성화한다. 따라서 jaw를 URDF `prismatic joint`로 추가해도 현재 런타임 경로와는 바로 맞지 않는다.
- 현재 엔드이펙터 prefab은 `VisualRoot` / `TcpFrame` 구조까지만 있고, `Adapter`, `JawLeft`, `JawRight` 같은 가동부 계층 SSOT가 없다.
- ROS-TCP-Connector Visualizations 패키지는 topic / TF를 **시각화**하는 도구이지, tool TCP SSOT나 gripper 기구학을 대신 관리해 주지 않는다.
- Unity Robotics Hub upstream 예제도 arm trajectory와 tool command를 분리한다. 향후 gripper 개폐는 `/gripper_command` 또는 SDK tool command 경로로 분리하는 편이 맞다.

### 그리퍼 개폐 구현 (완료, ADR-008)

- **방식**: FreeCAD에서 STEP → 3파트 STL 분리 (body + finger_left + finger_right) → Unity Transform 제어
- **구현**: `FR5EndEffectorAttachment.SetGripperOpen(float ratio)` — ratio 0(닫힘)~1(열림)
- **이동축**: 모델 공간 X축 (finger_left +X, finger_right -X, 스트로크 40mm)
- **Inspector**: `Gripper Open Ratio` 슬라이더로 에디터에서 즉시 테스트 가능
- **대칭 검증**: 좌우 finger 동일 (4,164 tri, 53.8×40.5×16.7mm, X축 미러)
- **실기 연동 확장**: `SetGripperOpen` ← ROS `/gripper_command` subscriber 또는 SDK `GetGripperState`

### 실기기 검은 추가 부품과 편심 리스크

- 실기기에 있는 검은 추가 부품(브라켓, 스페이서, 핑거, 어댑터 등)이 prefab/mesh에 없으면, 현재 Unity 자산은 완전한 physical twin이 아니다.
- 이 경우 TCP, 충돌 외형, 질량중심, 관성, 체감 편심이 모두 실제와 달라질 수 있다.
- authored 값으로 시각적으로 잘 맞아 보여도, 그 추가 부품 두께/편심이 빠져 있으면 실기 tool center와 preview가 어긋날 수 있다.
- 가장 안전한 방식은 실기 치수 또는 CAD를 확보해 `Adapter` 노드를 명시적으로 추가하고, calibration된 `TcpFrame`으로 최종 TCP를 잠그는 것이다.

### 현재 구조 (2026-04-03 확정)

```text
wrist3_link
  └── ToolMount                          (identity)
       └── PGEA_100_40                   [FR5EndEffectorAttachment]
            ├── VisualRoot
            │    └── PGEA-100-40_Model   (scale 0.001, authored offset)
            │         ├── body           (본체, 고정)
            │         ├── finger_left    (좌 핑거, X축 + 이동)
            │         └── finger_right   (우 핑거, X축 - 이동)
            └── TcpFrame
                 └── TcpMarker
```

- `ToolMount`는 identity 유지
- `PGEA-100-40_Model` 하위에 3파트 STL이 개별 GameObject로 배치
- `finger_left` / `finger_right`는 `FR5EndEffectorAttachment`의 `fingerLeft` / `fingerRight` 참조로 연결
- 개폐 시 모델 공간 X축으로 ±20mm 이동 (스트로크 40mm, 모델 공간 mm 단위)
- `SetGripperOpen(float ratio)` API로 런타임 제어 가능 (ROS/SDK 연동 확장점)

### 향후 권장 구조 (Adapter 추가 시)

```text
wrist3_link
  └── ToolMount               (identity)
       └── Adapter            (실기 검은 추가 부품 / 브라켓)
            └── PGEA_100_40   [FR5EndEffectorAttachment]
                 ├── VisualRoot
                 │    └── PGEA-100-40_Model
                 │         ├── body
                 │         ├── finger_left
                 │         └── finger_right
                 └── TcpFrame
```

- `Adapter`는 실기 검은 부품 CAD/치수 확보 후 추가 (ADR-009)

### 외부 참고

- ROS TCP Connector README: <https://github.com/Unity-Technologies/ROS-TCP-Connector>
- Visualizations README: <https://github.com/Unity-Technologies/ROS-TCP-Connector/blob/main/com.unity.robotics.visualizations/Documentation~/README.md>
- Unity Robotics Hub Pick-and-Place: <https://github.com/Unity-Technologies/Unity-Robotics-Hub/blob/main/tutorials/pick_and_place/README.md>
- URDF Importer README: <https://github.com/Unity-Technologies/URDF-Importer>
- Unity Blend Shapes Manual: <https://docs.unity3d.com/2021.1/Documentation/Manual/BlendShapes.html>

## 현재 의미 (2026-04-03 업데이트)

- 지금 저장된 값은 "팀이 공통으로 보는 시각화/프리뷰 기준선"이다
- `FAIRINO_FR5_Control_PGEA10040.prefab`가 장착값 SSOT이고, preview는 이 값을 자동 복사한다
- **그리퍼 메쉬가 3파트(body + finger_left + finger_right)로 분리**되어 개폐 시각화가 즉시 가능하다
- `SetGripperOpen(float)` API가 ROS/SDK 연동 확장점으로 준비되어 있다
- TCP 좌표계는 calibration 전까지 확정이 아니다 (`TcpCalibrated = false`, 노란색 marker)
- ROS 관절 동기화 시 그리퍼는 Transform 계층으로 자동 추종한다
- 즉, 현재 repo는 `3파트 mesh + visual alignment + gripper open/close + FK TCP offset 준비 + ROS 관절 연동 + shared TCP editing workflow`를 책임진다
- 실기 연동 시 필요한 것: `/gripper_command` 토픽 구독 또는 SDK `GetGripperState` 호출 → `SetGripperOpen` 연결
