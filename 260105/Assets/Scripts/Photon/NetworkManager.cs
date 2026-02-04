using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using System;

public class NetworkManager : SingletonPunBehaviour<NetworkManager>
{
    public bool IsConnected { get; private set; }
    public Action OnMasterChanged;
    protected override void Init()
    {
        base.isDestroyOnLoad = true;

        base.Init();
    }
    public void Connect()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.NickName = "Player" + UnityEngine.Random.Range(0, 1000);
        PhotonNetwork.PhotonServerSettings.AppSettings.FixedRegion = "kr";
        PhotonNetwork.PhotonServerSettings.AppSettings.UseNameServer = true;
        PhotonNetwork.ConnectUsingSettings();
    }
    // 서버 연결 성공
    public override void OnConnectedToMaster()
    {
        Debug.Log("서버 연결 완료!");
        SceneLoader.Instance.LoadScene(SceneType.Lobby);
    }

    // 연결 실패
    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.LogError($"연결 끊김: {cause}");
    }
    public void StartGame()
    {
        if (PhotonNetwork.IsMasterClient)
            SceneLoader.Instance.LoadNetworkScene(SceneType.InGame);
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }
    public override void OnLeftRoom()
    {
        SceneLoader.Instance.LoadScene(SceneType.Lobby);
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Logger.Log(newPlayer.NickName + "님이 방에 입장하셨습니다");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Logger.Log(otherPlayer.NickName + "님이 나갔습니다");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Logger.Log(newMasterClient.NickName + "님이 방장이 되었습니다");
    }
}