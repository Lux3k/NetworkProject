using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;   

public class GameOverUIData : BaseUIData
{
    public int Score;
    public int Wave;
    public Action OnRestart;
    public Action OnLobby;
    public Action OnQuit;
}

public class GameOverUI : BaseUI
{
    [SerializeField] private TextMeshProUGUI _scoreTxt;
    [SerializeField] private TextMeshProUGUI _waveTxt;
    [SerializeField] private Button _restartBtn;
    [SerializeField] private Button _lobbyBtn;
    [SerializeField] private Button _quitBtn;

    private Action _onRestart;
    private Action _onLobby;
    private Action _onQuit;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        var data = uiData as GameOverUIData;
        if (_scoreTxt) _scoreTxt.text = $"Score: {data.Score}";
        if (_waveTxt) _waveTxt.text = $"Wave: {data.Wave}";

        _onRestart = data.OnRestart;
        _onLobby = data.OnLobby;
        _onQuit = data.OnQuit;
        if (_restartBtn != null)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                _restartBtn.gameObject.SetActive(true); 
            }
            else
            {
                _restartBtn.gameObject.SetActive(false); 
            }
        }
    }

    public void OnClickRestartBtn()
    {
        //AudioManager.Instance?.PlaySFX(SFX.ui_button_click);
        _onRestart?.Invoke();
        CloseUI();
    }

    public void OnClickLobbyBtn()
    {
        //AudioManager.Instance?.PlaySFX(SFX.ui_button_click);
        _onLobby?.Invoke();
        CloseUI();
    }

    public void OnClickQuitBtn()
    {
        //AudioManager.Instance?.PlaySFX(SFX.ui_button_click);
        _onQuit?.Invoke();
    }
}
