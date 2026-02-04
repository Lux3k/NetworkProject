using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MonsterController : MonoBehaviour, IDamagable
{
    private MonsterData _data;
    private float _currentHP;
    private BulletShooter _shooter;
    private Rigidbody2D _rb;
    private Transform _target;

    private IMonsterCombatNetwork _network;
    public int UniqueID { get; private set; }

    [Header("UI")]
    [SerializeField] private Slider _hpSlider;

    private void Awake()
    {
        _shooter = GetComponent<BulletShooter>();
        _rb = GetComponent<Rigidbody2D>();
    }

    public void Initialize(int monsterID, int uniqueID, IMonsterCombatNetwork network)
    {
        UniqueID = uniqueID;
        _network = network;
        _data = DataManager.Instance.GetMonster(monsterID);
        _currentHP = _data.maxHP;
        _target = null;
        //UpdateUI();
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private void FixedUpdate()
    {
        FindNearestTarget();
        MoveToTarget();
    }

    private void FindNearestTarget()
    {
        if (_target != null && _target.gameObject.activeInHierarchy) return;
        _target = null;

        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        float minDist = float.MaxValue;
        foreach (var p in players)
        {
            float dist = Vector2.Distance(transform.position, p.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                _target = p.transform;
            }
        }
    }

    private void MoveToTarget()
    {
        if (_target == null || _data == null) return;

        Vector2 dir = ((Vector2)_target.position - _rb.position).normalized;
        _rb.MovePosition(_rb.position + dir * _data.moveSpeed * Time.fixedDeltaTime);

        if (dir.x != 0)
            transform.localScale = new Vector3(dir.x < 0 ? 1 : -1, 1, 1);
    }

    private IEnumerator AttackRoutine()
    {
        while (_currentHP > 0)
        {
            yield return new WaitForSeconds(_data.attackSpeed);

            if (_target != null && _data.attackPatternIDs.Length > 0)
            {
                int randomID = _data.attackPatternIDs[Random.Range(0, _data.attackPatternIDs.Length)];
                _network.OnMonsterAttack(UniqueID, randomID, _target.position);
            }
        }
    }
    public void ExecuteAttack(int patternID, Vector2 targetPos)
    {
        Vector2 dir = (targetPos - (Vector2)transform.position).normalized;
        _shooter.PlayPatternLocal(patternID, transform.position, dir, BulletType.EnemyBullet);
    }
    public void TakeDamage(int damage)
    {
        _network.OnMonsterDamaged(UniqueID, damage);
    }
    public void ApplyDamage(int damage)
    {
        _currentHP -= damage;
        UpdateUI();
        if (_currentHP <= 0) Die();
    }
    private void UpdateUI()
    {
        if (_hpSlider != null && _data != null)
            _hpSlider.value = _currentHP / _data.maxHP;
    }
    private void Die()
    {
        StopAllCoroutines();
        _network.OnMonsterDied(UniqueID);
    }
}
