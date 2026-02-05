using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameClearUIData : BaseUIData
{
    public int TotalScore;
    public int TotalWaves;
    public float PlayTime;
    public Action OnLobby;
    public Action OnQuit;
}

public class GameClearUI : BaseUI
{
    [SerializeField] private TextMeshProUGUI _scoreTxt;
    [SerializeField] private TextMeshProUGUI _waveTxt;
    [SerializeField] private TextMeshProUGUI _timeTxt;
    [SerializeField] private Button _lobbyBtn;
    [SerializeField] private Button _quitBtn;

    private Action _onLobby;
    private Action _onQuit;

    public override void SetInfo(BaseUIData uiData)
    {
        base.SetInfo(uiData);

        var data = uiData as GameClearUIData;
        if (_scoreTxt) _scoreTxt.text = $"Total Score: {data.TotalScore}";
        if (_waveTxt) _waveTxt.text = $"Clear Wave: {data.TotalWaves}";
        if (_timeTxt)
        {
            int min = (int)(data.PlayTime / 60);
            int sec = (int)(data.PlayTime % 60);
            _timeTxt.text = $"Play Time: {min:00}:{sec:00}";
        }

        _onLobby = data.OnLobby;
        _onQuit = data.OnQuit;
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
