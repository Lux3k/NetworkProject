using UnityEngine;
using Photon.Pun;

public enum BulletType
{
    PlayerBullet,
    EnemyBullet,
}

public class Bullet : MonoBehaviour
{
    // 기본 속성
    public Vector2 Direction { get; set; }
    public float CurrentSpeed { get; set; } = 10f;
    public float LifeTime { get; set; } = 5f;
    public int Damage { get; private set; } = 10;
    public Transform BulletTransform { get; private set; }

    // 그룹 및 소유자
    public int GroupID { get; set; }
    public int OwnerPhotonViewID { get; set; }

    [SerializeField ]private BulletMoveStrategyBase _moveStrategy;

    private float _currentLifeTime;
    private SpriteRenderer _spriteRenderer;
    private Rigidbody2D _rb;

    private void Awake()
    {
        BulletTransform = transform;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _rb = GetComponent<Rigidbody2D>();

        if (_rb != null)
        {
            _rb.bodyType = RigidbodyType2D.Kinematic;
            _rb.gravityScale = 0f;
        }
    }

    public void Initialize(Vector2 pos, Vector2 dir, BulletType bulletType,
                          Color bulletColor, int groupID, int ownerPhotonViewID)
    {
        // 레이어 설정
        if (bulletType == BulletType.PlayerBullet)
            gameObject.layer = LayerMask.NameToLayer("PlayerBullet");
        else
            gameObject.layer = LayerMask.NameToLayer("EnemyBullet");

        // 위치 및 방향
        BulletTransform.position = pos;
        Direction = dir.normalized;

        float angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
        BulletTransform.rotation = Quaternion.Euler(0, 0, angle);

        // 수명
        _currentLifeTime = LifeTime;

        // 그룹 및 소유자
        GroupID = groupID;
        OwnerPhotonViewID = ownerPhotonViewID;

        // 색상 적용
        _spriteRenderer.color = bulletColor;
    }

    public void Move()
    {
        if (_moveStrategy != null)
            _moveStrategy.BulletMovement(this);

        _currentLifeTime -= Time.deltaTime;
        if (_currentLifeTime <= 0)
        {
            PoolReturn();
        }
    }

    public void SetStrategy(BulletMoveStrategyBase newStrategy)
    {
        _moveStrategy = newStrategy;
        CurrentSpeed = _moveStrategy.startSpeed;

        // 전략 시작 시 초기화
        newStrategy.OnStrategyStart(this);
    }

    public void ColorChange(Color bulletColor)
    {
        _spriteRenderer.color = bulletColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        PhotonView targetPV = collision.GetComponent<PhotonView>();

        if (targetPV != null)
        {
            if (targetPV.ViewID != OwnerPhotonViewID && !targetPV.IsMine)
            {
                targetPV.RPC("TakeDamage", RpcTarget.All, Damage);
            }
        }
        else
        {
            if (collision.TryGetComponent<IDamagable>(out IDamagable damagable))
            {
                damagable.TakeDamage(Damage);
            }
        }

        PoolReturn();
    }

    private void PoolReturn()
    {
        GameManager.Instance.BulletManager.ReturnBullet(this);
    }
}