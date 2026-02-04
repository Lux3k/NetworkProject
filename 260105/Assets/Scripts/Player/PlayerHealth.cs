using System;
using Photon.Pun;
using UnityEngine;

public class PlayerHealth : MonoBehaviourPun, IDamagable
{
    [SerializeField] private float maxHP = 100f;
    private float _currentHP;

    public float CurrentHP => _currentHP;
    public float MaxHP => maxHP;
    public bool IsDead => _currentHP <= 0;

    public event Action<float, float> OnHPChanged; 
    public event Action OnDeath;

    void Start()
    {
        _currentHP = maxHP;
        OnHPChanged?.Invoke(_currentHP, maxHP);
    }

    [PunRPC]
    public void TakeDamage(int damage)
    {
        if (IsDead) return;
        if (!photonView.IsMine) return;

        _currentHP = Mathf.Max(0, _currentHP - damage);
        photonView.RPC(nameof(RPC_SyncHP), RpcTarget.All, _currentHP);

        if (_currentHP <= 0)
            Die();
    }

    [PunRPC]
    void RPC_SyncHP(float hp)
    {
        _currentHP = hp;
        OnHPChanged?.Invoke(_currentHP, maxHP);
    }

    private void Die()
    {
        OnDeath?.Invoke();
        photonView.RPC(nameof(RPC_PlayerDied), RpcTarget.All);
    }

    [PunRPC]
    void RPC_PlayerDied()
    {
        GameManager.Instance.OnPlayerDeath(photonView.Owner);
    }

    public void Heal(float amount)
    {
        if (IsDead) return;
        _currentHP = Mathf.Min(maxHP, _currentHP + amount);
        photonView.RPC(nameof(RPC_SyncHP), RpcTarget.All, _currentHP);
    }

    public void Revive()
    {
        _currentHP = maxHP;
        photonView.RPC(nameof(RPC_SyncHP), RpcTarget.All, _currentHP);
    }

}
