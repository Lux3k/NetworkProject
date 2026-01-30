
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    [Header("Bullet Pool")]
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private int _poolSize = 100;

    [Header("Pattern Database")]
    [SerializeField] private List<BulletPatternSO> patternDatabase;

    // 총알 풀링
    [SerializeField]private List<Bullet> _allActiveBullets = new List<Bullet>(500);
    private Stack<Bullet> _bulletPool = new Stack<Bullet>(100);

    // 그룹 관리
    private List<BulletGroupData> _activeGroups = new List<BulletGroupData>(50);
    private int _nextGroupID = 0;

    // 패턴 Dictionary
    private Dictionary<int, BulletPatternSO> _patternDict; 

    private void Awake()
    {
        Initialize(); 
    }

    private void Initialize()
    {
        if ( _patternDict != null) return;
        _patternDict = new Dictionary<int, BulletPatternSO>();
        if (patternDatabase != null)
        {
            foreach (var pattern in patternDatabase)
            {
                if (pattern != null && pattern.patternID != 0)
                    _patternDict[pattern.patternID] = pattern;
            }
        }

        if (_bulletPool == null)
        {
            _bulletPool = new Stack<Bullet>(_poolSize);
            _allActiveBullets = new List<Bullet>(_poolSize);

            for (int i = 0; i < _poolSize; i++)
            {
                if (_bulletPrefab != null)
                {
                    Bullet newBullet = Instantiate(_bulletPrefab);
                    newBullet.gameObject.SetActive(false);
                    _bulletPool.Push(newBullet);
                }
            }
        }

        Debug.Log("BulletManager Initialized");
    }

    // Manager가 업데이트 처리
    private void Update()
    {
        UpdateGroupPhases();
        UpdateAllBullets();
    }

    private void UpdateGroupPhases()
    {
        float deltaTime = Time.deltaTime;

        for (int i = _activeGroups.Count - 1; i >= 0; i--)
        {
            var group = _activeGroups[i];

            if (group.bulletCount <= 0)
            {
                RemoveGroupAt(i);
                continue;
            }
            int phaseCount;
            float currentPhaseDuration;

            if (group.pattern != null)
            {
                if (group.pattern.phases == null || group.pattern.phases.Length == 0)
                    continue;
                phaseCount = group.pattern.phases.Length;
                currentPhaseDuration = group.pattern.phases[group.currentPhaseIndex].duration;
            }
            else
            {
                if (group.patternData.phases == null || group.patternData.phases.Count == 0)
                    continue;
                phaseCount = group.patternData.phases.Count;
                currentPhaseDuration = group.patternData.phases[group.currentPhaseIndex].duration;
            }

            group.phaseTimer += deltaTime;

            if (group.currentPhaseIndex < phaseCount - 1)
            {
                if (group.phaseTimer >= currentPhaseDuration)
                {
                    group.currentPhaseIndex++;
                    group.phaseTimer = 0f;
                    ChangeGroupStrategy(group);
                }
            }
        }
    }
    //모든 총알을 단일 루프로 처리
    private void UpdateAllBullets()
    {
        for (int i = _allActiveBullets.Count - 1; i >= 0; i--)
        {
            var bullet = _allActiveBullets[i];
            if (bullet.gameObject.activeSelf)
            {
                bullet.Move();
            }
            else
            {
                _allActiveBullets.RemoveAt(i);
            }
        }
    }
    private void ChangeGroupStrategy(BulletGroupData group)
    {
        IBulletStrategy newStrategy;

        if (group.pattern != null)
        {
            newStrategy = group.pattern.phases[group.currentPhaseIndex].strategy;
        }
        else
        {
            var phase = group.patternData.phases[group.currentPhaseIndex];
            newStrategy = DataManager.Instance.GetStrategy(phase);
        }

        int groupID = group.groupID;
        int foundCount = 0;
        int expectedCount = group.bulletCount;

        for (int i = _allActiveBullets.Count - 1; i >= 0; i--)
        {
            var bullet = _allActiveBullets[i];
            if (bullet.GroupID == groupID)
            {
                bullet.SetStrategy(newStrategy);
                foundCount++;
                if (foundCount >= expectedCount)
                    break;
            }
        }
    }

    public BulletPatternSO GetPattern(int patternID)
    {

        if (_patternDict == null)
        {
            Initialize();
        }

        return _patternDict[patternID];
    }

    // 그룹 생성
    public int CreateGroup(BulletPatternSO pattern)
    {
        int groupID = _nextGroupID++;

        var groupData = new BulletGroupData
        {
            groupID = groupID,
            pattern = pattern,
            currentPhaseIndex = 0,
            phaseTimer = 0f,
            bulletCount = 0
        };

        _activeGroups.Add(groupData);
        return groupID;
    }
    public int CreateGroup(PatternData pattern)
    {
        int groupID = _nextGroupID++;

        var groupData = new BulletGroupData
        {
            groupID = groupID,
            patternData = pattern,
            currentPhaseIndex = 0,
            phaseTimer = 0f,
            bulletCount = 0
        };

        _activeGroups.Add(groupData);
        return groupID;
    }
    public Bullet GetBullet(Vector2 pos, Vector2 dir, BulletType bulletType,
                       Color bulletColor, int groupID, int ownerPhotonViewID,
                       BulletMoveStrategyBase initialStrategy)
    {
        Bullet bullet;

        if(_bulletPool.Count > 0)
        {
            bullet = _bulletPool.Pop();
        }
        else
        {
            bullet = Instantiate(_bulletPrefab);
        }

        bullet.Initialize(pos, dir, bulletType, bulletColor, groupID, ownerPhotonViewID);

        if (initialStrategy != null)
        {
            bullet.SetStrategy(initialStrategy);
        }

        bullet.gameObject.SetActive(true);
        _allActiveBullets.Add(bullet);

        var group = GetGroupByID(groupID);
        if (group != null)
            group.bulletCount++;

        return bullet;
    }

    public Bullet GetBullet(Vector2 pos, Vector2 dir, BulletType bulletType,
                       Color bulletColor, int groupID, int ownerPhotonViewID,
                       IBulletStrategy initialStrategy)
    {
        Bullet bullet;

        if (_bulletPool.Count > 0)
            bullet = _bulletPool.Pop();
        else
            bullet = Instantiate(_bulletPrefab);

        bullet.Initialize(pos, dir, bulletType, bulletColor, groupID, ownerPhotonViewID);

        if (initialStrategy != null)
            bullet.SetStrategy(initialStrategy);

        bullet.gameObject.SetActive(true);
        _allActiveBullets.Add(bullet);

        var group = GetGroupByID(groupID);
        if (group != null)
            group.bulletCount++;

        return bullet;
    }

    public void ReturnBullet(Bullet bullet)
    {
        if (!bullet.gameObject.activeSelf) return;

        var group = GetGroupByID(bullet.GroupID);
        if (group != null)
            group.bulletCount--;

        bullet.gameObject.SetActive(false);
        _bulletPool.Push(bullet);

        if (_allActiveBullets.Contains(bullet))
        {
            _allActiveBullets.Remove(bullet);
        }
    }


    private BulletGroupData GetGroupByID(int groupID)
    {
        return _activeGroups.Find(g => g.groupID == groupID);
    }

    private void RemoveGroupAt(int index)
    {
        int lastIndex = _activeGroups.Count - 1;
        _activeGroups[index] = _activeGroups[lastIndex];
        _activeGroups.RemoveAt(lastIndex);
    }
}