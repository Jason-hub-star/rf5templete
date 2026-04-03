---
name: doc-framework
description: "문서 관리 프레임워크 — doc update, doc sync, 문서 동기화, 상태 문서, 결정 로그, PROJECT-STATUS"
---

## Trigger
코드 변경 후 문서 업데이트가 필요할 때, `/doc-update` 또는 `/doc-sync` 요청 시, 프로젝트 상태 추적이 필요할 때 활성화.

## Core Principles

1. **Single Source of Truth**: 하나의 사실은 하나의 문서에만 기록. 다른 파일은 링크 또는 완료 표기만.
2. **Hierarchical Documentation**: 3계층 — status(항상 읽음), ref(필요 시), archive(결정 기록)
3. **Change Class Mapping**: 코드 변경 분류에 따라 문서 업데이트 자동 트리거

## Input Context
- 변경된 코드 파일 목록
- 변경 유형 (기능 추가, 버그 수정, 리팩터링, 아키텍처 결정)
- 현재 프로젝트 phase

## Read First
1. `docs/status/PROJECT-STATUS.md` — 현재 phase, 실행 체크리스트
2. `docs/status/DECISION-LOG.md` — 미결/해결 결정 사항
3. 관련 `docs/ref/` 문서

## Do

### /doc-update (문서 수정)
1. 코드 변경을 분류한다 (trivial / feature / architecture / breaking)
2. trivial(주석, 오타, 테스트)이면 문서 업데이트 건너뛴다
3. feature/architecture/breaking이면:
   - `PROJECT-STATUS.md` 체크리스트 갱신 (완료 항목에 날짜 필수)
   - 관련 `ref/` 문서 갱신
   - 아키텍처 결정이면 `DECISION-LOG.md`에 기록
4. 완료 후 14일 지난 항목은 archive 후보로 표시

### /doc-sync (검증만, 편집 없음)
1. 코드 변경 대비 문서 누락 탐지
2. `PROJECT-STATUS.md`와 실제 코드 상태 비교
3. 불일치 항목 리포트 출력
4. 파일 수정하지 않음

### 문서 구조
```
docs/
├── status/
│   ├── PROJECT-STATUS.md    ← 현재 phase, 체크리스트
│   └── DECISION-LOG.md      ← 결정 사항 추적
├── ref/                     ← 참조 문서
└── archive/                 ← 해결된 결정, 완료 항목
```

## Do Not
1. 충돌 시 문서보다 **구현을 먼저 확인**한다 (implementation-first verification)
2. trivial 변경에 문서 업데이트를 하지 않는다
3. 완료 항목에 날짜를 빠뜨리지 않는다
4. 하나의 사실을 여러 문서에 중복 기록하지 않는다

## Validation
- [ ] 변경된 코드에 대응하는 문서가 갱신됨
- [ ] PROJECT-STATUS.md 체크리스트가 현재 상태 반영
- [ ] 완료 항목에 날짜 포함
- [ ] DECISION-LOG.md에 아키텍처 결정 기록됨
- [ ] 14일 초과 완료 항목 archive 후보 표시

## Output Template
```
[doc-update 완료]
- 변경 분류: {trivial/feature/architecture/breaking}
- 갱신 문서: {파일 목록}
- 상태 체크리스트: {갱신/변경없음}
- 결정 로그: {추가/변경없음}
- archive 후보: {있음/없음}
```
