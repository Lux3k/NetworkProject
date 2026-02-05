using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class MonsterController : MonoBehaviour, IDamagable
{
    private MonsterData _data;
    private float _currentHP;
    private BulletShooter _shooter;
    private Rigidbody2D _rigidBody;
    private Transform _target;

    private IMonsterCombatNetwork _network;
    public int UniqueID { get; private set; }

    [Header("UI")]
    [SerializeField] private Slider _hpSlider;

    private void Awake()
    {
        _shooter = GetComponent<BulletShooter>();
        _rigidBody = GetComponent<Rigidbody2D>();
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
        float minDistance = float.MaxValue;
        foreach (var p in players)
        {
            float distance = Vector2.Distance(transform.position, p.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                _target = p.transform;
            }
        }
    }

    private void MoveToTarget()
    {
        if (_target == null || _data == null) return;

        Vector2 direction = ((Vector2)_target.position - _rigidBody.position).normalized;
        _rigidBody.MovePosition(_rigidBody.position + direction * _data.moveSpeed * Time.fixedDeltaTime);

        if (direction.x != 0)
            transform.localScale = new Vector3(direction.x < 0 ? 1 : -1, 1, 1);
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
        Vector2 direction = (targetPos - (Vector2)transform.position).normalized;
        _shooter.PlayPatternLocal(patternID, transform.position, direction, BulletType.EnemyBullet);
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
