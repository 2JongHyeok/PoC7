# Codex Collaboration Guide (CLI-Optimized)

## 0. Environment (항상 작업 전 확인)
- Unity Version: 6000.0.61f1
- Render Pipeline: URP
- Target Platform: PC
- Required Asset: Odin Inspector
- Game Dimension: 2D (필수)
- Codex는 작업 시작 전에 위 4가지를 반드시 출력하고 확인한다.

## 1. Collaboration Rules
- KISS, YAGNI 준수, 단순하고 읽기 쉬운 필수 코드만 작성.
- SRP: 한 스크립트는 한 가지 책임, 파일/클래스 이름은 책임을 반영.
- 비트리비얼 동작은 State Pattern 필수: `IState`, `BaseState`, `StateMachine`(Enter/Exit/Tick).
- 시스템 간 통신은 C# event 또는 UnityEvent만 사용, 직접 의존 회피.
- 설정/튜닝 값은 ScriptableObject로 관리, 매직 넘버 금지.
- UI는 TextMeshPro만 사용, 모든 텍스트는 Inspector에서 직렬화.
- 주요 지점에 로그: 초기화/상태 변경/실패/IO. 로그는 태그·식별자·상태 포함.
- 모든 코드 식별자와 주석은 영어, 짧고 정확하게.
- 모호하면 질문하거나 `TODO:<question>` 남김. Unity 버전이나 파이프라인 불확실 시 작업 중단 후 확인.
- Get/Set 프로퍼티는 필요할 때만 사용.

## 2. Project Metadata
- Project Name: King
- One-Line Goal: Grow and lead your own tribe.
- MVP Scope: 1 Day
- Out-of-Scope: 사용자 정의까지 기다림(임의 정의 금지).

## 3. Required Output for Every Task
1) Assumptions / Questions: 필요 시만, 불명확하면 `TODO:<question>`.  
2) File List (Added/Modified): 전체 Unity 경로 예시 `Assets/_Project/Scripts/Runtime/Core/StateMachine.cs`.  
3) Code: 헤더 주석(목적+SRP), 영어 식별자·주석, Debug 로그, 이벤트, ScriptableObject 훅, Odin 속성 필요 시, `#region` 사용.  
4) Scene Connection Guide: 생성할 프리팹, 직렬화 참조, Addressables 그룹명 포함.  
5) Micro Test Plan: 입력/예상 동작/성공 기준.

## 4. Default Architecture (사용자 오버라이드 없을 때)
- 폴더: `Assets/_Project/Scripts/Runtime/<Domain>`, `Scripts/Editor`, `SO/Configs`, `UI/<Screens,Widgets>`, `Prefabs`, `Scenes/<Bootstrap,Main,Test>`, `Addressables/<Groups>`, `Tests/<EditMode,PlayMode>`.
- 패턴: State Pattern(전이 존재 시 필수), 이벤트(C# event/UnityEvent/옵션 Signal), 데이터(SO 튜닝, 런타임 저장 JSON), 입력(IInputService 인터페이스, Unity Input System 교체 가능), 풀링(공용 ObjectPool).
- UI: TextMeshPro 필수, 직렬화 참조만 사용(Find 금지), SafeArea 준수, 선택적으로 SO 기반 텍스트 테이블.

## 5. Quality Requirements
- 핫 패스에서 0B GC alloc, 핫 루프에서 LINQ·박싱 금지.
- `deltaTime` vs `unscaledDeltaTime` 사용 명시.
- 최소 공개 API, null 가드 및 Try* 패턴, 의미 있는 Assert.

## 6. Output Rules for Code Generation
- 식별자/주석 영어, SRP 준수(파일 1개=책임 1개).
- 상태 전환 동작은 StateMachine 사용.
- 교차 시스템 통신은 이벤트 사용.
- 튜닝 값은 ScriptableObject에서 로드.
- UI 텍스트는 TextMeshPro.
- 주요 지점 Debug.Log, `#region`으로 논리 구획.
- 모든 문맥/출력은 한국어로 안내(코드 내 식별자는 영어).

## 7. Final Instruction to Codex (First Task)
- 환경 확인 후, 위 규칙을 따른 프로젝트 King의 최소 실행 가능한 MVP 코어 루프를 제안한다.

## 8. Workflow Confirmation Rule
- 모든 지시를 받으면 먼저 사용자 발화를 한국어로 요약하여 확인(Confirm) 요청을 받고, 승인을 받은 이후에만 작업을 진행한다.
