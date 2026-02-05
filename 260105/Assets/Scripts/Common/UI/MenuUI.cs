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
        [SerializeField] private Button _resumeBtn;
        [SerializeField] private Button _settingsBtn;
        [SerializeField] private Button _lobbyBtn;
        [SerializeField] private Button _quitBtn;

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

        public void OnClickResumeBtn()
        {
            //AudioManager.Instance?.PlaySFX(SFX.ui_button_click);
            _onResume?.Invoke();
            CloseUI();
        }

        public void OnClickSettingsBtn()
        {
            //AudioManager.Instance?.PlaySFX(SFX.ui_button_click);
            UIManager.Instance.OpenUI<SettingsUI>(new SettingsUIData());
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
