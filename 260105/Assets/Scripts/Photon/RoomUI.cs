using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomUI : MonoBehaviour
{
    [SerializeField] private Button _startButton;

    void Start()
    {
        Player[] players = PhotonNetwork.PlayerList;

        foreach (var p in players)
        {
            Logger.Log("방 안의 사람들 목록: " + p.NickName);
        }

        _startButton.interactable = PhotonNetwork.IsMasterClient;
    }

    void OnEnable()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMasterChanged += UpdateUI;
    }

    void OnDisable()
    {
        if (NetworkManager.Instance != null)
            NetworkManager.Instance.OnMasterChanged -= UpdateUI;
    }

    void UpdateUI()
    {
        _startButton.interactable = PhotonNetwork.IsMasterClient;
    }

    public void OnStartClick()
    {
        NetworkManager.Instance.StartGame();
    }
}