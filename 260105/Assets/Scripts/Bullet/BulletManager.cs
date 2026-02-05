
using System.Collections.Generic;
using UnityEngine;

public class BulletManager : MonoBehaviour
{
    [Header("Bullet Pool")]
    [SerializeField] private Bullet _bulletPrefab;
    [SerializeField] private int _poolSize = 100;

    [Header("Pattern Database")]
    [SerializeField] private List<BulletPatternSO> _patternDatabase;

    // 총알 풀링
    [SerializeField]private List<Bullet> _allActiveBullets = new List<Bullet>();
    private Stack<Bullet> BulletPool = new Stack<Bullet>();

    // 그룹 관리
    private List<BulletGroup> _activeGroups = new List<BulletGroup>();
    private Stack<BulletGroup> _groupPool = new Stack<BulletGroup>();
    private int _nextGroupID = 0;

    private Dictionary<int, BulletGroup> _groupDict = new Dictionary<int, BulletGroup>();

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
        if (_patternDatabase != null)
        {
            foreach (var pattern in _patternDatabase)
            {
                if (pattern != null && pattern.patternID != 0)
                    _patternDict[pattern.patternID] = pattern;
            }
        }

        if (BulletPool.Count == 0)
        {
            BulletPool = new Stack<Bullet>(_poolSize);
            _allActiveBullets = new List<Bullet>(_poolSize);
            _groupPool = new Stack<BulletGroup>(_poolSize);
            _groupDict = new Dictionary<int, BulletGroup>(_poolSize);

            for (int i = 0; i < _poolSize; i++)
            {
                if (_bulletPrefab != null)
                {
                    Bullet newBullet = Instantiate(_bulletPrefab);
                    newBullet.gameObject.SetActive(false);
                    BulletPool.Push(newBullet);
                }
            }
        }

    }

    // Manager가 업데이트 처리
    private void Update()
    {
        UpdateGroupPhases();
        UpdateAllBullets();
    }

    private void UpdateGroupPhases()
    {
        float dt = Time.deltaTime;

        for (int i = _activeGroups.Count - 1; i >= 0; i--)
        {
            BulletGroup group = _activeGroups[i];

            group.Update(dt);

            if (group.MyBullets.Count == 0)
            {
                RemoveGroupAt(i);
            }
        }
    }
    //모든 총알을 단일 루프로 처리
    private void UpdateAllBullets()
    {
        for (int i = _allActiveBullets.Count - 1; i >= 0; i--)
        {
            Bullet bullet = _allActiveBullets[i];

            if (!bullet.gameObject.activeSelf)
            {
                BulletPool.Push(bullet);

                int lastIndex = _allActiveBullets.Count - 1;
                _allActiveBullets[i] = _allActiveBullets[lastIndex];
                _allActiveBullets.RemoveAt(lastIndex);
            }
            else
                bullet.Move();
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
        BulletGroup group = GetGroupFromPool();

        group.Initialize(groupID, pattern); 

        _activeGroups.Add(group);
        _groupDict.Add(groupID, group);
        return groupID;
    }
    public int CreateGroup(PatternData pattern)
    {
        int groupID = _nextGroupID++;
        BulletGroup group = GetGroupFromPool(); 

        group.Initialize(groupID, pattern); 

        _activeGroups.Add(group);
        _groupDict.Add(groupID, group);
        return groupID;
    }
    private BulletGroup GetGroupFromPool()
    {
        if (_groupPool.Count > 0) 
            return _groupPool.Pop();
        return new BulletGroup();
    }
    //public Bullet GetBullet(Vector2 pos, Vector2 direction, BulletType bulletType,
    //                   Color bulletColor, int groupID, int ownerID,
    //                   BulletMoveStrategyBase initialStrategy)
    //{
    //    Bullet bullet;

    //    if(_bulletPool.Count > 0)
    //    {
    //        bullet = _bulletPool.Pop();
    //    }
    //    else
    //    {
    //        bullet = Instantiate(_bulletPrefab);
    //    }

    //    bullet.Initialize(pos, direction, bulletType, bulletColor, groupID, ownerID);

    //    if (initialStrategy != null)
    //    {
    //        bullet.SetStrategy(initialStrategy);
    //    }

    //    bullet.gameObject.SetActive(true);
    //    _allActiveBullets.Add(bullet);

    //    var group = GetGroupByID(groupID);
    //    if (group != null)
    //        group.bulletCount++;

    //    return bullet;
    //}

    public Bullet GetBullet(Vector2 pos, Vector2 direction, BulletType bulletType,
                        Color bulletColor, int groupID, int ownerID,
                        IBulletStrategy initialStrategy)
    {
        Bullet bullet;

        if (BulletPool.Count > 0)
            bullet = BulletPool.Pop();
        else
            bullet = Instantiate(_bulletPrefab);

        bullet.Initialize(pos, direction, bulletType, bulletColor, groupID, ownerID);

        if (initialStrategy != null)
            bullet.SetStrategy(initialStrategy);

        bullet.gameObject.SetActive(true);
        _allActiveBullets.Add(bullet);

        if (_groupDict.TryGetValue(groupID, out var group))
        {
            group.AddBullet(bullet);
        }

        return bullet;
    }


    private BulletGroup GetGroupByID(int groupID)
    {
        if (_groupDict.TryGetValue(groupID, out var group)) 
            return group;
        return null;
    }

    private void RemoveGroupAt(int index)
    {
        BulletGroup group = _activeGroups[index];

        _groupDict.Remove(group.GroupID); 
        group.Reset(); 
        _groupPool.Push(group); 

        int lastIndex = _activeGroups.Count - 1;
        _activeGroups[index] = _activeGroups[lastIndex];
        _activeGroups.RemoveAt(lastIndex);
    }
}