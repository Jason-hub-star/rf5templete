---
name: end-effector-install
description: "FR5에 엔드이펙터(PGEA-100-40) 설치 및 unityctl 검증 — end effector install, gripper attach, 그리퍼 장착, 엔드이펙터 설치, TCP 검증"
---

## Trigger
사용자가 "엔드이펙터 설치", "그리퍼 장착", "end effector", "PGEA", "TCP 검증" 관련 요청을 하거나, FR5에 새 엔드이펙터를 붙이는 작업을 할 때 활성화.

## Input Context
- 대상 엔드이펙터 ID (기본: `PGEA_100_40`)
- STL 소스 파일 경로
- 대상 로봇 prefab (기본: `FAIRINO_FR5_Control`, `FAIRINO_FR5`)

## Read First
1. `docs/END-EFFECTOR-PGEA10040.md` — 엔드이펙터 정책 및 저장소 배치 규약
2. `Assets/Scripts/Visualization/FR5EndEffectorAttachment.cs` — attachment 컴포넌트 SSOT
3. `Assets/Editor/FR5EndEffectorSetupTool.cs` — 설치 도구 (메뉴 실행)
4. `Assets/Editor/FR5EndEffectorAttachmentEditor.cs` — Inspector TCP 조정 UI
5. `Assets/Scripts/App/FR5TemplateSlimManifest.cs` — 경로/패키징 매니페스트
6. `Assets/Scripts/App/FR5TemplateMinimalController.cs` — 런타임 prefab 로드 fallback 로직

### robotapp2 참조 문서 (유기적 연결 기준)
7. `robotapp2/docs/ref/product/robotcontrol-fr5-v2/fr5-gripper-tcp-calibration-spec.md` — TCP SSOT 정의
8. `robotapp2/docs/ref/product/robotcontrol-fr5-v2/fr5-command-ssot.md` — Tool/TCP Lock 규칙

## Do

### Phase 1: 파일 존재 검증
1. STL 소스 존재 확인: `Assets/Runtime/EndEffectors/PGEA_100_40/Source/PGEA-100-40.stl`
2. 엔드이펙터 prefab 존재 확인: `Assets/Runtime/Resources/EndEffectors/PGEA_100_40.prefab`
3. 장착 variant prefab 확인:
   - `Assets/Runtime/Resources/Robots/FAIRINO_FR5_Control_PGEA10040.prefab`
   - `Assets/Runtime/Resources/Robots/FAIRINO_FR5_PGEA10040.prefab`
4. 에디터/스크립트 파일 확인: `FR5EndEffectorSetupTool.cs`, `FR5EndEffectorAttachmentEditor.cs`, `FR5EndEffectorAttachment.cs`

### Phase 2: unityctl 연결 및 에디터 검증
1. `unityctl editor instances --json` — Unity 인스턴스 확인
2. `unityctl editor select --project .` — 에디터 연결
3. `unityctl ping --project . --json` — 연결 확인
4. `unityctl console get-entries --project . --json` — 컴파일 에러 0건 확인
5. `unityctl asset get-info --project . --path <prefab경로> --json` — 각 prefab 에셋 확인

### Phase 3: Play 모드 계층구조 검증
1. `unityctl play start --project . --json`
2. `unityctl scene hierarchy --project . --json` — 전체 계층 추출
3. 아래 계층구조 필수 확인:
   ```
   wrist3_link
     └── ToolMount
          └── PGEA_100_40              [FR5EndEffectorAttachment]
               ├── VisualRoot
               │    └── PGEA-100-40_Model
               │         ├── PGEA-100-40_0  [MeshFilter, MeshRenderer]
               │         └── PGEA-100-40_1  [MeshFilter, MeshRenderer]
               └── TcpFrame
                    └── TcpMarker       [MeshFilter, MeshRenderer]
   ```
4. 모든 노드가 `activeSelf: true` 확인

### Phase 4: 컴포넌트 속성 검증
1. `unityctl gameobject find --project . --name "PGEA_100_40" --json` — 오브젝트 찾기
2. `unityctl gameobject get --project . --id <id> --json` — 컴포넌트 ID 추출
3. `unityctl component get --project . --component-id <id> --full --json` — 속성 검증:
   - `attachmentId` = "PGEA_100_40"
   - `visualRoot` = "VisualRoot" (null 아님)
   - `tcpFrame` = "TcpFrame" (null 아님)
   - `baseFrame` = "base_link" (null 아님 — 로봇 기본 프레임 참조)

### Phase 5: 방향 규칙 검증
1. ToolMount의 Transform 확인: `m_LocalRotation`이 identity `{x:0, y:0, z:0, w:1}` 이어야 함
2. PGEA_100_40 인스턴스의 로컬 Transform 확인:
   - rotation: `Quaternion(0, 0, -0.71705276, 0.69701904)` ≈ Z -91.6°
   - position: `(0.004, 0.1699, 0.0324)`
   - 이 값은 시각적 정렬 값이며 STL 원점 보정용
3. 그리퍼 jaw side가 로봇 바깥+아래 방향을 향하는지 스크린샷으로 확인
4. mount side가 wrist3 flange에 붙어있는지 확인
5. 시각적 정렬 값 ≠ TCP 좌표계임을 인지 — calibration 전까지 TcpFrame은 확정이 아님

### Phase 6: 스크린샷 시각 검증
1. `unityctl screenshot capture --project . --view scene --output "evidence/verify-endeffector-scene.png"` — Scene view
2. Play 모드에서 `unityctl screenshot capture --project . --view game --output "evidence/verify-endeffector-game.png"` — Game view
3. 스크린샷에서 확인:
   - 로봇 wrist3 끝에 그리퍼 메쉬가 보이는가
   - TcpMarker(초록 구)가 보이는가
   - 관절 조작 시 그리퍼가 함께 따라가는가

### Phase 7: 유기적 연결 검증 (robotapp2 기준)
1. `FR5TemplateMinimalController`가 attached variant를 우선 로드하는 fallback 존재 확인
2. `FR5TemplateSlimManifest`에 엔드이펙터 관련 경로 모두 선언 확인
3. TCP 기준점 문서 확인: `TcpFrame`이 "그리퍼 사이 정중앙 공중점" 기준으로 되어있는지
4. 아래 "유기적 연결" 미완료 항목 기록:
   - TCP 읽기 명령 (`GetActualTCPPose`, `GetCurToolCoord`) 미구현 → 실기 연동 시 필요
   - 좌표 문맥 저장 (`FairinoCoordContext`) 미구현 → teaching 기능 추가 시 필요
   - Preview 동기화 (ghost target의 TCP offset 반영) 미구현 → preview 기능 추가 시 필요
   - Tool coordinate 검증 ("must match" 규칙) 미구현 → live 운용 시 필요

### Phase 8: 검증 결과 문서 정리
1. `unityctl play stop --project . --json` — Play 모드 종료
2. 검증 결과를 테이블로 정리
3. evidence 폴더에 스크린샷 보관

## Do Not
1. STL/STEP 원점을 TCP로 간주하지 않는다
2. 시각적 정렬 값을 TCP 좌표계 값으로 혼동하지 않는다 — authored rotation/position은 메쉬 보정용
3. 에디터 모드 씬에서 엔드이펙터를 찾지 않는다 — 런타임 인스턴스화 방식이므로 Play 모드에서 확인해야 한다
3. `ExternalStlPath`/`ExternalStepPath`의 하드코딩 경로를 팀 배포 기준으로 쓰지 않는다
4. prefab hierarchy에 "붙어있다"만으로 "유기적 연결" 완료라고 판단하지 않는다
5. ToolMount 방향을 임의로 변경하지 않는다 — `DefaultAttachmentRotation` 규칙을 따른다

## Validation
- [ ] STL 소스 파일 존재
- [ ] 엔드이펙터 prefab 존재 (`unityctl asset get-info`)
- [ ] Control/Preview attached variant prefab 존재
- [ ] 컴파일 에러 0건 (`unityctl console get-entries`)
- [ ] Play 모드 계층구조 일치 (wrist3→ToolMount→PGEA_100_40→{VisualRoot, TcpFrame})
- [ ] 모든 노드 active
- [ ] FR5EndEffectorAttachment 속성 검증 (attachmentId, visualRoot, tcpFrame, baseFrame 비null)
- [ ] ToolMount identity 확인 + PGEA_100_40 인스턴스 authored transform 확인
- [ ] Scene/Game 스크린샷에서 그리퍼 메쉬 확인
- [ ] 유기적 연결 미완료 항목 기록됨

## Output Template
```
[end-effector-install 검증 완료]
- 대상: {attachment_id}
- 로봇: FR5
- prefab 존재: {ok/fail}
- 계층구조: {ok/fail}
- 컴포넌트 참조: {ok/fail}
  - attachmentId: {value}
  - visualRoot: {connected/null}
  - tcpFrame: {connected/null}
  - baseFrame: {connected/null}
- ToolMount: identity {ok/fail}
- PGEA_100_40 authored transform: {ok/fail} (Z {actual_degrees}도)
- 좌표계 상태: visual-alignment (calibration 전)
- 컴파일 에러: {count}건
- 스크린샷: evidence/{filename}
- 유기적 연결 수준: {visual-only / tcp-preview / full-live}
- 미완료 항목: {list}
```
