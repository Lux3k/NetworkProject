using Photon.Pun;
using Photon.Pun.Demo.PunBasics;
using System;
using UnityEngine;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager Instance;
    [SerializeField] GameObject playerPrefab;
    [SerializeField] private BulletManager bulletManager;
    public BulletManager BulletManager => bulletManager;


    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        if (bulletManager == null)
        {
            bulletManager = FindObjectOfType<BulletManager>();

        }

    }
    void Start()
    {
        
        if (PlayerManager.LocalPlayerInstance == null) //플레이어매니져가 이미 플레이어 정보를 들고있다면 패스할거임
        {
            // PlayerManager의 Start()에서 이미 PlayerInputController를 활성화/비활성화하므로
            // 여기서는 생성만 하면 됩니다
            PhotonNetwork.Instantiate(playerPrefab.name, new Vector3(0, 0, 0), Quaternion.identity);
        }
       
    }
    public override void OnLeftRoom()
    {
        SceneLoader.Instance.LoadScene(SceneType.Lobby);
    }
    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
}
