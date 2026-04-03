# PGEA-100-40 그리퍼 개폐 모델링 — FreeCAD 작업 가이드

> **작성일**: 2026-04-03
> **대상 그리퍼**: DH-Robotics PGEA-100-40 (평행 전동 그리퍼)
> **목적**: 그리퍼 조(jaw) 개폐를 FreeCAD에서 모델링하고 Unity로 내보내기

---

## 1. PGEA-100-40 그리퍼 사양 요약

| 항목 | 값 |
|------|-----|
| 그리핑 포스 | 30~100 N (per jaw) |
| 스트로크 | 40 mm (각 조 20mm씩 이동) |
| 개폐 시간 | 0.15초 |
| 반복 정밀도 | ±0.02 mm |
| 무게 | 0.6 kg |
| 권장 워크피스 중량 | 2 kg |
| 인터페이스 | Modbus RTU, Digital I/O, TCP/IP, USB2.0, CAN2.0A 등 |

---

## 2. CAD 원본 확보

| 소스 | URL | 포맷 |
|------|-----|------|
| DH-Robotics 공식 | https://en.dh-robotics.com/service/download | STEP, 2D |
| TraceParts | https://www.traceparts.com/goto?CatalogPath=DH_ROBOTICS_28427589 | STEP, STL, IGES 등 |
| 프로젝트 내 보관 | `archive/EndEffectors/PGEA_100_40/CAD/PGEA-100-40-W-F_V1.0_3D_20241226.STEP` | STEP |

> **이미 STEP 파일이 프로젝트 archive에 있으므로 바로 FreeCAD에서 열 수 있음**

---

## 3. FreeCAD Assembly 워크벤치 선택

### 권장: Built-in Assembly Workbench (FreeCAD 1.0+)

FreeCAD 1.0부터 내장된 Ondsel Solver 기반 Assembly 워크벤치. 가장 현대적이고 활발히 개발 중.

| 워크벤치 | 특징 | 그리퍼 적합도 |
|-----------|------|---------------|
| **Built-in Assembly** (1.0+) | Slider/Revolute Joint, 드래그 시뮬레이션 | **최적** — Slider Joint로 조 이동 구속 |
| **Assembly4** | Expression Engine + Master Sketch, 애니메이션 강점 | 애니메이션 필요 시 선택 |
| **A2plus** | 레거시, 평면/축 구속 기반 | 비추천 (내장 WB와 비호환) |

### 그리퍼에 유용한 Joint 타입

| Joint | 설명 | 용도 |
|-------|------|------|
| **Slider (Prismatic)** | 단일 축 선형 이동 | **조 개폐 이동 — 핵심** |
| Fixed | 완전 고정 | 본체 고정 |
| Gears | 기어 커플링 | 양쪽 조 동기화 |
| Parallel | Z축 평행 유지 | 평행 조 구속 보조 |

---

## 4. 모델링 워크플로우

### Step 1: STEP 임포트 및 파트 분리

```
FreeCAD 실행 → File → Open → PGEA-100-40.STEP 선택
```

STEP 파일 임포트 후 트리 뷰에서 파트를 확인하고, **3개의 독립 Body로 분리**:

| 파트 | 설명 | 움직임 |
|------|------|--------|
| `gripper_body` | 메인 하우징/본체 | 고정 (Ground) |
| `jaw_left` | 왼쪽 조 (핑거) | X축 +방향 슬라이딩 |
| `jaw_right` | 오른쪽 조 (핑거) | X축 -방향 슬라이딩 |

> **현재 프로젝트 STL 구조 참고**: `PGEA-100-40_0`(base) + `PGEA-100-40_1`(fingers)로 이미 분할됨. STEP에서도 유사한 구조일 가능성 높음.

### Step 2: Assembly 생성 및 Joint 설정

```
1. Assembly WB로 전환
2. Assembly 도구 아이콘으로 Assembly 객체 생성
3. 각 파트(Body)를 트리에서 Assembly로 드래그
4. gripper_body 선택 → "Toggle Grounded" (고정)
5. jaw_left에 Slider Joint 적용:
   - First Object: jaw_left의 슬라이딩 면
   - Second Object: gripper_body의 가이드 면
   - Min = 0mm, Max = 20mm
6. jaw_right에도 동일하게 Slider Joint 적용 (반대 방향)
7. (선택) Gears Joint로 양쪽 조 동기화 — 하나가 +10mm면 다른 쪽 -10mm
```

### Step 3: 동작 확인

```
1. Assembly 더블클릭으로 활성화
2. jaw를 드래그하여 슬라이딩 동작 확인
3. 스트로크 범위(0~40mm) 내에서 정상 이동 확인
```

---

## 5. 대안: Assembly4로 애니메이션 포함 모델링

파라메트릭 애니메이션이 필요한 경우 Assembly4 워크벤치 사용:

### Variables 설정
```
Variables 객체 생성:
  - Jaw_Opening: Float, 범위 0.00 ~ 40.00 mm
```

### Master Sketch 구성
```
1. Sketcher WB에서 Master Sketch 생성
2. 원점에서 좌우로 Jaw_Opening/2 거리에 점 배치
3. 구속조건에 Variables.Jaw_Opening Expression 연결
4. 각 점에 LCS(Local Coordinate System) 부착
```

### 애니메이션 실행
```
1. Assembly4 → Animate Assembly
2. Variable: Jaw_Opening
3. Range: 0 ~ 40
4. Steps: 20
5. ▶ Play로 개폐 애니메이션 확인
```

---

## 6. Spreadsheet 기반 파라메트릭 설계 (선택)

FreeCAD Spreadsheet WB로 모든 치수를 중앙 관리:

```
Spreadsheet 셀 예시:
  A1: jaw_stroke   = 40    (mm, 전체 스트로크)
  A2: body_width   = 64    (mm, 본체 폭)
  A3: body_height  = 78.5  (mm, 본체 높이)
  A4: jaw_width    = 10    (mm, 조 폭)

Sketch 구속조건에서 참조:
  Spreadsheet.jaw_stroke / 2  → 각 조의 최대 이동거리
```

---

## 7. Unity 내보내기 파이프라인

### 파이프라인 A: 직접 내보내기 (단순, 권장)

```
FreeCAD → 각 파트별 개별 STL/OBJ → Unity 임포트
```

**내보내기 순서:**
1. Tree View에서 `gripper_body` 선택
2. File → Export → `gripper_body.stl`
3. `jaw_left` 선택 → Export → `jaw_left.stl`
4. `jaw_right` 선택 → Export → `jaw_right.stl`

**메시 품질 조정:**
```
Edit → Preferences → Import-Export → Mesh Formats
  Maximum mesh deviation: 0.1 (낮을수록 고품질, 파일 커짐)
```

> **주의**: STL/OBJ는 하나의 파일에 여러 오브젝트를 담아도 Unity에서 단일 메시로 인식됨. **반드시 파트별 별도 파일로 내보내기**.

### 파이프라인 B: Blender 경유 (고품질)

```
FreeCAD → STEP/OBJ → Blender → FBX → Unity
```

1. FreeCAD에서 각 파트를 OBJ로 내보내기 (또는 전체 STEP)
2. Blender에서 임포트
3. Decimate Modifier로 폴리곤 최적화
4. 머테리얼/UV 설정
5. FBX로 내보내기 (Unity 최적 포맷)
6. Unity 임포트

**Blender 경유 장점:**
- FBX = Unity 최적 포맷 (공식 권장)
- 계층 구조(hierarchy) 유지
- 메시 최적화 + 머테리얼 설정 가능

### 포맷 비교

| 포맷 | FreeCAD 직접 | Unity 호환 | 멀티 오브젝트 | 추천도 |
|------|:---:|:---:|:---:|:---:|
| STL | O | O | X (파트별 분리 필요) | **현재 프로젝트 기준선** |
| OBJ | O | O | 제한적 | 머테리얼 필요 시 |
| DAE | O | O | O | 멀티 오브젝트 유지 |
| FBX | X (Blender 경유) | O (최적) | O | **최종 목표** |

### Unity 임포트 시 주의사항

- **단위**: FreeCAD = mm, Unity = m → **스케일 팩터 0.001 적용**
- **각 파트를 별도 파일로 내보내야** Unity에서 개별 Transform 제어 가능
- 현재 프로젝트에서는 STL 기준선 사용 중 (ADR-004)

---

## 8. Unity에서 개폐 구현 연결

내보낸 메시를 Unity에서 사용할 때의 구조:

```
PGEA_100_40 [FR5EndEffectorAttachment]
  ├── VisualRoot
  │    ├── gripper_body (고정)
  │    ├── jaw_left     (Transform.localPosition.x로 이동)
  │    └── jaw_right    (Transform.localPosition.x로 반대 이동)
  └── TcpFrame
```

**개폐 스크립트 개념:**
```csharp
// jawOpening: 0.0 (완전 닫힘) ~ 1.0 (완전 열림)
float stroke = 0.040f; // 40mm = 0.04m
jawLeft.localPosition  = new Vector3(+jawOpening * stroke / 2, 0, 0);
jawRight.localPosition = new Vector3(-jawOpening * stroke / 2, 0, 0);
```

> **현재 상태**: 프로젝트의 STL 메시가 `PGEA-100-40_0`(base) + `PGEA-100-40_1`(fingers)로 이미 분할되어 있어 개폐 구현에 유리 (ADR 참고).

---

## 9. 참고 리소스

### 공식 문서
- [FreeCAD Assembly Workbench 위키](https://wiki.freecad.org/Assembly_Workbench)
- [FreeCAD Assembly4 위키](https://wiki.freecad.org/Assembly4_Workbench)
- [FreeCAD Export to STL/OBJ 가이드](https://wiki.freecadweb.org/Export_to_STL_or_OBJ)

### 튜토리얼
- [DigiKey: FreeCAD Assembly Tutorial (2025)](https://www.digikey.com/en/maker/tutorials/2025/intro-to-freecad-part-9-assembly-tutorial)
- [FreeCAD Blog: Assembly WB 입문](https://blog.freecad.org/2024/09/30/tutorial-getting-started-with-the-assembly-workbench/)
- [Assembly4 Tutorial 2: Kinematic Mechanism](https://github.com/leoheck/FreeCAD_Assembly4.1/blob/main/docs/Tutorial2/TUTORIAL_2.md)
- [FreeCAD Blog: Spreadsheet 파라메트릭 설계 (2025)](https://blog.freecad.org/2025/04/08/tutorialgetting-started-with-spreadsheets-and-parametric-design/)

### 동영상
- [FreeCAD Mechanical Animation Series](https://www.classcentral.com/course/youtube-freecad-mechanical-animation-series-90511)
- [FreeCAD Spring/Bolt Animation in Assembly4](https://www.classcentral.com/course/youtube-freecad-spring-and-bolt-animation-in-assembly4-90510)

### CAD 모델
- [DH-Robotics 공식 다운로드](https://en.dh-robotics.com/service/download)
- [TraceParts DH-Robotics 카탈로그](https://www.traceparts.com/goto?CatalogPath=DH_ROBOTICS_28427589)
- [GrabCAD Gripper 모델](https://grabcad.com/library/tag/gripper)

### 내보내기 참고
- [CAD to Blender using FreeCAD](https://cgian.com/2024/01/stp-to-blender-uing-freecad)
- [3D CAD Models into Unity (CAD Exchanger)](https://cadexchanger.medium.com/how-to-import-3d-cad-models-into-unity-d922317ca040)

---

## 10. 현재 프로젝트와의 관계

| 항목 | 현재 상태 | FreeCAD 작업 후 |
|------|-----------|-----------------|
| 메시 소스 | 단일 STL (2파트 분할) | 3파트 분리 STL/FBX |
| 개폐 시각화 | 미구현 (ADR 미착수) | jaw Transform 제어로 구현 가능 |
| TCP 좌표 | TcpFrame placeholder | 변경 없음 (calibration 별도) |
| ROS 토픽 | 관절만 구독 | 그리퍼 상태 토픽 추가 구독 필요 |

### 다음 단계 (제안)

1. FreeCAD에서 STEP 파일 열기 → 파트 분리 확인
2. jaw 파트를 개별 STL로 내보내기
3. Unity에서 기존 단일 STL을 3파트 구조로 교체
4. `FR5EndEffectorAttachment`에 개폐 파라미터 추가
5. ROS gripper_state 토픽 구독 → jawOpening 값 반영
