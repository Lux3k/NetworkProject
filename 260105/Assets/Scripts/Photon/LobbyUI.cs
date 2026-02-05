using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic; //실시간으로 해야 할 것들
using TMPro;

public class LobbyUI : MonoBehaviourPunCallbacks
{

    
    [SerializeField] private TMP_InputField _createRoomInput;
    [SerializeField] private TMP_InputField _joinRoomInput;

    [SerializeField] private GameObject _roomPrefab;
    [SerializeField] private Transform _roomListPanel;

    private void Start()
    {
        PhotonNetwork.JoinLobby(); //전 씬이 아닌, 여기서 조인로비 수행할거임
    }

    public void CreateRoom()
    {
        PhotonNetwork.CreateRoom(_createRoomInput.text); //옆에 인풋필드에 들어있던 내용의 이름으로 방 생성
    }
    public void JoinRoom()
    {
        PhotonNetwork.JoinRoom(_joinRoomInput.text);
    }
    public void JoinRandomRoom()
    {
        PhotonNetwork.JoinRandomOrCreateRoom();
    }
    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        // 이 로그가 뜨면 방 만들기가 실패한 것입니다.
        Logger.LogError($"방 만들기 실패! 원인: {message}");

        // (만약 버튼을 꺼놨다면 여기서 다시 켜줘야 함)
    }
    public void ExitLobby()
    {
        PhotonNetwork.LeaveLobby(); //로비 떠나라고 포톤에게 지시
        SceneLoader.Instance.LoadScene(SceneType.Title);
    }
    public override void OnJoinedLobby()
    {
        Logger.Log("로비에 입장하였습니다");
    }

    public override void OnJoinedRoom()
    {
        Logger.Log("방에 입장하여 룸 씬으로 전환요청까지 해둠");
        SceneLoader.Instance.LoadScene(SceneType.Room);
    }


    public override void OnRoomListUpdate(List<RoomInfo> roomList) //얘는 언제 호출? 룸 정보가 변했을 때, 조인로비 성공시 한번, 
    {
        foreach (Transform child in _roomListPanel)
        {
            Destroy(child.gameObject);
        }

        foreach (RoomInfo roomInfo in roomList)
        {
           
            if (roomInfo.RemovedFromList || !roomInfo.IsOpen || !roomInfo.IsVisible) continue;

            var room = Instantiate(_roomPrefab, _roomListPanel);
            room.GetComponentInChildren<TMP_Text>().text = roomInfo.Name;
        }
    }

}
