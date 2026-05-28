# Photon 기반 탄막 슈팅 프로토타입

Photon PUN을 사용해 만든 2D PvE 탄막 슈팅 프로토타입입니다.

이 프로젝트는 완성형 게임보다는, **네트워크 환경에서 대량 객체를 어떻게 관리할지**와 **클라이언트 관점에서 Photon의 로비/룸/RPC/씬 동기화 흐름을 익히는 것**을 목표로 진행한 개인 프로젝트입니다.

## 프로젝트 정보

| 항목 | 내용 |
| --- | --- |
| 프로젝트 | Photon 기반 탄막 슈팅 프로토타입 |
| 형태 | 개인 프로젝트 |
| 개발 기간 | 2026.01 - 2026.02, 약 3주 |
| 엔진 | Unity 2022.3.62f3 |
| 네트워크 | Photon PUN |
| 주요 관심사 | PvE 이벤트 동기화, 대량 탄환/몬스터 관리, 데이터 기반 탄막 패턴 |

프로젝트 본문은 [260105](260105) 폴더에 있습니다.

## 구현 요약

- Photon 기반 서버 접속, 로비 입장, 룸 생성/입장, 네트워크 씬 전환 구현
- 마스터 클라이언트가 스테이지/웨이브 진행과 몬스터 스폰을 주도하는 구조 구현
- 몬스터마다 네트워크 객체를 두기보다, 매니저가 RPC 진입점 역할을 맡는 이벤트 동기화 구조 구성
- 몬스터 스폰, 공격, 피격, 사망을 `uniqueID` 기반으로 동기화
- 탄막을 `Pattern -> Phase -> Strategy` 구조로 분리해, 진행 중 이동 방식이 바뀌는 패턴 구현
- ScriptableObject 테스트 구조와 CSV/Google Sheet 기반 실제 데이터 구조를 병행
- Google Sheet 접근 실패 시 로컬 CSV로 fallback하는 데이터 로딩 구조 구현
- 탄환과 몬스터에 오브젝트 풀링 적용

## 핵심 설계 방향

이 프로젝트는 PvP가 아닌 **PvE 협동 플레이**를 전제로 했습니다.

그래서 모든 몬스터의 위치를 매 프레임 정밀하게 동기화하기보다, 게임 진행에 중요한 이벤트를 맞추는 데 우선순위를 두었습니다. 몬스터 스폰, 공격, 피격, 사망 같은 결과 중심 이벤트는 RPC로 동기화하고, 각 클라이언트에서는 로컬 이동을 어느 정도 허용하는 방향을 선택했습니다.

이 선택은 네트워크 비용과 구현 범위를 줄이기 위한 판단이었습니다. 대량 몬스터를 모두 `PhotonView` 기반 네트워크 객체로 관리하면 동기화 비용과 관리 복잡도가 커질 수 있다고 보았고, 클라이언트 포트폴리오 범위에서는 주요 이벤트 흐름을 안정적으로 맞추는 데 집중했습니다.

## 네트워크 흐름

Photon 연결과 씬 전환은 [NetworkManager.cs](260105/Assets/Scripts/Photon/NetworkManager.cs)에서 관리합니다.

주요 흐름은 다음과 같습니다.

1. Title에서 Photon 서버 접속
2. 접속 성공 시 Lobby 씬으로 이동
3. Lobby에서 룸 생성, 룸 입장, 랜덤 룸 입장 처리
4. Room에서 마스터 클라이언트만 게임 시작 가능
5. InGame 씬은 Photon의 네트워크 씬 전환으로 동기화

관련 코드:

- [NetworkManager.cs](260105/Assets/Scripts/Photon/NetworkManager.cs)
- [LobbyUI.cs](260105/Assets/Scripts/Photon/LobbyUI.cs)
- [RoomUI.cs](260105/Assets/Scripts/Photon/RoomUI.cs)
- [SceneLoader.cs](260105/Assets/Scripts/Common/SceneLoader.cs)

## 마스터 클라이언트 기반 진행

스테이지와 웨이브 진행은 [StageManager.cs](260105/Assets/Scripts/Stage/StageManager.cs)가 담당합니다.

클라이언트마다 스테이지 상태가 다르게 진행되면 게임 흐름이 어긋날 수 있으므로, 마스터 클라이언트가 스테이지 시작과 다음 스테이지 진행을 요청하도록 했습니다. 실제 시작 이벤트는 RPC를 통해 모든 클라이언트에 전달됩니다.

```csharp
public void RequestStartStage(int stageID)
{
    photonView.RPC(nameof(RPC_StartStage), RpcTarget.All, stageID);
}
```

이 구조를 통해 모든 클라이언트가 같은 스테이지와 웨이브 순서를 공유하도록 했습니다.

## 몬스터 동기화 구조

몬스터는 개별 `PhotonView` 중심으로 관리하기보다, [MonsterManager.cs](260105/Assets/Scripts/Monster/MonsterManager.cs)가 네트워크 이벤트의 진입점 역할을 하도록 구성했습니다.

역할은 다음처럼 나누었습니다.

| 클래스 | 역할 |
| --- | --- |
| [MonsterManager.cs](260105/Assets/Scripts/Monster/MonsterManager.cs) | RPC 수신, `uniqueID` 기반 활성 몬스터 Dictionary 관리, 스폰/피격/공격/사망 이벤트 동기화 |
| [MonsterSpawner.cs](260105/Assets/Scripts/Monster/MonsterSpawner.cs) | 몬스터 풀링, 실제 생성/반납, 스폰 위치 계산 |
| [MonsterController.cs](260105/Assets/Scripts/Monster/MonsterController.cs) | 몬스터 이동, 타겟 탐색, 공격 루틴, 체력 처리 |

이렇게 나눈 이유는 **네트워크 동기화 책임과 로컬 오브젝트 관리 책임을 분리**하기 위해서입니다. RPC 처리와 오브젝트 풀링 로직이 한 클래스에 섞이면 흐름을 추적하기 어려워질 수 있어, 네트워크 이벤트는 `MonsterManager`, 실제 오브젝트 관리는 `MonsterSpawner`가 담당하도록 했습니다.

몬스터는 `uniqueID`로 식별합니다. 이를 통해 각 클라이언트가 같은 논리적 몬스터를 가리킬 수 있고, 스폰/피격/사망 이벤트를 동일한 ID 기준으로 처리할 수 있습니다.

## 탄막 패턴 구조

이 프로젝트에서 가장 중점적으로 구현한 부분은 탄막 패턴 구조입니다.

탄막은 다음 흐름으로 구성했습니다.

```text
WeaponData
  -> PatternData
      -> PhaseData[]
          -> IBulletStrategy
```

관련 코드:

- [BulletShooter.cs](260105/Assets/Scripts/Bullet/BulletShooter.cs)
- [BulletManager.cs](260105/Assets/Scripts/Bullet/BulletManager.cs)
- [BulletGroup.cs](260105/Assets/Scripts/Bullet/BulletGroup.cs)
- [Bullet.cs](260105/Assets/Scripts/Bullet/Bullet.cs)
- [IBulletStrategy.cs](260105/Assets/Scripts/Bullet/BulletMove/IBulletStrategy.cs)

### Phase를 둔 이유

탄막 슈팅에서는 탄환이 발사된 뒤에도 중간에 움직임이 바뀌는 기믹이 자주 등장합니다.

예를 들어 다음과 같은 패턴을 표현하고 싶었습니다.

- 처음에는 직선으로 이동
- 일정 시간 후 정지
- 다시 곡선으로 회전하며 이동
- 여러 탄환 그룹이 시간차를 두고 회전 발사

이를 위해 탄막 패턴을 하나의 이동 방식으로 고정하지 않고, 시간 구간을 의미하는 `Phase`로 나누었습니다. 각 Phase는 이동 전략을 가지고 있고, [BulletGroup.cs](260105/Assets/Scripts/Bullet/BulletGroup.cs)이 시간이 지나면 그룹 내 탄환들의 전략을 교체합니다.

```csharp
private void NextPhase()
{
    _currentPhaseIndex++;
    _phaseTimer = 0f;

    IBulletStrategy newStrategy = null;

    if (PatternSO != null) newStrategy = PatternSO.phases[_currentPhaseIndex].strategy;
    else if (PatternData != null) newStrategy = DataManager.Instance.GetStrategy(PatternData.phases[_currentPhaseIndex]);

    if (newStrategy == null) return;

    for (int i = MyBullets.Count - 1; i >= 0; i--)
    {
        if (MyBullets[i].gameObject.activeSelf)
            MyBullets[i].SetStrategy(newStrategy);
    }
}
```

### 전략 패턴 기반 이동

탄환 이동은 [IBulletStrategy.cs](260105/Assets/Scripts/Bullet/BulletMove/IBulletStrategy.cs)를 기준으로 분리했습니다.

현재 구현된 전략은 다음과 같습니다.

| 전략 | 역할 |
| --- | --- |
| [StraightStrategy.cs](260105/Assets/Scripts/Bullet/BulletMove/MoveStrategy/StraightStrategy.cs) | 직선 이동, 가속, 최고 속도 처리 |
| [CurveStrategy.cs](260105/Assets/Scripts/Bullet/BulletMove/MoveStrategy/CurveStrategy.cs) | 진행 방향을 회전시키며 곡선 이동 |
| [StopStrategy.cs](260105/Assets/Scripts/Bullet/BulletMove/MoveStrategy/StopStrategy.cs) | 탄환 정지 |

새로운 이동 패턴이 필요할 때 `IBulletStrategy` 구현체를 추가하면 되도록 구성했습니다. 이 구조는 탄환 발사 코드가 구체적인 이동 방식을 직접 알지 않아도 된다는 장점이 있습니다.

## 데이터 기반 탄막 구성

이전 작업에서 ScriptableObject만으로 패턴 데이터를 관리할 때, 반복 수정과 외부 데이터 관리가 불편하다고 느꼈습니다. 그래서 이 프로젝트에서는 CSV/Google Sheet 기반 데이터 구조를 시도했습니다.

[DataManager.cs](260105/Assets/Scripts/Data/DataManager.cs)는 Google Sheet URL이 있으면 먼저 시트 데이터를 받아오고, 실패하면 로컬 CSV를 읽습니다.

```csharp
IEnumerator LoadCSV(string sheetURL, string localPath, Action<string> parser)
{
    if (!string.IsNullOrEmpty(sheetURL))
    {
        using var request = UnityWebRequest.Get(sheetURL);
        request.timeout = 5;
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            parser(request.downloadHandler.text);
            yield break;
        }
    }

    var csv = Resources.Load<TextAsset>(localPath);
    if (csv != null)
    {
        parser(csv.text);
    }
}
```

시트 접근에 실패해도 로컬 CSV로 테스트할 수 있게 하기 위한 구조입니다. 실제 플레이 환경에서는 클라이언트마다 데이터가 달라지면 안 되므로, 빌드에 포함된 CSV를 기준으로 동작하는 것이 더 안정적이라고 판단했습니다.

데이터 클래스:

- [PatternData.cs](260105/Assets/Scripts/Data/PatternData.cs)
- [PhaseData.cs](260105/Assets/Scripts/Data/PhaseData.cs)
- [WeaponData.cs](260105/Assets/Scripts/Data/WeaponData.cs)
- [WaveData.cs](260105/Assets/Scripts/Stage/WaveData.cs)
- [StageData.cs](260105/Assets/Scripts/Stage/StageData.cs)
- [MonSterData.cs](260105/Assets/Scripts/Monster/MonSterData.cs)

## 오브젝트 풀링

탄환과 몬스터는 전투 중 많이 생성되고 사라지는 객체입니다.

탄환은 [BulletManager.cs](260105/Assets/Scripts/Bullet/BulletManager.cs)에서 풀을 관리하고, 비활성화된 탄환을 다시 풀로 돌려 재사용합니다. 몬스터는 [MonsterSpawner.cs](260105/Assets/Scripts/Monster/MonsterSpawner.cs)에서 풀을 관리합니다.

대량 객체를 다루는 네트워크 프로토타입이었기 때문에, `Instantiate`/`Destroy` 호출을 줄이고 오브젝트 재사용 흐름을 직접 구현하는 데 초점을 두었습니다.

## 구현하며 신경 쓴 점

### 네트워크 비용과 구현 범위

PvE 게임에서는 PvP처럼 모든 위치와 판정을 완전히 동일하게 맞추는 것보다, 게임 진행에 중요한 이벤트가 일관되게 공유되는 것이 더 중요하다고 보았습니다.

그래서 몬스터마다 개별 네트워크 동기화를 걸기보다, 매니저가 주요 이벤트를 RPC로 전달하는 구조를 선택했습니다. 이 선택은 대량 객체 관리 비용을 줄이면서 Photon의 핵심 흐름을 익히기 위한 프로토타입 목적과도 맞았습니다.

### 데이터 수정 편의성

탄막 패턴은 수치 조정이 잦은 영역입니다. ScriptableObject만으로 관리하면 반복 수정이 불편할 수 있다고 느껴, CSV/Google Sheet 기반 구조를 시도했습니다.

특히 `Pattern`과 `Phase`를 나누어 탄환이 시간에 따라 이동 방식을 바꿀 수 있게 한 것이 핵심입니다.

### 대량 탄환 관리

탄환은 개별 `Update`에 모든 책임을 두지 않고, `BulletManager`가 활성 탄환 리스트를 순회하며 이동을 처리합니다. 비활성화된 탄환은 풀로 반환하고, 그룹 단위로 Phase를 관리해 탄막 이동 패턴을 바꿉니다.

## 개선 가능 지점

- 현재 구조는 PvE 프로토타입 기준으로 주요 이벤트 동기화에 집중했기 때문에, 정밀한 위치 보정은 부족합니다. 더 정교한 동기화가 필요한 게임이라면 마스터 기준 위치/상태 보정이 필요합니다.
- 탄환 owner 판정과 RPC 전달 흐름은 더 엄격하게 정리할 여지가 있습니다. 발사 주체 정보를 명확히 전달하고, 각 클라이언트에서 동일하게 해석되도록 구조를 개선할 수 있습니다.
- 마스터 클라이언트 교체 시 UI와 진행 상태 갱신을 더 안정적으로 처리할 필요가 있습니다.
- CSV 파싱은 단순 문자열 분리 기반이므로, 잘못된 ID나 누락된 참조를 사전에 검증하는 도구가 있으면 안정성이 높아질 수 있습니다.
- ScriptableObject 테스트 데이터와 CSV 실제 데이터가 병행되어 있어, 장기적으로는 하나의 데이터 파이프라인으로 정리하는 편이 좋습니다.

## 주요 코드 링크

| 영역 | 링크 |
| --- | --- |
| Photon 연결/씬 전환 | [NetworkManager.cs](260105/Assets/Scripts/Photon/NetworkManager.cs) |
| 로비 UI | [LobbyUI.cs](260105/Assets/Scripts/Photon/LobbyUI.cs) |
| 룸 UI | [RoomUI.cs](260105/Assets/Scripts/Photon/RoomUI.cs) |
| 게임 진행 | [GameManager.cs](260105/Assets/Scripts/Common/GameManager.cs) |
| 스테이지 진행 | [StageManager.cs](260105/Assets/Scripts/Stage/StageManager.cs) |
| 몬스터 네트워크 이벤트 | [MonsterManager.cs](260105/Assets/Scripts/Monster/MonsterManager.cs) |
| 몬스터 스폰/풀링 | [MonsterSpawner.cs](260105/Assets/Scripts/Monster/MonsterSpawner.cs) |
| 탄막 발사 | [BulletShooter.cs](260105/Assets/Scripts/Bullet/BulletShooter.cs) |
| 탄환 풀링/관리 | [BulletManager.cs](260105/Assets/Scripts/Bullet/BulletManager.cs) |
| 탄환 그룹/Phase 전환 | [BulletGroup.cs](260105/Assets/Scripts/Bullet/BulletGroup.cs) |
| 탄환 이동 전략 | [IBulletStrategy.cs](260105/Assets/Scripts/Bullet/BulletMove/IBulletStrategy.cs) |
| 데이터 로딩 | [DataManager.cs](260105/Assets/Scripts/Data/DataManager.cs) |
