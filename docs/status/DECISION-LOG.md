# DECISION-LOG

## 해결된 결정

### ADR-001: URDF에 gripper fixed link를 추가하지 않는다 — 2026-04-03
- **결정**: wrist3_link에서 URDF 종료, 엔드이펙터는 prefab hierarchy로 관리
- **이유**: URDF Importer 부수효과(ArticulationBody 재구성, mesh 재설정) 회피
- **대안 검토**: `<joint type="fixed">` + `<link name="gripper_base_link">` → importer 의존성 높음

### ADR-002: SetToolCoord를 이 템플릿에서 호출하지 않는다 — 2026-04-03
- **결정**: 슬림 템플릿은 read-only, 쓰기 명령 없음
- **이유**: SetToolCoord는 컨트롤러 상태 변경 명령, 템플릿 범위 초과
- **후속**: robotapp2에서 구현

### ADR-003: 시각적 정렬과 TCP 좌표계를 분리한다 — 2026-04-03
- **결정**: PGEA_100_40 authored transform = 시각적 정렬, TcpFrame = TCP 좌표계
- **이유**: STL/FBX 메쉬 원점 ≠ 실제 TCP. calibration 전까지 TCP는 미확정
- **후속**: calibration 시 TcpFrame 값 덮어쓰기 + tcpCalibrated = true

### ADR-004: Unity 런타임 엔드이펙터는 STL authored baseline을 유지한다 — 2026-04-03
- **결정**: 런타임 장착 prefab과 FR5 attached variant는 STL을 기준으로 유지한다. FBX는 원점 비교/향후 자산 개선용 참고본으로만 둔다
- **이유**: 현재 팀이 검증한 장착감과 그리퍼 체감 크기는 STL 배치가 가장 안정적이었다. 2026-04-03에 X축을 1mm 줄인 authored 보정으로 미세 편심만 완화했다
- **근거 데이터**: Play 모드에서 `ToolMount` identity, `PGEA_100_40` authored transform `(0.003, 0.1676, 0.031) / Z -91.6°`, STL 분할 메쉬 계층, Game view 부착 상태를 재확인

### ADR-005: MCP for Unity를 제거한다 — 2026-04-03
- **결정**: Assets/MCPForUnity/ 삭제
- **이유**: 50,000줄+ 코드가 도메인 리로드를 5분+ 지연. 프로젝트에 불필요
- **보호**: ROS TCP Connector + RosJointStateSubscriber는 별개이며 유지 (실기 연동 성공)

### ADR-006: ROS 관절 동기화 시 엔드이펙터는 별도 연동 불필요 — 2026-04-03
- **결정**: 그리퍼는 Transform 자식 계층으로 자동 추종, ROS 별도 연동 없음
- **이유**: wrist3_link > ToolMount > PGEA_100_40 — 관절 회전 시 자동 따라감
- **예외**: 그리퍼 개폐는 별도 토픽(/gripper_command) 필요 → 현재 범위 밖

### ADR-007: 엔드이펙터 장착값 SSOT는 Control variant다 — 2026-04-03
- **결정**: `FAIRINO_FR5_Control_PGEA10040.prefab`를 장착값 기준점으로 두고, preview variant와 editor preview는 Control의 attachment 포즈를 복사한다
- **이유**: 같은 의미의 장착값이 Control/Preview/SetupTool에 중복되면 유지보수 비용이 커지고, Play 전후로 값이 달라 보이는 혼란이 생긴다
- **후속**: Setup Tool은 재생성 시 기존 Control attachment 값을 우선 보존하고, 없는 경우에만 bootstrap 기본값을 사용한다

### ADR-008: 그리퍼 개폐는 FreeCAD 3파트 STL + Transform 제어로 구현한다 — 2026-04-03
- **결정**: URDF prismatic joint나 blend shape 대신, FreeCAD에서 body/finger_left/finger_right로 분리한 STL을 사용하고 `FR5EndEffectorAttachment.SetGripperOpen(float)`로 finger Transform을 직접 이동
- **이유**:
  - URDF에 jaw joint를 추가하면 `FairinoUrdfJointDriver`가 non-root ArticulationBody를 비활성화하는 문제와 충돌
  - `RosJointStateSubscriber`는 arm 6축만 읽으므로 URDF 확장은 런타임 제어 구조 변경을 수반
  - 평행 jaw의 rigid slide는 blend shape보다 Transform 이동이 자연스러움
  - FreeCAD에서 파트별 STL 내보내기 → 좌우 대칭(4,164 tri, 53.8×40.5×16.7mm) 확인
- **이동 축**: 모델 공간 X축 (STL 분석 기반: finger_left +X, finger_right -X 미러)
- **단위**: 모델 공간은 mm 단위 (부모 PGEA-100-40_Model scale=0.001), `StrokeMm = 40f`
- **실기 연동 경로**: `SetGripperOpen(float)` ← ROS `/gripper_command` subscriber 또는 SDK `GetGripperState`

### ADR-009: Adapter 노드는 실기 연동 시 추가한다 — 2026-04-03
- **결정**: 문서 권장 계층(`ToolMount → Adapter → GripperBase → ...`)에서 Adapter 노드는 현재 생략하고, 실기 검은 추가 부품 CAD/치수 확보 후 추가
- **이유**: 현재 visual-only 단계에서 빈 Adapter 노드를 넣으면 계층만 깊어지고 실익 없음. CAD 없이 추정 geometry를 넣으면 calibrated TCP와 불일치 위험
- **후속**: 실기 치수 또는 부품 CAD 확보 시 Adapter 노드 + mesh 추가, TcpFrame 재조정

## 미결 결정

### `/gripper_command` ROS 토픽 구독 방식
- **상태**: 미착수 (API `SetGripperOpen` 준비 완료)
- **옵션**: A) `RosJointStateSubscriber` 확장 B) 별도 `RosGripperSubscriber` 신설 C) robotapp2에서 구현
- **의존**: ROS2 측 그리퍼 토픽 발행 여부, 메시지 타입 결정 (Float32 ratio vs GripperCommand)

### SDK 그리퍼 상태 읽기 방식
- **상태**: 미착수 (API `SetGripperOpen` 준비 완료)
- **옵션**: A) `LiveFairinoClient`에 `GetGripperState` 추가 B) Modbus RTU 직접 통신 C) robotapp2에서 구현
- **의존**: FAIRINO SDK의 그리퍼 상태 읽기 지원 여부, PGEA-100-40 통신 인터페이스 설정
