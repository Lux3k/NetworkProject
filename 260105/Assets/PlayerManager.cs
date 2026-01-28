using Photon.Pun;
using Cinemachine;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks, IPunObservable, IDamagable
{
    [Header("Status")]
    [SerializeField] private int Health = 100;
    [SerializeField] private float moveSpeed = 10f;

    [Header("Combat")]
    [SerializeField] private BulletShooter bulletShooter; 
    [SerializeField] private BulletPatternSO currentPattern; 
    [SerializeField] private Transform firePoint; 

    public static GameObject LocalPlayerInstance;

    private bool isFiring;
    private Vector2 _moveDirection;

    void Awake()
    {
        if (photonView.IsMine)
            LocalPlayerInstance = this.gameObject;

        if (bulletShooter == null)
            bulletShooter = GetComponent<BulletShooter>();
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            CinemachineVirtualCamera vcam = FindObjectOfType<CinemachineVirtualCamera>();
            if (vcam != null)
            {
                vcam.Follow = this.transform;
            }
        }
    }

    void Update()
    {
        if (photonView.IsMine)
        {
            Move();
        }
    }

    public void SetMoveDirection(Vector2 direction)
    {
        _moveDirection = direction;
    }

    void Move()
    {
        if (_moveDirection == Vector2.zero) return;

        transform.Translate(_moveDirection * moveSpeed * Time.deltaTime);

        if (_moveDirection.x > 0)
            transform.localScale = new Vector3(1, 1, 1);
        else if (_moveDirection.x < 0)
            transform.localScale = new Vector3(-1, 1, 1);
    }

    public void TryAttack(bool isAttacking, Vector2 screenPos)
    {
        isFiring = isAttacking;

        if (isFiring && photonView.IsMine)
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

            FireBullet(worldPos);
        }
    }

    private void FireBullet(Vector2 targetPos)
    {
        if (bulletShooter == null || currentPattern == null) return;

        Vector2 fireDirection = (targetPos - (Vector2)transform.position).normalized;

        Vector2 firePosition = firePoint != null ? firePoint.position : transform.position;
        Debug.DrawRay(transform.position, fireDirection * 5f, Color.red, 1f);
        bulletShooter.PlayPattern(currentPattern, firePosition, fireDirection, BulletType.PlayerBullet);
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        Health -= damage;
        CheckDeath();
    }

    void CheckDeath()
    {
        if (!photonView.IsMine) return;

        if (Health <= 0f)
        {
            Debug.Log("»ç¸Á!");
            // GameManager.Instance.LeaveRoom();
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting)
        {
            stream.SendNext(isFiring);
            stream.SendNext(Health);
        }
        else
        {
            this.isFiring = (bool)stream.ReceiveNext();
            this.Health = (int)stream.ReceiveNext();
        }
    }
}