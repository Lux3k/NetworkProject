using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Photon.Pun;
using Photon.Realtime;

public class RoomMgr : MonoBehaviourPunCallbacks
{
    [SerializeField] Button roomBtn;

    private void Start()
    {
        Player[] players = PhotonNetwork.PlayerList; //방 속 사람을 받아옴

        foreach (var p in players)
        {
            Logger.Log("방 안의 사람들 목록: " + p.NickName);
        }

        if (PhotonNetwork.IsMasterClient == false) //방장 여부에 따라 코드 등등
        {
            roomBtn.interactable = false;
            //혹은 방장이 아니라면 text를 start 대신 Ready등등
        }
    }
    public void StartGame()
    {
        //PhotonNetwork.AutomaticallySyncScene 이거 아까 켜놨던 것 기억하기
        if (PhotonNetwork.IsMasterClient == true)
        {
            PhotonNetwork.LoadLevel("InGame"); //네트워크상에서 씬 바꾸는 것
        }
    }
    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Logger.Log(newPlayer.NickName + "님이 방에 입장하셨습니다");
    }

    public override void OnPlayerLeftRoom(Player newPlayer)
    {
        Logger.Log(newPlayer.NickName + "님이 나갔습니다");
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Logger.Log(newMasterClient.NickName + "님이 방장이 되었습니다");
        //PhotonNetwork.IsMasterClient 로 내 자신이 방장이 되었는지 체크 후 Start 권한 이어받는 로직
    }
}
