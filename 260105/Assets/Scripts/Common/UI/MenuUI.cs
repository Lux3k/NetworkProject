    using System;
    using UnityEngine;
    using UnityEngine.UI;

    public class MenuUIData : BaseUIData
    {
        public Action OnResume;
        public Action OnLobby;
        public Action OnQuit;
    }

    public class MenuUI : BaseUI
    {
        [SerializeField] private Button _resumeButton;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private Button _lobbyButton;
        [SerializeField] private Button _quitButton;

        private Action _onResume;
        private Action _onLobby;
        private Action _onQuit;

        public override void SetInfo(BaseUIData uiData)
        {
            base.SetInfo(uiData);

            var data = uiData as MenuUIData;
            _onResume = data.OnResume;
            _onLobby = data.OnLobby;
            _onQuit = data.OnQuit;
        }

        public void OnClickResumeButton()
        {
            //AudioManager.Instance?.PlaySFX(SFX.ui_button_click);
            _onResume?.Invoke();
            CloseUI();
        }

        public void OnClickSettingsButton()
        {
            //AudioManager.Instance?.PlaySFX(SFX.ui_button_click);
            UIManager.Instance.OpenUI<SettingsUI>(new SettingsUIData());
        }

        public void OnClickLobbyButton()
        {
            //AudioManager.Instance?.PlaySFX(SFX.ui_button_click);
            _onLobby?.Invoke();
            CloseUI();
        }

        public void OnClickQuitButton()
        {
            //AudioManager.Instance?.PlaySFX(SFX.ui_button_click);
            _onQuit?.Invoke();
        }
    }
