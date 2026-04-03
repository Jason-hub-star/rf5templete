# PROJECT-STATUS

## 현재 Phase: 엔드이펙터 시각적 장착 + ROS 관절 연동

### 완료 항목

- [x] FR5 슬림 템플릿 초기 프로젝트 구성 — 2026-03-25
- [x] read-only live smoke UI 구현 — 2026-03-25
- [x] ROS TCP Connector + RosJointStateSubscriber 연동 (hyun-su-kim) — 2026-04-02
- [x] 실기기 ROS 관절 동기화 성공 (hyun-su-kim) — 2026-04-02
- [x] MCP for Unity 제거 (무한 리로드 원인) — 2026-04-03
- [x] PGEA-100-40 STL 임포트 + EndEffector prefab 생성 — 2026-04-02
- [x] FR5EndEffectorAttachment 컴포넌트 구현 (TcpFrame, NudgeTcp, Gizmo) — 2026-04-02
- [x] FR5EndEffectorSetupTool Editor 메뉴 구현 — 2026-04-02
- [x] FR5 Control/Preview variant prefab 생성 — 2026-04-02
- [x] ToolMount identity + authored visual alignment 확정 — 2026-04-02
- [x] STL 메쉬 분석 (MeasureGripper.cs) — 편심 원인 파악 — 2026-04-03
- [x] FBX 소스 도입 — 원점 정상, 편심 해소 — 2026-04-03
- [x] FBX 기반 Setup Tool 로직 구현 — 2026-04-03
- [x] Enter Play Mode Settings 활성화 (도메인 리로드 건너뛰기) — 2026-04-03
- [x] RobotKinematicsFacade TCP offset 합성 준비 (SetTcpOffset) — 2026-04-03
- [x] TcpFrame calibration 상태 시각화 (노란색 gizmo + Inspector 경고) — 2026-04-03
- [x] 엔드이펙터 스킬 문서 작성 (end-effector-install) — 2026-04-02
- [x] 아키텍처 결정 기록 (ADR-001~003) — 2026-04-03
- [x] doc-framework 스킬 설치 — 2026-04-03

### 진행 중

- [ ] FBX 기반 그리퍼 시각적 방향 최종 검증 (Play 모드 스크린샷)
- [ ] Preview variant prefab에도 FBX 기반 정렬 적용
- [ ] 커밋 + 푸시

### 미착수

- [ ] TCP calibration (실기 pendant/SDK 보정 후 TcpFrame 값 확정)
- [ ] 그리퍼 개폐 시각화 (prismatic joint — 별도 토픽 필요)
- [ ] robotapp2 이관 (SetToolCoord, FairinoCoordContext, WaypointStore)

## 성공 패턴

### FBX > STL (엔드이펙터 메쉬 소스)

| | STL | FBX |
|---|---|---|
| 원점 | 메쉬에서 325mm 떨어짐 (비표준) | 메쉬 중심 근처 (XY≈0) |
| 편심 | 54.75mm X축 편심 | 거의 없음 |
| 스케일 | mm 단위, 수동 변환 필요 | Unity importer 자동 처리 |
| 메쉬 수 | 2개 분할 | 1개 통합 |
| 자동 정렬 | 실패 (비표준 원점) | 가능 (정상 원점) |

**결론**: 엔드이펙터 메쉬는 FBX를 우선 사용한다. STL은 fallback.

### authored 정렬 패턴 (STL 사용 시)

STL 좌표계가 비표준일 때:
1. Unity Inspector에서 수동으로 방향/위치 조정
2. Prefab 저장
3. prefab YAML에서 값 감지 (`m_Modifications` 섹션)
4. 감지된 값을 코드에 반영

### unityctl 검증 패턴

1. `unityctl editor select` → `ping` — 연결 확인
2. `unityctl play start` → `scene hierarchy` — Play 모드 계층구조 확인
3. `unityctl gameobject find` → `gameobject get` → `component get` — 컴포넌트 속성 검증
4. `unityctl screenshot capture --view game` — 시각적 검증
5. `unityctl console get-entries --filter error` — 에러 확인

### Enter Play Mode Settings

도메인 리로드가 느리거나 무한일 때:
- `Edit > Project Settings > Editor > Enter Play Mode Settings` 활성화
- Domain Reload + Scene Reload 건너뛰기 (옵션 값 3)
- unityctl: `project-settings set --property m_EnterPlayModeOptionsEnabled --value true`
- unityctl: `project-settings set --property m_EnterPlayModeOptions --value 3`
