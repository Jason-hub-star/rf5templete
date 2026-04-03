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

### 런타임 시각 자산

- `Assets/Runtime/EndEffectors/PGEA_100_40/Source/PGEA-100-40.stl`
- `Assets/Runtime/Resources/EndEffectors/PGEA_100_40.prefab`

### 원본 CAD 보관본

- `archive/EndEffectors/PGEA_100_40/CAD/PGEA-100-40-W-F_V1.0_3D_20241226.STEP`

### 팀 공용 즉시 사용 prefab

- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control_PGEA10040.prefab`
- `Assets/Runtime/Resources/Robots/FAIRINO_FR5_PGEA10040.prefab`

## 팀 우선순위

### 1. STL을 Unity visual source로 고정

- Unity에서 실제로 붙는 것은 `STL`
- `STEP`은 치수/원점 검토용
- 팀원은 `Resources/EndEffectors/PGEA_100_40.prefab`만 직접 참조하면 된다

### 2. FR5 control/preview variant를 기본 사용

- 팀원이 직접 wrist3에 붙이지 않게 한다
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
- 현재 authored 값: `rotation Z -91.6°`, `position (0.004, 0.1699, 0.0324)`
- 이 값은 STL 메쉬 원점과 wrist3 좌표계의 차이를 보정하기 위한 **시각적 정렬 값**이다
- 실기 TCP calibration 값과는 별개이며, calibration 후 `TcpFrame`을 다시 잠궈야 한다

### 4. 조정 기준은 Tool/Base/World 모두 허용

- `FR5EndEffectorAttachment` inspector에서 `Tool / Base / World` 기준으로 TCP를 nudge 할 수 있다
- 첫 배치는 대략 맞추고, 실제 calibration 값은 나중에 `TcpFrame`으로 옮긴다

## 사용 방법

1. Unity에서 `RobotTemplate/End Effector/Install PGEA-100-40 On FR5` 실행
2. 데모 씬을 열면 FR5 attached variant가 우선 로드된다
3. Hierarchy에서 `PGEA_100_40` 오브젝트의 `FR5EndEffectorAttachment`를 선택한다
4. Inspector에서 `Tool / Base / World` 기준으로 `TcpFrame`을 미세 조정한다
5. Scene/Game view에서 TCP marker와 gizmo를 보며 위치를 맞춘다
6. 기본 방향이 틀리면 setup tool을 다시 실행해 attached prefab을 재생성한다

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
| Preview 동기화 | ghost target의 TCP offset 반영 | preview 기능 추가 시 |
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
- **원인**: Setup Tool exec가 불안정 (STL 재임포트 충돌), prefab 값이 적용 안 되는 경우 있음
- **현재 상태**: authored 값(`Z -91.6°, pos (0.004, 0.1699, 0.0324)`)이 코드에 반영됨
- **다음 단계**: Unity가 안정된 후 Install 메뉴로 prefab 재생성 → Play에서 확인

## 현재 의미

- 지금 저장된 값은 "팀이 공통으로 보는 시각화/프리뷰 기준선"이다
- 시각적 정렬은 authored 값으로 확정되었으나, Unity 도메인 리로드 문제로 검증 미완
- TCP 좌표계는 calibration 전까지 확정이 아니다
- `TcpCalibrated = false` 상태에서 TCP marker는 노란색으로 표시된다
- ROS 관절 동기화 시 그리퍼는 Transform 계층으로 자동 추종한다
- 즉, 현재 repo는 `mesh import + visual alignment + FK TCP offset 준비 + ROS 관절 연동 + shared TCP editing workflow`를 책임진다
