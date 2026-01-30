using UnityEngine;
using Photon.Pun;

public enum BulletType
{
    PlayerBullet,
    EnemyBullet,
}

public class Bullet : MonoBehaviour
{
    public Vector2 Direction { get; set; }
    public float CurrentSpeed { get; set; } = 10f;
    public float LifeTime { get; set; } = 5f;
    public int Damage { get; private set; } = 10;
    public Transform BulletTransform { get; private set; }

    public int GroupID { get; set; }
    public int _ownerActorNumber { get; set; }
    private IBulletStrategy _moveStrategy;


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
        if (bulletType == BulletType.PlayerBullet)
            gameObject.layer = LayerMask.NameToLayer("PlayerBullet");
        else
            gameObject.layer = LayerMask.NameToLayer("EnemyBullet");

        BulletTransform.position = pos;
        Direction = dir.normalized;

        float angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
        BulletTransform.rotation = Quaternion.Euler(0, 0, angle);

        _currentLifeTime = LifeTime;

        GroupID = groupID;
        _ownerActorNumber = ownerPhotonViewID;

        _spriteRenderer.color = bulletColor;
    }

    public void Move()
    {
        if (_moveStrategy != null)
            _moveStrategy.Move(this);

        _currentLifeTime -= Time.deltaTime;
        if (_currentLifeTime <= 0)
        {
            PoolReturn();
        }
    }

    public void SetStrategy(IBulletStrategy newStrategy)
    {
        _moveStrategy = newStrategy;
        newStrategy.OnStart(this);
    }

    public void ColorChange(Color bulletColor)
    {
        _spriteRenderer.color = bulletColor;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (PhotonNetwork.LocalPlayer.ActorNumber == _ownerActorNumber)
        {
            if (collision.TryGetComponent<IDamagable>(out IDamagable damagable))
            {
                if (collision.TryGetComponent<PhotonView>(out PhotonView targetPV))
                {

                    if (collision.CompareTag("Enemy"))
                        targetPV.RPC("TakeDamage", RpcTarget.MasterClient, Damage);
                    else
                        targetPV.RPC("TakeDamage", RpcTarget.All, Damage);
                }
                else
                {
                    damagable.TakeDamage(Damage);
                }
            }
        }

        PoolReturn();
    }

    private void PoolReturn()
    {
        GameManager.Instance.BulletManager.ReturnBullet(this);
    }
}