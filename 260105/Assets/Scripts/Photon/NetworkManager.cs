using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;

        PhotonNetwork.NickName = "Player" + Random.Range(0, 1000);

        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "kr";
        PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
        PhotonNetwork.ConnectUsingSettings();

        Debug.Log("서버 연결 시도 중...");
    }

    // 서버 연결 성공
    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 연결 완료!");
        SceneManager.LoadScene("LobbyScene");
    }

    // 연결 실패
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"연결 끊김: {cause}");
    }
}