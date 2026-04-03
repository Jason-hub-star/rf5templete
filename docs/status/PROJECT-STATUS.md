# PROJECT-STATUS

## 현재 Phase: 그리퍼 3파트 개폐 시각화 + ROS 관절 연동

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
- [x] FBX 소스 비교 검토 완료 — 런타임 기준선으로는 채택하지 않음 — 2026-04-03
- [x] STL 기준 엔드이펙터 리소스/robot variant 복구 — 2026-04-03
- [x] STL authored X축 1mm 미세보정 적용 (`0.004 -> 0.003`) — 2026-04-03
- [x] Play 모드에서 STL 장착 계층/transform/game view 검증 — 2026-04-03
- [x] Enter Play Mode Settings 활성화 (도메인 리로드 건너뛰기) — 2026-04-03
- [x] RobotKinematicsFacade TCP offset 합성 준비 (SetTcpOffset) — 2026-04-03
- [x] TcpFrame calibration 상태 시각화 (노란색 gizmo + Inspector 경고) — 2026-04-03
- [x] 에디터 전용 FR5 preview 표시 (Play 전 Scene view 가시성 확보) — 2026-04-03
- [x] 엔드이펙터 스킬 문서 작성 (end-effector-install) — 2026-04-02
- [x] 아키텍처 결정 기록 (ADR-001~003) — 2026-04-03
- [x] doc-framework 스킬 설치 — 2026-04-03
- [x] authored/TCP/그리퍼 확장성 조사 결과 문서화 — 2026-04-03
- [x] Control prefab 기준 엔드이펙터 SSOT 정리 + Preview 자동 동기화 보강 — 2026-04-03
- [x] FreeCAD에서 STEP → 3파트 STL 분리 (body + finger_left + finger_right) — 2026-04-03
- [x] 3파트 STL 프로젝트 임포트 + 대칭 검증 (4,164 tri 동일, X축 미러) — 2026-04-03
- [x] FR5EndEffectorAttachment에 그리퍼 개폐 제어 추가 (SetGripperOpen, Inspector 슬라이더) — 2026-04-03
- [x] FR5EndEffectorSetupTool 3파트 빌드 로직 교체 — 2026-04-03
- [x] 셀프 리뷰 — 단위 오류(mm↔m), 이동축(Y→X), 기준위치 오염 방지 수정 — 2026-04-03
- [x] FreeCAD 그리퍼 모델링 가이드 문서 작성 (docs/GRIPPER-FREECAD-MODELING.md) — 2026-04-03

### 진행 중

- [ ] Unity에서 Install 메뉴 실행 → 3파트 prefab 재빌드 + Play 모드 개폐 검증
- [ ] 커밋 + 푸시

### 미착수

- [ ] TCP calibration (실기 pendant/SDK 보정 후 TcpFrame 값 확정)
- [ ] `/gripper_command` ROS 토픽 구독 → `SetGripperOpen` 연결
- [ ] SDK `GetGripperState` → `SetGripperOpen` 연결 (LiveFairinoClient 확장)
- [ ] robotapp2 이관 (SetToolCoord, FairinoCoordContext, WaypointStore)

## 성공 패턴

### STL 3파트 런타임 기준선

| | 3파트 STL (현재) | 단일 STL (이전) | FBX |
|---|---|---|---|
| 현재 역할 | **Unity 런타임 기준선** | 레거시 (교체됨) | 비교용 참고 자산 |
| 파트 구성 | body + finger_left + finger_right | base + fingers(합쳐짐) | 통합 메쉬 |
| 개폐 시각화 | **즉시 가능** (finger Transform 제어) | 불가 (합쳐진 메쉬) | 추가 분해 필요 |
| 소스 | FreeCAD에서 STEP → 파트별 STL 내보내기 | 단일 STL 직접 임포트 | CAD 변환 |
| 대칭 검증 | 좌우 4,164 tri 동일, X축 미러 확인 | 미분리 | 미검증 |

**결론**: 엔드이펙터 런타임 메쉬는 FreeCAD에서 분리한 3파트 STL을 사용한다. Inspector에서 `Gripper Open Ratio` 슬라이더로 개폐를 즉시 확인할 수 있다. `SetGripperOpen(float)` API로 ROS/SDK 연동 확장 가능.

### authored 정렬 패턴 (STL 사용 시)

STL 좌표계가 비표준일 때:
1. `FAIRINO_FR5_Control_PGEA10040.prefab`에서 수동으로 방향/위치 조정
2. Control prefab 저장
3. preview/editor preview는 Control 기준 포즈를 복사
4. Setup Tool은 재생성 시 기존 Control attachment 값을 우선 보존하고, 없을 때만 bootstrap 값 사용

### unityctl 검증 패턴

1. `unityctl editor select` → `ping` — 연결 확인
2. `unityctl play start` → `scene hierarchy` — Play 모드 계층구조 확인
3. `unityctl gameobject find` → `gameobject get` → `component get` — 컴포넌트 속성 검증
4. `unityctl screenshot capture --view game` — 시각적 검증
5. `unityctl console get-count` + `Editor.log` grep — 에러/경고 원인 확인

### Enter Play Mode Settings

도메인 리로드가 느리거나 무한일 때:
- `Edit > Project Settings > Editor > Enter Play Mode Settings` 활성화
- Domain Reload + Scene Reload 건너뛰기 (옵션 값 3)
- unityctl: `project-settings set --property m_EnterPlayModeOptionsEnabled --value true`
- unityctl: `project-settings set --property m_EnterPlayModeOptions --value 3`
