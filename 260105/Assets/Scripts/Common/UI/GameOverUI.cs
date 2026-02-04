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
    [SerializeField] private TextMeshProUGUI scoreTxt;
    [SerializeField] private TextMeshProUGUI waveTxt;
    [SerializeField] private Button restartBtn;
    [SerializeField] private Button lobbyBtn;
    [SerializeField] private Button quitBtn;

    private Action _onRestart;
    private Action _onLobby;
    private Action _onQuit;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        var data = uiData as GameOverUIData;
        if (scoreTxt) scoreTxt.text = $"Score: {data.Score}";
        if (waveTxt) waveTxt.text = $"Wave: {data.Wave}";

        _onRestart = data.OnRestart;
        _onLobby = data.OnLobby;
        _onQuit = data.OnQuit;
        if (restartBtn != null)
        {
            if (PhotonNetwork.IsMasterClient)
            {
                restartBtn.gameObject.SetActive(true); 
            }
            else
            {
                restartBtn.gameObject.SetActive(false); 
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
