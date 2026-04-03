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

### ADR-004: FBX를 엔드이펙터 메쉬 소스로 우선 사용한다 — 2026-04-03
- **결정**: STL 대신 FBX를 1순위 소스로 사용. STL은 fallback
- **이유**: FBX 원점이 메쉬 중심 근처(편심 없음), Unity importer가 스케일 자동 처리, 메쉬 통합(1개)
- **근거 데이터**: STL 편심 54.75mm vs FBX 편심 0.028mm

### ADR-005: MCP for Unity를 제거한다 — 2026-04-03
- **결정**: Assets/MCPForUnity/ 삭제
- **이유**: 50,000줄+ 코드가 도메인 리로드를 5분+ 지연. 프로젝트에 불필요
- **보호**: ROS TCP Connector + RosJointStateSubscriber는 별개이며 유지 (실기 연동 성공)

### ADR-006: ROS 관절 동기화 시 엔드이펙터는 별도 연동 불필요 — 2026-04-03
- **결정**: 그리퍼는 Transform 자식 계층으로 자동 추종, ROS 별도 연동 없음
- **이유**: wrist3_link > ToolMount > PGEA_100_40 — 관절 회전 시 자동 따라감
- **예외**: 그리퍼 개폐는 별도 토픽(/gripper_command) 필요 → 현재 범위 밖

## 미결 결정

### 그리퍼 개폐 시각화 방식
- **상태**: 미착수
- **옵션**: A) prismatic joint로 URDF 확장 B) 애니메이션 클립 C) 범위 밖으로 유지
- **의존**: STL/FBX가 베이스+핑거로 분리되어 있는지 확인 필요
