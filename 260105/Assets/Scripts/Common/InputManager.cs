using UnityEngine;
using UnityEngine.InputSystem;
using Photon.Pun;

public class PlayerInputController : MonoBehaviour
{
    [SerializeField] private PlayerManager _manager;

    private InputAction _moveAction;
    private InputAction _attackAction;
    private PhotonView _photonView;
    private Camera _mainCamera;
    void Awake()
    {
        _manager = GetComponent<PlayerManager>();
        _photonView = GetComponent<PhotonView>();

        _moveAction = InputSystem.actions.FindAction("Move");
        _attackAction = InputSystem.actions.FindAction("Fire");
        _mainCamera = Camera.main;

    }

    void OnEnable()
    {
        // 내 캐릭터만 Input 활성화
        if (_photonView != null && !_photonView.IsMine)
        {
            return;
        }


        if (_moveAction != null && _attackAction != null)
        {
            _moveAction.Enable();
            _attackAction.Enable();
            _attackAction.performed += OnAttack;
            _attackAction.canceled += OnAttackCancel;
        }
    }

    void OnDisable()
    {
        if (_moveAction != null && _attackAction != null)
        {
            _moveAction.Disable();
            _attackAction.Disable();
            _attackAction.performed -= OnAttack;
            _attackAction.canceled -= OnAttackCancel;
        }
    }

    void Update()
    {
        // 혹시 몰라서 이중 체크
        if (_photonView != null && !_photonView.IsMine) return;

        if (_moveAction != null && _manager != null)
        {
            Vector2 moveInput = _moveAction.ReadValue<Vector2>();
            _manager.SetMoveDirection(moveInput);
        }
    }

    void OnAttack(InputAction.CallbackContext context)
    {
        Vector2 screenPos = Mouse.current.position.ReadValue();
        _manager?.TryAttack(true,screenPos);
    }

    void OnAttackCancel(InputAction.CallbackContext context)
    {
        _manager?.TryAttack(false,Vector2.zero);
    }
    
}