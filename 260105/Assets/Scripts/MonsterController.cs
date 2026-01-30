using Photon.Pun;
using System.Collections;
using UnityEngine;
using UnityEngine.UI; 

[RequireComponent(typeof(Rigidbody2D))] 
[RequireComponent(typeof(Collider2D))]  
[RequireComponent(typeof(BulletShooter))] 
public class MonsterController : MonoBehaviourPunCallbacks, IPunObservable, IDamagable
{
    [Header("Monster Stats")]
    [SerializeField] private float _maxHP = 100f;
    [SerializeField] private float _moveSpeed = 2f;
    [SerializeField] private int _scoreValue = 100;

    [Header("Attack Settings")]
    [SerializeField] private int[] _attackPatternIDs; 
    [SerializeField] private float _attackInterval = 3f;

    [Header("UI")]
    [SerializeField] private Slider _hpSlider; 

    private float _currentHP;
    private BulletShooter _shooter;
    private Rigidbody2D _rb;
    private Transform _target; 

    private void Awake()
    {
        _shooter = GetComponent<BulletShooter>();
    }

    public override void OnEnable()
    {
        base.OnEnable();
        _currentHP = _maxHP;
        UpdateUI();

        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(AttackRoutine());
        }
    }

    private void Update()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            FindNearestTarget();
            MoveToTarget();
        }

    }

    private void FindNearestTarget()
    {
        if (_target == null)
        {
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
    }

    private void MoveToTarget()
    {
        if (_target == null) return;

        Vector2 dir = (_target.position - transform.position).normalized;
        transform.Translate(dir * _moveSpeed * Time.deltaTime);

        if (dir.x != 0)
            transform.localScale = new Vector3(dir.x < 0 ? 1 : -1, 1, 1);
    }

    private IEnumerator AttackRoutine()
    {
        while (_currentHP > 0)
        {
            yield return new WaitForSeconds(_attackInterval);

            if (_target != null && _attackPatternIDs.Length > 0)
            {
                int randomPatternID = _attackPatternIDs[Random.Range(0, _attackPatternIDs.Length)];

                Vector2 dir = (_target.position - transform.position).normalized;

                _shooter.PlayPattern(randomPatternID, transform.position, dir, BulletType.EnemyBullet);
            }
        }
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        _currentHP -= damage;
        UpdateUI();


        if (_currentHP <= 0)
        {
            Die();
        }
    }

    private void UpdateUI()
    {
        if (_hpSlider != null)
        {
            _hpSlider.value = _currentHP / _maxHP;
        }
    }

    private void Die()
    {

        if (photonView.IsMine)
        {
            PhotonNetwork.Destroy(gameObject);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(_currentHP);
        }
        else
        {
            _currentHP = (float)stream.ReceiveNext();
            UpdateUI();
        }
    }
}