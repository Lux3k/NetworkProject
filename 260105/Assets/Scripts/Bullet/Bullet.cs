using UnityEngine;
using Photon.Pun;

public enum BulletType
{
    PlayerBullet,
    EnemyBullet,
}

public class Bullet : MonoBehaviour
{
    public const int NO_GROUP_ID = -1;
    public Vector2 Direction { get; set; }
    public float CurrentSpeed { get; set; } = 10f;
    public float LifeTime { get; set; } = 5f;
    public int Damage { get; private set; } = 10;
    public Transform BulletTransform { get; private set; }

    public int GroupID { get; set; } = NO_GROUP_ID;
    public int OwnerActorNumber { get; set; }
    private IBulletStrategy _moveStrategy;


    private float _currentLifeTime;
    private SpriteRenderer _spriteRenderer;
    private float _radius;
    private int _targetLayer;

    private void Awake()
    {
        BulletTransform = transform;
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _radius = _spriteRenderer.bounds.extents.x;
    }

    public void Initialize(Vector2 pos, Vector2 direction, BulletType bulletType,
                          Color bulletColor, int groupID, int ownerID)
    {
        if (bulletType == BulletType.PlayerBullet)
            _targetLayer = LayerMask.GetMask("Enemy");
        else
            _targetLayer = LayerMask.GetMask("Player");

        BulletTransform.position = pos;
        Direction = direction.normalized;

        float angle = Mathf.Atan2(Direction.y, Direction.x) * Mathf.Rad2Deg;
        BulletTransform.rotation = Quaternion.Euler(0, 0, angle);

        _currentLifeTime = LifeTime;
        GroupID = groupID;
        OwnerActorNumber = ownerID;
        _spriteRenderer.color = bulletColor;
    }
    public void ResetGroupID()
    {
        GroupID = NO_GROUP_ID;
    }
    public void Move()
    {
        if (_moveStrategy != null)
            _moveStrategy.Move(this);


        Collider2D hit = Physics2D.OverlapCircle(
            BulletTransform.position,
            _radius,
            _targetLayer);

        if (hit != null)
            HandleHit(hit);

        _currentLifeTime -= Time.deltaTime;
        if (_currentLifeTime <= 0)
            SetDeactivate();
    }

  
    private void HandleHit(Collider2D collision)
    {

        if (PhotonNetwork.LocalPlayer.ActorNumber == OwnerActorNumber)
        {

            if (collision.TryGetComponent<IDamagable>(out IDamagable damagable))
            {
                if (collision.TryGetComponent<PhotonView>(out PhotonView targetPV))
                {
                    targetPV.RPC("TakeDamage", RpcTarget.All, Damage);
                }
                else
                {
                    damagable.TakeDamage(Damage);
                }
            }
        }

        SetDeactivate();
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

    private void SetDeactivate()
    {
        if (!gameObject.activeSelf) 
            return;
        gameObject.SetActive(false);
    }
}