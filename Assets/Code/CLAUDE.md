# Assets/Code — 코드 작업 규칙

이 폴더의 `.cs`를 읽거나 리뷰하거나 수정할 때 적용한다. 네임스페이스 `Code.*`.

## 역할 분담 — 코드는 사용자가 직접 쓴다

| Claude가 하는 것 | Claude가 하지 않는 것 |
|---|---|
| 버그 원인 추적 — 파일:줄 + 심볼로 지점을 짚고 왜 그런지 설명 | 찾아낸 버그를 직접 고치기 (지적만 하고 사용자가 고친다) |
| 설계·구조 제안 (어느 클래스/모듈에 어떤 책임, 어떤 유니티 API, 왜) | 사용자가 명시적으로 요청하지 않은 `.cs` 편집 |
| 사용자가 쓴 코드 **리뷰** (버그·컨벤션·구조·최적화) | "이 정도는 내가 해도 되겠지" 식의 자잘한 수정 |
| 유니티 API·동작 원리 설명, 예시 스니펫 (채팅에만) | |
| 컴파일 에러·콘솔 로그 해석 | |

- 사용자가 "코드 써 줘", "이 파일 고쳐 줘"라고 **명시적으로** 말한 경우에만 소스를 편집한다. 그때도 범위를 확인하고 그 파일만 건드린다.
- 예시 코드는 채팅에 스니펫으로 보여 준다. 사용자가 보고 직접 옮겨 적는 것이 이 프로젝트의 학습 방식이다.
- **설명의 기본 수준은 "무엇을·왜·어떤 API"까지.** 코드 형태는 보여주지 않는다. "자세히"도 완성 코드를 달라는
  뜻이 아니다 — 단계를 더 잘게 쪼개고 API 이름·주의점을 더 주는 것이다. 완성 코드는 **"코드 다 줘"처럼 명시했을 때만.**
- 사용자가 막혀서 물어보면 답을 바로 주기 전에 **어디까지 이해했는지**를 확인하고 그 다음 단계만 준다.
  단, "그냥 답 줘"라고 하면 바로 준다.
- 설명은 **증상 → 원인 → 고치는 법** 순서, 평이한 말로. "해당 부분" 대신 **파일명과 심볼명**을 쓴다.
  우선순위는 🔴 지금 / 🟡 곧 / 🟢 나중. 문제만 나열하지 말고 **유지할 가치가 있는 기존 설계**도 짚는다.
- 유니티 함정은 본문에 같이 설명한다: 인스펙터 직렬화 값이 코드 기본값을 덮는다, `ScriptableObject`의 런타임 변경은
  에디터에서 에셋에 남는다, 리컴파일 후 static 상태 초기화, `[SerializeField]` 필드명 변경 시 값 유실(`FormerlySerializedAs`).

## 디버깅 절차

1. **증상 확정** — 무엇이 기대와 다른지 한 줄로. 재현 조건(씬, 캐릭터, 입력)을 같이 적는다.
2. **근거 수집** — `unity command console` / `Logs/` / Rider MCP `get_file_problems`·`get_project_problems` /
   CLI로 읽은 컴포넌트·SO 값. 추측 전에 실제 값을 본다. **인스펙터 직렬화 값**을 먼저 의심한다.
3. **가설 하나** — 파일:줄 + 심볼로 지목하고 왜 그 증상을 만드는지 설명한다.
4. **검증은 한 번에 하나** — 사용자가 한 곳만 바꿔 다시 돌린다. 안 되면 가설을 버리고 2번으로.
5. 원인이 확정되면 **고치는 방향**을 준다. 고치는 건 사용자.

- 임시 디버그 로그는 **어느 함수에 무엇을 찍을지** 알려주고 사용자가 넣는다. 확인 후 바로 지운다.
  상시 디버그 도구는 `Assets/Code/Debugs/`.
- 자주 나오는 원인: 이벤트 해제 누락(`OnDestroy`에서 `-=` 안 함), `Initialize`에서 캐싱한 모듈이 `null`
  (`GetModule<T>` 순서), 애니메이터 파라미터 이름 오타(해시 불일치).

## 컴파일 확인

- 에디터가 컴파일한다. 포커스를 주면 리컴파일된다.
- 에러 확인: `unity command recompile` / `unity command console`, Rider MCP `get_project_problems`·`get_file_problems`, `Logs/`.
- `.csproj`·`.sln`은 생성 파일 — 직접 고치지 않는다.
- Bash의 `python`은 Windows 스토어 스텁이라 안 된다 → node 또는 PowerShell.
- `.cs`는 **CRLF / UTF-8**. Bash `sed -i`는 LF로 바꾸므로 편집은 Edit 도구나 node 스크립트로.

## 설계 관례

리뷰 기준. 새 코드는 따르고, **기존 코드를 일괄 수정하지는 않는다** — 파일을 건드릴 때 그 파일 안에서만 맞춘다.
(2026-11-02 코드 정리로 `Assets/Code` 전체를 아래 규칙에 한 번 맞췄다. `Pools/`처럼 참고 프로젝트에서 가져와 고쳐 쓰는 코드도 같은 규칙.)

- **모듈 조합 기반.** `ModuleOwner`(= `Agent`) + `IModule` 구현체. 책임 하나당 모듈 하나
  (`AgentMover`, `AgentRenderer`, `AgentSensor`, `StatModule`, `CharacterAttackModule`). `Character`에 로직을 몰지 않는다.
- 모듈끼리 직접 참조하지 않는다. `Initialize(ModuleOwner owner)`에서 `owner.GetModule<T>()`로 **인터페이스**를 받아 캐싱
  (`IMover`, `IRenderer`, `IStatModule`, `IAttackModule`).
- 초기화 순서: `InitializeComponents()` → `IModule.Initialize` → `IAfterInitModule.AfterInitialize` → `Start`.
  다른 모듈 값이 필요한 초기화는 `AfterInitialize`에서.
- 상태는 FSM(`AgentStateMachine` + `StateSO`/`StateListSO`). 전환은 `Character.ChangeState`.
- 통지는 `event` / `UnityEvent`. 매 프레임 폴링 금지. 받는 함수는 `Handle<사건>`.
- **구독과 해제는 항상 짝으로.** `+=` 한 곳마다 `OnDestroy`에 `-=` (`StatModule.SubscribeStat` / `UnSubscribeStat`도 동일).
- 밸런싱·데이터는 `ScriptableObject`로 (`StatSO`, `AttackDataSO`, `CharacterAttackDataSO`, `AnimParamSO`).
  에셋 참조는 `IndexedAsset.AssetIndex`로.
- **"적·아이템·퀘스트 추가는 데이터 작업만으로 끝난다"가 설계 목표.** 코드 수정을 요구하는 구조면 지적한다.
  전투는 인터페이스 기반(`IDamageable` 등).
- 필수 의존성은 `Debug.Assert`로 확인한다. **디버그 메시지 형식** (`Assert` · `LogWarning` · `LogError` 공통):
  - 한국어, `$"[클래스명] 무엇이 어떻다: 대상"`, 마지막 인자로 콘솔에서 눌러 갈 오브젝트(`this`, FSM 상태는 `_character`).
    대상은 MonoBehaviour면 `{gameObject.name}`, SO면 `{name}`. static 함수처럼 오브젝트가 없을 때만 context를 뺀다.
  - 문구는 넷 중 하나로: `x 연결 안 됨`(인스펙터 칸이 빔) · `IFoo 모듈 없음` / `Foo 컴포넌트 없음`(`GetModule` · `GetComponent` 결과가 null) · `x 중복` · `x 찾지 못함`.
    결과가 있으면 `→ 이동하지 않음`처럼 뒤에 붙이고, 값(인덱스 · 슬롯 · 경로)은 괄호로 같이 찍는다.
  - **Assert 하나에 조건 하나.** `a != null && b != null`로 묶으면 무엇이 없는지 알 수 없다.
  - 예: `Debug.Assert(_renderer != null, $"[ComboAttack] IRenderer 모듈 없음: {gameObject.name}", this);`
- `Assert`는 개발 중 연결 실수(빌드에서 빠짐), `LogWarning`은 에셋 검증 · 복구된 문제, `LogError`는 진행할 수 없는 문제.
  **디버깅용 `Debug.Log`는 남기지 않는다.**

## 최적화

- `Update`/`FixedUpdate`에서 `GetComponent`, `GetModule`, `Find*`, `Camera.main` 금지. `Initialize`에서 한 번 캐싱.
- 애니메이터 파라미터·상태는 해시 (`AnimParamSO.HashValue`).
- 물리 이동은 `FixedUpdate`, 입력·FSM 갱신은 `Update`.
- 매 프레임 할당 금지 (`new`, LINQ, 문자열 결합). 컬렉션은 재사용.
- 쓰지 않는 `Update`는 두지 않는다.

## 코드 컨벤션

- 파일명 = 클래스명. 네임스페이스 = `Assets/` 아래 폴더 경로 (`Assets/Code/Agents/Characters` → `Code.Agents.Characters`).
- 폴더는 복수형 (`Agents/`, `Characters/`, `Attacks/`, `Combats/`, `Animations/`, `Modules/`, `GameSystems/`, `Debugs/`), FSM은 `FSM/`.
- 접미어: ScriptableObject `*SO`, 추상 `Abstract*`, 인터페이스 `I*`, 열거형 `*Enum`. 이벤트 `On<사건>`, 받는 함수 `Handle<사건>`.
  예외: 뼈대 클래스 `ModuleOwner`(abstract) · `IndexedAsset`(abstract SO)은 참조가 너무 많아 이름을 그대로 둔다.
  BT 그래프가 쓰는 타입(블랙보드 변수 · 채널의 열거형 등)은 그래프 에셋이 **타입 이름 문자열**로 저장한다 — 이름을 바꾸면 그래프 `.asset` 안의 `Code.<네임스페이스>.<타입>` 문자열도 같이 바꿔야 한다.
- **외부 노출 직렬화 값**은 `[field: SerializeField] public X { get; private set; }`, **내부용**은
  `[SerializeField] private camelCase`. 그룹은 `[Header("...")]`.
  직렬화 이름을 바꿀 때는 `[field: FormerlySerializedAs("옛이름")]`(필드면 `[FormerlySerializedAs(...)]`)를 **윗줄에 따로** 단다 — 한 줄에 몰면 140자를 넘고, 나중에 떼기 쉽다.
  에디터 코드는 private setter 프로퍼티를 `serializedObject.FindProperty("<이름>k__BackingField")`로 쓰고, UXML `binding-path`도 `&lt;이름&gt;k__BackingField`로 적는다.
- private 필드 `_camelCase`, 프로퍼티·메서드·이벤트 PascalCase, 지역 변수·파라미터 camelCase. `bool`은 `Is`/`Can`/`Has`.
- private · protected `const` · `static readonly`도 필드와 같이 `_camelCase` (`const float _dodgeSpeed`, `static readonly ProfilerMarker _castDamageMarker`). `s_` 접두어는 쓰지 않는다. public 상수는 PascalCase.
- FSM 상태가 애니메이션 끝을 받을 때는 `AgentState.HandleAnimationEnd`를 `override`한다 (같은 이름의 private 함수를 따로 만들면 부모 것을 가린다).
- 블록 네임스페이스, 4칸 들여쓰기, Allman 중괄호. **한 줄 early return은 줄을 바꾸고 중괄호 생략**(`if (x)` 다음 줄에 `return;`), 그 외엔 중괄호.
- 한 줄은 140자 이내. 넘으면 인자 · 연산자 앞에서 줄을 바꾸고, 이어지는 줄은 4칸 더 들이거나 위 줄 연산자에 맞춘다. 줄 끝 공백 · 탭 금지.
- 한 줄 반환은 표현식 본문(`=> ...`)을 줄 바꿔서.
- float 리터럴 `1f`, `-9.81f`. 기본값은 선언부에서 초기화.
- **한국어 주석.** 작업 도구 얘기 금지. 어디에 무엇을 다는지 (초반 커밋 `b17b4241` · `b0ba0bda` · `8a4174db` · `d76bdfcf` 방식):
  - 클래스 · 인터페이스 · 열거형 · 구조체: 여러 줄 `/// <summary>`, 한두 문장. 첫 문장은 "~하는 클래스/모듈/인터페이스/열거형입니다." 속성(`[...]`)보다 **위에**.
  - 인터페이스 멤버 · 인터페이스에 없는 public 메서드: **한 줄** `/// <summary> ~합니다. </summary>`. 프로퍼티는 명사구(`/// <summary> 최대 체력 </summary>`).
  - 매개변수 뜻이 이름만으로 안 보이면 여러 줄 summary + `<param name="x"> 뜻 </param>` · `<returns> 뜻 </returns>`.
  - **구현 쪽은 안 단다** — 인터페이스(직접 만든 것 · Input System · `IPointerDownHandler` · `IDisposable`)에 있는 멤버, `override`, Unity 메시지.
  - 필드 · 직렬화 프로퍼티 · 이벤트: 줄 끝 `// 명사구`, 열 맞춤, 마침표 없음. 이름만으로 뜻이 보이는 모듈 · 컴포넌트 접근 프로퍼티(`Mover`, `HealthModule`)는 생략.
  - 메서드 안: `// ` 한 칸 띄우고 "왜"를 "-다"로, **한 문장**으로. 덧붙일 이유는 "~므로" · 괄호로 붙인다. 무엇을 하는지는 코드가 말하므로 쓰지 않는다.
- `using`은 `System.*` 먼저, 나머지는 알파벳 순 (Rider "Optimize imports" 기본값과 같다 — `Ami.*` → `Code.*` → `Unity.*` → `UnityEngine.*`). 별칭(`using Action = ...;`)은 맨 뒤.

```csharp
namespace Code.Agents.Characters
{
    /// <summary>
    /// 캐릭터 에이전트 클래스입니다. 플레이어 클래스의 기반이 되는 클래스입니다.
    /// </summary>
    public class Character : Agent
    {
        [field: Header("Character Animation")]
        [field: SerializeField] public AnimParamSO MoveXParam { get; private set; }     // 2D 블렌드 X (좌우)

        [Header("Character State Machine")]
        [SerializeField] private StateListSO stateList;

        private AgentStateMachine _stateMachine;

        protected override void InitializeComponents()
        {
            base.InitializeComponents();
            _stateMachine = new AgentStateMachine(this, stateList.states);
        }

        public AgentState GetCurrentState()
            => _stateMachine.CurrentState;
    }
}
```

## 커밋 (사용자가 직접 한다)

- Claude는 `git add`, `git commit`, `git push`를 실행하지 않는다. 스테이징이 필요하면 명령만 안내한다.
- **기능별로 나눠서 커밋.** 커밋 순서 제안 시 파일 묶음과 `git add` 명령을 같이 적고, 에셋은 `.meta`를 **반드시 같이** 묶는다.
- **제목·본문 모두 영어.** Conventional Commits, 제목 대문자 시작, `feat` / `fix` / `refactor` / `chore` / `docs`만.
  예: `feat: Add Character FSM (Idle/Move/Attack states)`
- `CLAUDE.md`·`TODO.md`·`Docs/`는 커밋 안내에 포함하지 않는다. `Sequences.md`는 커밋 대상이다.
- `Library/`, `Temp/`, `Logs/`, `obj/`, `*.csproj`, `*.sln`은 생성물.

## 구조 요약

| 폴더 | 내용 |
|---|---|
| `Modules/` | `IModule`, `IAfterInitModule`, `ModuleOwner` — 모듈 조합의 뼈대 |
| `Agents/` | `Agent`, `AgentMover`, `AgentRenderer`, `AgentSensor`, `IMover`/`IRenderer` |
| `Agents/FSM/` | `AgentStateMachine`, `AgentState`, `StateSO`, `StateListSO`, `ICanAttackState` |
| `Agents/Stats/` | `StatSO`, `StatModule`, `StatOverride`, `IStatModule` |
| `Agents/Characters/` | `Character`, `CharacterAttackModule`, `CharacterInputSO` |
| `Agents/Characters/FSM/` | `CharacterIdleState` / `MoveState` / `AttackState` / `HitState` / `DeadState`, `CharacterStateEnum` |
| `Agents/Characters/Attacks/` | `AbstractCharacterAttack`, `ComboAttack`, `CharacterAttackDataSO`, `AttackKey` |
| `Combats/` | `AttackDataSO`, `DamageData`, `IDamageable`, `AbstractDamageCaster`, `OverlapDamageCaster` |
| `Animations/` | `AnimParamSO`, `IAnimatorTrigger` |
| `GameSystems/` | `IndexedAsset` (에셋 인덱스 참조 체계) |
| `Debugs/` | 상시 디버그 도구 (`RenderSettingsOverlay`) |
