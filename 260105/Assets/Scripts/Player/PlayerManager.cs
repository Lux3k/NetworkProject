using Cinemachine;
using Photon.Pun;
using UnityEngine;

public class PlayerManager : MonoBehaviourPunCallbacks
{
    [Header("Status")]
    [SerializeField] private float moveSpeed = 10f;

    [Header("Combat")]
    [SerializeField] private WeaponHolder weaponHolder;

    public static GameObject LocalPlayerInstance;

    private Vector2 _moveDirection;

    void Awake()
    {
        if (photonView.IsMine)
            LocalPlayerInstance = this.gameObject;

        if (weaponHolder == null)
            weaponHolder = GetComponentInChildren<WeaponHolder>();
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

        Vector3 currentScale = transform.localScale;

        if (_moveDirection.x > 0)
        {
            transform.localScale = new Vector3(Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
        else if (_moveDirection.x < 0)
        {
            transform.localScale = new Vector3(-Mathf.Abs(currentScale.x), currentScale.y, currentScale.z);
        }
    }

    public void TryAttack(bool isAttacking, Vector2 screenPos)
    {
        if (!photonView.IsMine) return;


        Vector3 worldPos = Camera.main.ScreenToWorldPoint(new Vector3(screenPos.x, screenPos.y, Camera.main.nearClipPlane));

        worldPos.z = 0f;
        Debug.DrawLine(transform.position, worldPos, Color.red, 1.0f);

        FireBullet(worldPos);
    }

    private void FireBullet(Vector2 targetPos)
    {
        if (weaponHolder == null || weaponHolder.CurrentWeapon == null) return;

        Vector2 fireDirection = (targetPos - (Vector2)transform.position).normalized;
        weaponHolder.Fire(fireDirection, BulletType.PlayerBullet);
    }
}


