# Little Hacker 프로젝트 문서

> **작성일**: 2025-10-20
> **Unity 버전**: 2022.3.x
> **프로젝트 타입**: 2D 수학 퍼즐 게임

---

## 📋 목차

1. [프로젝트 개요](#프로젝트-개요)
2. [파일 구조](#파일-구조)
3. [아키텍처](#아키텍처)
4. [핵심 시스템](#핵심-시스템)
5. [게임플레이 메커니즘](#게임플레이-메커니즘)
6. [데이터 구조](#데이터-구조)
7. [씬 구조](#씬-구조)
8. [기술 스택](#기술-스택)

---

## 프로젝트 개요

**Little Hacker**는 수학 퍼즐을 테마로 한 2D 그리드 기반 퍼즐 게임입니다.

### 게임 컨셉
- 플레이어가 맵을 이동하며 숫자와 연산자를 순서대로 수집
- 수집한 아이템으로 수식을 완성하여 계산 수행
- 계산 결과가 목표 문(Door)의 값과 일치하면 스테이지 클리어

### 주요 특징
- **그리드 기반 이동**: 4방향 이동 (상하좌우)
- **실시간 계산**: 숫자-연산자-숫자 패턴 완성 시 자동 계산
- **퍼즐 요소**: 박스 밀기, 벽 회피, 함정 피하기
- **Undo 시스템**: 턴 단위로 되돌리기 가능
- **시나리오 기반**: 3개 시나리오 × 15개 스테이지 구조 (현재 시나리오 1만 구현)

---

## 파일 구조

### C# 스크립트 파일 (총 18개)

#### 📁 루트 Scripts 디렉토리 (8개)
| 파일명 | 역할 | 주요 기능 |
|--------|------|-----------|
| `GameManager.cs` | 게임 총괄 관리자 | 스테이지/시나리오 관리, 턴 추적, 씬 초기화 |
| `Player.cs` | 플레이어 컨트롤러 | 이동, 수집, 계산, 충돌 처리, Undo |
| `UiManager.cs` | UI/메뉴 시스템 | 스테이지 선택 그리드, 잠금/해금 표시 |
| `RayEmission.cs` | Raycast 유틸리티 | 2D Raycast 헬퍼 함수 |
| `Helper.cs` | 유틸리티 헬퍼 | (현재 거의 비어있음) |
| `Util.cs` | 터치 입력 유틸 | 크로스 플랫폼 터치/마우스 입력 |
| `Option.cs` | 옵션 메뉴 | 오디오 설정 (BGM/SFX 볼륨) |
| `CSVReader.cs` | CSV 파싱 유틸리티 | CSV 파일 읽기 및 파싱 |

#### 📁 Manager 디렉토리 (7개)
| 파일명 | 역할 | 패턴 |
|--------|------|------|
| `Managers.cs` | 매니저 통합 관리 | 싱글톤 코디네이터 |
| `Singleton.cs` | 싱글톤 베이스 클래스 | Generic 싱글톤 패턴 |
| `TextManager.cs` | 대화/텍스트 시스템 | 대화 출력, CSV 로드 |
| `SoundManager.cs` | 오디오 관리 | BGM/SFX 재생, 볼륨 제어 |
| `SoundBox.cs` | 사운드 에셋 컨테이너 | AudioClip 저장소 |
| `TemporaySoundPlayer.cs` | 임시 오디오 소스 | 일회성 사운드 재생 |
| `CoroutineHelper.cs` | 코루틴 유틸리티 | 비MonoBehaviour 코루틴 지원 |

#### 📁 MapCreate 디렉토리 (3개)
| 파일명 | 역할 |
|--------|------|
| `MapCreate.cs` | 맵 렌더링 및 레벨 로더 |
| `ObjectData.cs` | 게임 오브젝트 데이터 컨테이너 |
| `MakeJsonMapData.cs` | CSV → JSON 변환기 |

### 데이터 파일

#### 레벨 데이터 (시나리오 1: 16개 스테이지)
```
Assets/Resources/MapDatasJSON/
├── SN_1_ST_1.json
├── SN_1_ST_2.json
├── ...
└── SN_1_ST_16.json

Assets/Resources/MapDatasCSV/
├── SN_1_ST_1.csv
├── SN_1_ST_2.csv
├── ...
├── SN_1.csv (스테이지 시작 대화)
└── SN_1_Clear.csv (스테이지 클리어 대화)
```

### 씬 파일 (5개)

| 씬 파일 | 목적 | 씬 인덱스 |
|---------|------|-----------|
| `1.StartScene.unity` | 메인 메뉴/시작 화면 | 0 |
| `MainScenes.unity` | 스테이지 선택 화면 | 1 |
| `2.StoryModeScene.unity` | 게임플레이 씬 | 2 |
| `SampleScene.unity` | Unity 기본 템플릿 | - |
| `TestJoung.unity` | 테스트용 씬 | - |

---

## 아키텍처

### 전체 아키텍처 패턴: MVC + Manager Pattern

```
┌─────────────────────────────────────────────┐
│         Input Layer (Player.cs)              │
│   - 터치/키보드 입력 처리                      │
└──────────────────┬──────────────────────────┘
                   ↓
┌─────────────────────────────────────────────┐
│      Game State (GameManager.cs)             │
│   - 시나리오/스테이지 관리                     │
│   - 턴 카운팅, 씬 초기화                       │
└──────────────────┬──────────────────────────┘
                   ↓
┌─────────────────────────────────────────────┐
│     Map/Level System (MapCreate.cs)          │
│   - JSON 레벨 데이터 로드                      │
│   - 맵 오브젝트 렌더링                         │
└──────────────────┬──────────────────────────┘
                   ↓
┌─────────────────────────────────────────────┐
│      Manager Systems (Managers.cs)           │
│   ├── TextManager (대화 시스템)               │
│   ├── SoundManager (오디오 시스템)            │
│   └── Singleton (베이스)                      │
└──────────────────┬──────────────────────────┘
                   ↓
┌─────────────────────────────────────────────┐
│         UI Layer (UiManager.cs)              │
│   - 스테이지 선택, 옵션 메뉴                   │
└─────────────────────────────────────────────┘
```

### 디자인 패턴

1. **Singleton Pattern**: `Singleton<T>` 제네릭 베이스 클래스
   - DontDestroyOnLoad로 씬 전환 시에도 유지
   - TextManager, SoundManager 등에 적용

2. **MVC 분리**:
   - Model: MapData (JSON), ObjectData
   - View: UI 컴포넌트, Sprite 렌더링
   - Controller: GameManager, Player, MapCreate

3. **Dictionary 기반 상태 관리**:
   - 플레이어 수식: `Dictionary<int, string>` (순서별 아이템 저장)
   - Undo 시스템: `Dictionary<int, revertObject>` (턴별 상태 저장)

---

## 핵심 시스템

### 1. 게임 관리 시스템 (GameManager.cs)

**역할**: 게임의 중앙 상태 컨트롤러

**주요 책임**:
- 현재 시나리오 (1~3) 및 스테이지 (1~15) 추적
- 플레이어 턴 카운트 관리
- 그리드 크기 설정
- 씬 초기화 및 정리
- 스테이지 완료 이벤트 트리거
- Canvas/UI 카메라 설정

**주요 필드**:
```csharp
public static int nowScenario = 1;    // 현재 시나리오
public static int nowStage = 1;       // 현재 스테이지
public static int playerTurn = 0;     // 플레이어 턴 수
public static int gridSize = 70;      // 그리드 셀 크기
```

**핵심 메서드**:
- `Awake()`: 싱글톤 인스턴스 생성, DontDestroyOnLoad 설정
- `SetCanvas()`: 캔버스 카메라 설정
- 씬 전환: `SceneManager.LoadScene()` 사용

---

### 2. 플레이어 시스템 (Player.cs)

**역할**: 플레이어 캐릭터 로직 및 상호작용

**주요 책임**:
- 터치/마우스 입력 처리 (드래그로 방향 결정)
- 키보드 입력 처리 (WASD 이동)
- 4방향 그리드 기반 이동
- 수학 아이템 수집 (숫자, 연산자)
- 실시간 수식 계산
- Undo/되돌리기 시스템 구현
- 충돌 감지 (벽, 아이템, 박스, 문)
- 수집한 아이템 UI 렌더링

**이동 시스템**:
```csharp
// 터치 입력: Util.TouchUtil.GetMouseDirection() 사용
// 키보드 입력: WASD 키
// 방향: 0=상, 1=하, 2=좌, 3=우
```

**수식 처리 로직**:
```csharp
Dictionary<int, string> formula;  // 0: 첫 숫자, 1: 연산자, 2: 둘째 숫자
// 패턴: Number → Operator → Number → Calculate
// 계산 후 결과가 다음 수식의 첫 번째 피연산자가 됨
```

**Undo 시스템**:
```csharp
class revertObject {
    Vector2 playerPosition;
    Dictionary<string, Vector2> objectPosition;  // 박스 위치
    Dictionary<int, string> formula;
    // ... 기타 상태
}
Dictionary<int, revertObject> backUpRevert;  // 턴별 저장
```

**디버그 키**:
- Q: 현재 수식 출력
- M: 다음 스테이지
- N: 이전 스테이지
- R: 스테이지 재시작
- K: 모든 스테이지 리셋
- L: 모든 스테이지 해금

---

### 3. 맵 생성 및 렌더링 시스템

#### MapCreate.cs
**역할**: 동적 레벨 생성 및 렌더링

**주요 책임**:
- JSON 레벨 데이터 Resources에서 로드
- 맵 구조 파싱 및 역직렬화
- 게임 오브젝트 렌더링:
  - 벽 (Walls)
  - 숫자 아이템 (Numbers)
  - 연산자 아이템 (Operators)
  - 박스 (Boxes)
  - 함정 (Traps)
  - 게이트 (Gates)
  - 올연산자 (AllOperators)
  - 문 (Door)
  - 플레이어 (Player)
- 맵 크기에 따른 카메라 크기 조정
- 레벨 전환 및 초기화 처리

**맵 데이터 로드 흐름**:
```csharp
1. Resources.Load<TextAsset>($"MapDatasJSON/SN_{scenario}_ST_{stage}")
2. JsonUtility.FromJson<MapData>(json)
3. 2D 배열 순회하며 프리팹 인스턴스화
4. 카메라 orthographicSize 조정
```

#### ObjectData.cs
**역할**: 게임 오브젝트에 부착되는 데이터 컴포넌트

**필드**:
```csharp
public int num;              // 숫자 값 (숫자, 문)
public string doorNum;       // 문 식별자
public string oper;          // 연산자 (+, -, *, /)
public bool boxTrigger;      // 박스 밀림 중 여부
public bool boxStop;         // 박스 장애물 충돌
public Vector2 boxMoveDir;   // 박스 이동 방향
```

#### MakeJsonMapData.cs
**역할**: CSV를 JSON으로 변환

**기능**:
- CSV 파일 읽기 (CSVReader 사용)
- MapData 구조체로 변환
- JSON 형식으로 직렬화
- Resources 폴더에 저장

---

### 4. 매니저 시스템 (Singleton Pattern)

#### Managers.cs
**역할**: 싱글톤 매니저들의 통합 관리자

**구조**:
```csharp
public class Managers : MonoBehaviour
{
    static Managers s_instance;
    static Managers Instance { get { Init(); return s_instance; } }

    TextManager _text = new TextManager();
    public static TextManager Text { get { return Instance._text; } }

    SoundManager _sound = new SoundManager();
    public static SoundManager Sound { get { return Instance._sound; } }
}
```

#### Singleton.cs
**역할**: 제네릭 싱글톤 베이스 클래스

**특징**:
```csharp
public class Singleton<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    public static T Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<T>();
                if (_instance == null)
                {
                    GameObject obj = new GameObject(typeof(T).Name);
                    _instance = obj.AddComponent<T>();
                }
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
```

---

### 5. 텍스트/대화 시스템 (TextManager.cs)

**역할**: 대화 표시 및 진행 관리

**주요 기능**:
- 시나리오/스테이지별 CSV에서 대화 로드
- 글자별 애니메이션으로 대화 표시
- 스테이지 클리어/실패 대화 처리
- 대화 스킵 기능
- 대화 상태 및 진행 관리

**대화 CSV 형식**:
```
SN_1.csv: 스테이지 시작 대화
SN_1_Clear.csv: 스테이지 클리어 대화
```

**핵심 메서드**:
```csharp
public void ShowDialogue(int scenario, int stage, bool isClear)
public IEnumerator TypeText(string text, float delay)
public void SkipDialogue()
```

---

### 6. 사운드 시스템

#### SoundManager.cs
**역할**: 오디오 재생 및 관리

**기능**:
- Audio Mixer 통합으로 볼륨 제어
- BGM/SFX 타입 분류
- 사전 로드된 AudioClip Dictionary
- BGM 루프 지원
- 임시 AudioSource 정리
- PlayerPrefs에 볼륨 설정 저장

**주요 메서드**:
```csharp
public void PlaySound(string clipName, SoundType type)
public void PlayBGM(string clipName)
public void StopBGM()
public void SetVolume(SoundType type, float volume)
```

#### SoundBox.cs
**역할**: AudioClip 저장소

**구조**:
```csharp
[System.Serializable]
public class SoundClip
{
    public string name;
    public AudioClip clip;
}
public List<SoundClip> bgmClips;
public List<SoundClip> sfxClips;
```

#### TemporaySoundPlayer.cs
**역할**: 일회성 사운드 재생 후 자동 파괴

---

### 7. UI 시스템

#### UiManager.cs
**역할**: 메인 UI 및 메뉴 관리

**기능**:
- 스테이지 선택 그리드 표시 (3 시나리오 × 15 스테이지)
- 잠금/해금 상태 스프라이트 표시
- 레벨 진행 관리
- 씬 전환 처리

**구조**:
```csharp
// 3x15 스테이지 버튼 그리드
// PlayerPrefs로 잠금 상태 저장: "Stage_cleared_{scenario}_{stage}"
```

#### Option.cs
**역할**: 옵션 메뉴 (오디오 설정)

**기능**:
- BGM/SFX 볼륨 슬라이더
- Audio Mixer 파라미터 조정
- PlayerPrefs에 설정 저장

---

## 게임플레이 메커니즘

### 수학 퍼즐 시스템

게임의 핵심 메커니즘은 수학 수식 완성입니다.

#### 1. 아이템 수집
플레이어가 맵을 이동하며 순서대로 아이템 수집:
- **패턴**: `숫자` → `연산자` → `숫자` → **자동 계산**

#### 2. 수식 구축
수집한 아이템이 수식 구조에 배치됨:
```
[Number] [Operator] [Number]
   ↓         ↓         ↓
   5         +         3    → 계산 결과: 8
```
- 최대 3개 슬롯 표시
- 3개 아이템 수집 시 자동 계산 실행
- 계산 결과가 다음 수식의 첫 번째 피연산자가 됨

#### 3. 지원 연산
- **덧셈**: `+`
- **뺄셈**: `-`
- **곱셈**: `*`
- **나눗셈**: `/`

#### 4. 목표
- **문(Door)**: 목표 값 표시
- **클리어 조건**: 계산 결과 == 문의 값
- 일치하면 스테이지 클리어

**예시**:
```
맵 레이아웃:
[플레이어] → [3] → [+] → [5] → [문:8]

진행:
1. 3 수집 → formula[0] = "3"
2. + 수집 → formula[1] = "+"
3. 5 수집 → formula[2] = "5" → 자동 계산 (3+5=8)
4. 문 도달 → 8 == 8 → 클리어!
```

---

### 이동 시스템

#### 입력 방식
1. **터치/마우스**: 드래그하여 방향 결정
   - `Util.TouchUtil.GetMouseDirection()` 사용
2. **키보드**: WASD 직접 이동

#### 이동 특성
- **그리드 기반**: 한 번에 1칸 이동
- **턴제**: 이동마다 턴 카운트 증가
- **충돌 감지**: Physics2D Raycast 사용

#### 상호작용 오브젝트
| 오브젝트 | 동작 |
|----------|------|
| **벽 (Walls)** | 이동 차단 |
| **박스 (Boxes)** | 플레이어가 밀 수 있음 (소코반 스타일) |
| **숫자/연산자** | 수집 시 수식에 추가 |
| **문 (Door)** | 수식 완성 후 값 일치 시 통과 |
| **함정 (Traps)** | 피해야 할 장애물 |
| **올연산자 (AllOperators)** | 특수 게이트 (조건부 통과) |
| **게이트 (Gates)** | 번호가 있는 특수 문 |

---

### Undo/되돌리기 시스템

#### 구현 방식
턴별로 게임 상태 스냅샷 저장:

```csharp
class revertObject
{
    public Vector2 playerPosition;              // 플레이어 위치
    public Dictionary<string, Vector2> objectPosition;  // 박스 위치들
    public Dictionary<int, string> formula;     // 수집한 수식
    public string calculateResult;              // 계산 결과
    // ... 기타 상태
}

Dictionary<int, revertObject> backUpRevert;  // 턴 번호 → 상태
```

#### 저장 시점
- 매 이동 전에 현재 상태 저장
- 턴 번호를 키로 사용

#### 복구 방법
- **Back 버튼** 클릭 시 이전 턴으로 복구
- 플레이어 위치, 박스 위치, 수식 상태 모두 복원

---

## 데이터 구조

### MapData (JSON 형식)

```json
{
  "Walls": [[1,1,1,0,0], [1,0,0,0,1], ...],           // 2D 배열, 1=벽, 0=빈공간
  "Numbers": [["","5","","",""], ["3","","","2",""], ...],  // 숫자 문자열
  "Operators": [["","+","","",""], ["","","","*",""], ...], // 연산자 (+,-,*,/)
  "Boxes": [[0,0,0,1,0], [0,1,0,0,0], ...],           // 1=박스 위치
  "AllOperators": [["","A_+_5","","",""], ...],       // 특수 게이트 (A_연산자_숫자)
  "Traps": [[0,0,1,0,0], ...],                        // 1=함정 위치
  "Gates": [["","G_1","","",""], ...],                // 게이트 (G_번호)
  "PlayerPosition": {"x": 0, "y": 4},                 // 플레이어 시작 위치
  "DoorPosition": {"x": 4, "y": 0},                   // 문 위치
  "DoorValue": 8                                      // 목표 값
}
```

### MapData C# 구조체

```csharp
[System.Serializable]
public class MapData
{
    public int[][] Walls;
    public string[][] Numbers;
    public string[][] Operators;
    public int[][] Boxes;
    public string[][] AllOperators;
    public int[][] Traps;
    public string[][] Gates;
    public Position PlayerPosition;
    public Position DoorPosition;
    public int DoorValue;
}

[System.Serializable]
public class Position
{
    public int x;
    public int y;
}
```

---

### ObjectData 컴포넌트

게임 오브젝트에 부착되는 데이터:

```csharp
public class ObjectData : MonoBehaviour
{
    public int num;              // 숫자 값 (숫자 아이템, 문에 사용)
    public string doorNum;       // 문 식별자
    public string oper;          // 연산자 문자열 (+, -, *, /)
    public bool boxTrigger;      // 박스가 밀리고 있는지 여부
    public bool boxStop;         // 박스가 장애물에 막혔는지
    public Vector2 boxMoveDir;   // 박스 이동 방향
}
```

---

### 레벨 파일 명명 규칙

**형식**: `SN_{시나리오}_ST_{스테이지}.json`

**예시**:
- `SN_1_ST_1.json`: 시나리오 1, 스테이지 1
- `SN_1_ST_16.json`: 시나리오 1, 스테이지 16

**범위**:
- 시나리오: 1~3 (현재 시나리오 1만 데이터 존재)
- 스테이지: 1~15 (시나리오 1은 16개까지 존재)

---

## 씬 구조

### 씬 흐름도

```
1.StartScene (메인 메뉴)
     ↓
     ├─ Option 버튼 → 오디오 설정
     └─ Start 버튼
           ↓
    MainScenes (스테이지 선택)
           ↓
           ├─ 3x15 그리드 버튼
           └─ 스테이지 선택
                 ↓
          2.StoryModeScene (게임플레이)
                 ↓
                 ├─ 맵 렌더링 (MapCreate)
                 ├─ 플레이어 제어 (Player)
                 ├─ 게임 로직 (GameManager)
                 └─ 클리어/실패
                       ↓
                 MainScenes (돌아가기)
```

---

### 씬별 상세 정보

#### 1. StartScene (씬 인덱스: 0)
**목적**: 게임 시작 화면

**주요 컴포넌트**:
- **GameStart 버튼**: `MainScenes`로 전환
- **Option 버튼**: 옵션 패널 표시
- **AudioSlider**: BGM/SFX 볼륨 조절
- **배경**: 타이틀 이미지

**스크립트**:
- `Option.cs`: 오디오 설정 관리

---

#### 2. MainScenes (씬 인덱스: 1)
**목적**: 스테이지 선택 화면

**주요 컴포넌트**:
- **스테이지 버튼 그리드**: 3 시나리오 × 15 스테이지 = 45개 버튼
- **잠금/해금 스프라이트**: PlayerPrefs 기반
- **Back 버튼**: `StartScene`으로 돌아가기

**스크립트**:
- `UiManager.cs`: 버튼 생성, 잠금 상태 관리

**레벨 해금 로직**:
```csharp
// PlayerPrefs 키: "Stage_cleared_{scenario}_{stage}"
// 0 = 잠김, 1 = 해금됨
```

---

#### 3. StoryModeScene (씬 인덱스: 2)
**목적**: 실제 게임플레이

**주요 컴포넌트**:
- **GameManager**: 게임 상태 관리
- **Player**: 플레이어 오브젝트
- **MapCreate**: 맵 생성 및 렌더링
- **UICanvas**: HUD (수식 표시, 턴 수, 버튼)
- **Main Camera**: 맵 크기에 따라 조정

**UI 요소**:
- 수식 디스플레이: `[Num] [Op] [Num]`
- 턴 카운터
- Undo 버튼
- Restart 버튼
- Back 버튼 (스테이지 선택으로)

**스크립트**:
- `GameManager.cs`
- `Player.cs`
- `MapCreate.cs`
- `TextManager.cs` (대화 출력)

---

#### 4. SampleScene
**목적**: Unity 기본 템플릿 씬 (사용 안 함)

---

#### 5. TestJoung
**목적**: 개발/테스트용 씬

---

## 기술 스택

### Unity 패키지 및 의존성

| 패키지 | 버전 | 용도 |
|--------|------|------|
| `com.unity.2d.sprite` | - | 2D 스프라이트 렌더링 |
| `com.unity.textmeshpro` | 3.0.6 | 리치 텍스트 렌더링 (UI, 대화) |
| `com.unity.ugui` | 1.0.0 | UI 시스템 (버튼, 슬라이더 등) |
| `com.unity.nuget.newtonsoft-json` | 3.2.1 | JSON 직렬화/역직렬화 |
| `com.unity.audio` | - | 오디오 시스템, Audio Mixer |
| `com.unity.physics2d` | - | 2D 물리, Raycast |

---

### 커스텀 유틸리티

#### Namespace: `Util.TouchUtil`
**파일**: `Util.cs`

**기능**: 크로스 플랫폼 터치/마우스 입력

**주요 메서드**:
```csharp
public static Vector2 GetMouseDirection()
public static bool IsTouchDown()
public static bool IsTouchUp()
```

**플랫폼 분기**:
```csharp
#if UNITY_EDITOR
    Input.GetMouseButtonDown(0)
#else
    Input.touchCount > 0
#endif
```

---

#### Namespace: `LittleRay`
**파일**: `RayEmission.cs`

**기능**: 2D Raycast 헬퍼 함수

**주요 메서드**:
```csharp
public static RaycastHit2D RayEmit(Vector2 pos, Vector2 dir, float distance, LayerMask layer)
```

---

### CSV 파싱
**파일**: `CSVReader.cs`

**기능**: CSV 파일을 2D 문자열 배열로 파싱

**사용처**:
- 맵 데이터 CSV → JSON 변환
- 대화 CSV 로드

---

### 데이터 파이프라인

```
┌─────────────────┐
│   MapData.csv   │ (사람이 작성 가능한 형식)
└────────┬────────┘
         ↓
┌─────────────────┐
│  CSVReader.cs   │ (CSV 파싱)
└────────┬────────┘
         ↓
┌─────────────────┐
│MakeJsonMapData  │ (JSON 변환)
└────────┬────────┘
         ↓
┌─────────────────┐
│  MapData.json   │ (Resources 폴더 저장)
└────────┬────────┘
         ↓
┌─────────────────┐
│  MapCreate.cs   │ (런타임 로드 및 렌더링)
└─────────────────┘
```

---

## 제약 사항 및 기술 노트

### 현재 제약 사항

1. **시나리오 1만 구현**:
   - 시나리오 2, 3은 코드에 참조되지만 레벨 데이터 없음
   - UI는 3개 시나리오 지원하도록 설계���

2. **한글 주석**:
   - 대부분의 코드 주석이 한글로 작성됨
   - 로직 설명 포함

3. **수동 씬 인덱스 사용**:
   - `SceneManager.LoadScene(0/1/2)` 형태
   - 씬 이름 대신 인덱스 사용

4. **PlayerPrefs 기반 저장**:
   - 스테이지 해금 상태만 저장
   - 게임 진행 데이터는 저장 안 됨
   - 클라우드 세이브 미지원

5. **고정 3칸 수식 UI**:
   - 수식은 최대 3개 요소만 표시
   - 계산 후 즉시 새 수식 시작

6. **CSV 기반 레벨 디자인**:
   - 레벨 데이터를 CSV로 작성
   - 런타임에 JSON으로 변환
   - 인게임 레벨 에디터 없음

---

### 성능 고려사항

- **오브젝트 풀링 미사용**: 맵 오브젝트를 매번 Instantiate/Destroy
- **Physics2D Raycast**: 매 프레임 충돌 체크
- **Dictionary 조회**: O(1) 상수 시간 복잡도로 효율적

---

### 확장 가능성

#### 쉽게 추가 가능한 기능:
1. **새 스테이지**: CSV 작성 → JSON 변환
2. **새 연산자**: ObjectData에 연산자 타입 추가
3. **새 오브젝트 타입**: MapData에 필드 추가
4. **사운드 추가**: SoundBox에 AudioClip 등록

#### 구조 변경이 필요한 기능:
1. **멀티플레이**: 네트워크 동기화 필요
2. **레벨 에디터**: 인게임 맵 편집 UI
3. **스토리 시스템**: 확장된 대화 시스템 필요
4. **세이브/로드**: 더 복잡한 저장 시스템 필요

---

## 디렉토리 구조 전체

```
LittleHacker/
├── Assets/
│   ├── Scripts/
│   │   ├── GameManager.cs
│   │   ├── Player.cs
│   │   ├── UiManager.cs
│   │   ├── Helper.cs
│   │   ├── Util.cs
│   │   ├── Option.cs
│   │   ├── CSVReader.cs
│   │   ├── RayEmission.cs
│   │   ├── Manager/
│   │   │   ├── Managers.cs
│   │   │   ├── Singleton.cs
│   │   │   ├── TextManager.cs
│   │   │   ├── SoundManager.cs
│   │   │   ├── SoundBox.cs
│   │   │   ├── TemporaySoundPlayer.cs
│   │   │   └── CoroutineHelper.cs
│   │   └── MapCreate/
│   │       ├── MapCreate.cs
│   │       ├── ObjectData.cs
│   │       └── MakeJsonMapData.cs
│   ├── Resources/
│   │   ├── MapDatasJSON/
│   │   │   ├── SN_1_ST_1.json
│   │   │   ├── ...
│   │   │   └── SN_1_ST_16.json
│   │   ├── MapDatasCSV/
│   │   │   ├── SN_1_ST_1.csv
│   │   │   ├── ...
│   │   │   ├── SN_1_ST_16.csv
│   │   │   ├── SN_1.csv
│   │   │   └── SN_1_Clear.csv
│   │   └── Sound/
│   │       ├── BGM/
│   │       └── SFX/
│   ├── Scenes/
│   │   ├── 1.StartScene.unity
│   │   ├── MainScenes.unity
│   │   ├── 2.StoryModeScene.unity
│   │   ├── SampleScene.unity
│   │   └── TestJoung.unity
│   ├── Prefabs/
│   │   ├── Wall.prefab
│   │   ├── Number.prefab
│   │   ├── Operator.prefab
│   │   ├── Box.prefab
│   │   ├── Door.prefab
│   │   ├── Player.prefab
│   │   └── ...
│   └── Sprites/
│       └── ... (게임 이미지 리소스)
├── ProjectSettings/
└── Packages/
```

---

## 주요 코드 플로우

### 게임 시작 플로우

```
앱 실행
  ↓
1.StartScene 로드
  ↓
GameManager.Awake() → 싱글톤 초기화
Managers.Init() → TextManager, SoundManager 초기화
  ↓
사용자: "Start" 버튼 클릭
  ↓
MainScenes 로드 (씬 인덱스 1)
  ↓
UiManager.Start()
  - PlayerPrefs에서 해금 상태 로드
  - 3x15 스테이지 버튼 생성
  - 잠금/해금 스프라이트 설정
  ↓
사용자: 스테이지 버튼 클릭
  ↓
GameManager.nowScenario = 선택한 시나리오
GameManager.nowStage = 선택한 스테이지
  ↓
2.StoryModeScene 로드 (씬 인덱스 2)
```

---

### 게임플레이 플로우

```
2.StoryModeScene 로드
  ↓
MapCreate.Start()
  ↓
1. JSON 로드: Resources.Load($"MapDatasJSON/SN_{scenario}_ST_{stage}")
  ↓
2. MapData 파싱: JsonUtility.FromJson<MapData>(json)
  ↓
3. 맵 렌더링:
   - Walls 배열 순회 → Wall 프리팹 생성
   - Numbers 배열 순회 → Number 프리팹 생성
   - Operators 배열 순회 → Operator 프리팹 생성
   - Boxes, Traps, Gates 등 동일
   - Player 위치에 플레이어 생성
   - Door 위치에 문 생성 (DoorValue 설정)
  ↓
4. 카메라 크기 조정
  ↓
Player.Update() 시작 (매 프레임)
  ↓
입력 감지:
  - WASD 키 또는
  - 터치 드래그 방향
  ↓
이동 시도:
  - Raycast로 다음 칸 확인
  - 충돌 체크 (벽/박스/아이템/문)
  ↓
아이템 수집:
  - 숫자/연산자 충돌 시
  - formula Dictionary에 추가
  - UI 업데이트
  ↓
계산 실행 (formula.Count == 3일 때):
  - int.Parse(formula[0])
  - 연산자 formula[1]
  - int.Parse(formula[2])
  - switch문으로 계산
  - 결과를 새 formula[0]에 저장
  ↓
문 도달:
  - 계산 결과 == DoorValue?
  - YES → 스테이지 클리어
  - NO → 계속 진행
  ↓
클리어 시:
  - PlayerPrefs에 저장
  - TextManager로 클리어 대화 표시
  - MainScenes로 복귀
```

---

### Undo 플로우

```
Player.Update()
  ↓
이동 직전:
  revertObject backup = new revertObject();
  backup.playerPosition = 현재 위치
  backup.objectPosition = 모든 박스 위치
  backup.formula = 현재 수식
  backUpRevert[playerTurn] = backup;
  ↓
이동 실행
  ↓
playerTurn++;
  ↓
사용자: "Back" 버튼 클릭
  ↓
playerTurn--;
revertObject restore = backUpRevert[playerTurn];
  ↓
플레이어 위치 복원
박스들 위치 복원
수식 복원
UI 업데이트
```

---

## 요약

**Little Hacker**는 교육적 수학 퍼즐 게임으로, 다음과 같은 특징을 가집니다:

### 핵심 강점:
- 명확한 MVC + Manager 아키텍처
- 확장 가능한 싱글톤 패턴
- CSV 기반 쉬운 레벨 디자인
- 턴 기반 Undo 시스템
- 크로스 플랫폼 입력 지원

### 개선 가능 영역:
- 오브젝트 풀링으로 성능 최적화
- 시나리오 2, 3 레벨 데이터 추가
- 클라우드 세이브 지원
- 인게임 레벨 에디터

### 기술적 하이라이트:
- **18개 C# 스크립트**: 명확한 역할 분리
- **제네릭 싱글톤**: 재사용 가능한 베이스 클래스
- **Dictionary 기반 상태 관리**: 효율적인 데이터 구조
- **Raycast 충돌 감지**: 정확한 그리드 이동
- **JSON 직렬화**: Newtonsoft.Json 활용

---

**문서 버전**: 1.0
**마지막 업데이트**: 2025-10-20
**작성자**: Claude Code Agent
