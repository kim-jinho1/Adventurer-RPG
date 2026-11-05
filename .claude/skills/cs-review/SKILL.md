---
name: cs-review
description: 변경한 C# 코드(Assets/Code)를 Unity 관점에서 리뷰한다. "리뷰해 줘", "코드 검토", 커밋 전 점검 요청 시 사용.
---

# C# 코드 리뷰 (Unity-RPG)

## 변경 내용

!`git diff HEAD -- "*.cs"`

새로 추가된(추적 안 된) 스크립트:

!`git ls-files --others --exclude-standard -- "*.cs"`

인자로 파일·커밋 범위가 주어지면(`$ARGUMENTS`) 위 diff 대신 그것을 리뷰한다. 변경이 없으면 그렇다고 말하고 끝낸다.

## 리뷰 전에

- `Assets/Code/CLAUDE.md`(역할 분담·설계 관례·최적화·코드 컨벤션)를 기준으로 삼는다. diff만 보지 말고
  **바뀐 메서드의 호출부, 구독·해제 짝, 인스펙터에 걸린 SO 값**까지 확인한다.
- **리뷰만 한다. 파일은 고치지 않는다.** 이 프로젝트는 코드를 사용자가 직접 쓴다.
  고치는 법은 "무엇을·왜·어떤 API"까지만 주고, 완성 코드는 사용자가 요청할 때만.
- 추측으로 지적하지 않는다. 확인이 안 된 것은 "확인 필요"로 따로 적는다.

## 검토 항목

- `Update()`/`FixedUpdate()` 안의 `GetComponent()`, `GetModule<T>()`, `Find*()`, `Camera.main`
- `new`·LINQ·문자열 결합으로 인한 프레임당 힙 할당
- 누락된 null 체크 (필수 의존성은 `Debug.Assert`로). 메시지가 규칙 형식인가 — 한국어 `$"[클래스명] 무엇이 어떻다: 대상"` + context 오브젝트,
  문구 넷(연결 안 됨 · 모듈/컴포넌트 없음 · 중복 · 찾지 못함), **Assert 하나에 조건 하나**(`&&`로 묶으면 무엇이 없는지 모름)
- `public` 필드 남용 (외부 노출은 `[field: SerializeField] public X { get; private set; }`, 내부용은 `[SerializeField] private`)
- 이벤트 구독(`+=`)과 해제(`-=`, `OnDestroy`)가 짝으로 있는가
- SOLID 원칙 준수, GOD class 금지 (`Character`에 로직이 몰리는가 → 모듈로)
- 구체 구현체보다 인터페이스(`IMover`, `IStatModule`, `IDamageable` …)에 의존하는가. 부득이하게 구체 타입에 의존해야 하면 이유를 말한다
- "적·아이템·퀘스트 추가가 데이터 작업만으로 끝나는가" — 코드 수정을 요구하는 구조면 지적
- 유니티 함정: 인스펙터 직렬화 값이 코드 기본값을 덮음, `ScriptableObject` 런타임 변경이 에셋에 남음,
  `[SerializeField]` 필드명 변경 시 값 유실(`FormerlySerializedAs`), 애니메이터 파라미터를 문자열로 접근
- 직렬화 이름 변경 시 `FormerlySerializedAs` 누락, BT 그래프가 쓰는 타입 이름 변경 시 그래프 `.asset`의 타입 문자열 미변경
- 남아 있는 디버깅용 `Debug.Log`, 컨벤션 위반 — 이름(`*Enum` · `Handle*` · `_camelCase` 상수 포함 · `Is/Can/Has`), Allman, `1f`,
  140자 · 줄 끝 공백 · `using` 순서(`System.*` 먼저, 나머지 알파벳)
- 주석 규칙 — 타입은 여러 줄 summary(속성 위), 인터페이스 멤버 · 인터페이스에 없는 public 메서드는 한 줄 summary,
  구현 쪽 · `override`에는 안 닮, 필드는 줄 끝 `//` 명사구, 메서드 안 `//`는 "왜"를 한 문장으로
- 그 외 문제점 및 유의사항

## 출력 형식

모든 항목은 위험도에 따라 **치명적 / 중간 / 경고**로 분류한다. 항목마다 **`파일:줄` + 심볼**, **증상 → 원인 → 고치는 방향**을 평이하게 쓴다.

```
## 치명적 (버그·크래시·데이터 유실)
### 1. <한 줄 요약> — `Agents/Characters/Character.cs:42` `Character.HandleHit`
- 증상:
- 원인:
- 고치는 방향: 무엇을·왜·어떤 API

## 중간 (성능·설계 문제)
## 경고 (컨벤션·가독성)
## 유지할 설계
- 이번 변경에서 잘한 선택과 이유 (문제만 나열하지 않는다)
## 확인 필요
```

마지막에 "치명적 N / 중간 N / 경고 N" 한 줄 요약을 붙인다.
