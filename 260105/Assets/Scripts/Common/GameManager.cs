using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

public enum GameState { Playing, GameOver, StageClear }

public class GameManager : SingletonPunBehaviour<GameManager>
{

    [SerializeField] private GameObject _playerPrefab;
    [SerializeField] private BulletManager _bulletManager;
    [SerializeField] private StageManager _stageManager;

    public BulletManager BulletManager => _bulletManager;
    public GameState CurrentState { get; private set; } = GameState.Playing;

    private HashSet<int> _deadPlayers = new();
    private int _score;
    private float _playTime;
    private int _totalWaves;


    void Awake()
    {

        if (_bulletManager == null)
            _bulletManager = FindObjectOfType<BulletManager>();
    }

    protected override void Init()
    {
        base._isDestroyOnLoad = true;

        base.Init();
    }

    void Start()
    { 
        CurrentState = GameState.Playing;
        _deadPlayers.Clear();



        if (PlayerController.LocalPlayerInstance == null)
            PhotonNetwork.Instantiate(_playerPrefab.name, Vector3.zero, Quaternion.identity);

        if (_stageManager != null)
        {
            _stageManager.OnWaveStart += OnWaveStart;
            _stageManager.OnAllStageClear += OnGameClear;
        }
    }

    void Update()
    {
        HandleEscapeInput();

        if (CurrentState == GameState.Playing)
            _playTime += Time.deltaTime;
    }

    private void OnDisable()
    {
        _stageManager.OnWaveStart -= OnWaveStart;
        _stageManager.OnAllStageClear -= OnGameClear;
    }
    void HandleEscapeInput()
    {
        if (!Input.GetKeyDown(KeyCode.Escape)) return;
        if (CurrentState == GameState.GameOver || CurrentState == GameState.StageClear) return;

        if (UIManager.Instance.ExistsOpenUI())
        {
            UIManager.Instance.CloseCurrFrontUI();
            if (!UIManager.Instance.ExistsOpenUI())
                ResumeGame();
        }
        else
        {
            PauseGame();
        }
    }

    void PauseGame()
    {

        UIManager.Instance.OpenUI<MenuUI>(new MenuUIData
        {
            OnResume = ResumeGame,
            OnLobby = ReturnToLobby,
            OnQuit = QuitGame,
            OnClose = ResumeGame
        });
    }

    void ResumeGame()
    {
        CurrentState = GameState.Playing;
    }

    public void AddScore(int amount) => _score += amount;

    public void OnPlayerDeath(Player player)
    {
        _deadPlayers.Add(player.ActorNumber);

        if (_deadPlayers.Count >= PhotonNetwork.CurrentRoom.PlayerCount)
        {
            if (PhotonNetwork.IsMasterClient)
                photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
        }
    }

    [PunRPC]
    void RPC_GameOver()
    {
        CurrentState = GameState.GameOver;

        UIManager.Instance.OpenUI<GameOverUI>(new GameOverUIData
        {
            Score = _score,
            Wave = _stageManager?.CurrentWaveIndex ?? 0,
            OnRestart = Restart,
            OnLobby = ReturnToLobby,
            OnQuit = QuitGame
        });
    }


    void OnWaveStart(int waveIndex, int totalWaves)
    {
        _totalWaves++;
    }

    void OnGameClear()
    {
        CurrentState = GameState.StageClear;

        UIManager.Instance.OpenUI<GameClearUI>(new GameClearUIData
        {
            TotalScore = _score,
            TotalWaves = _totalWaves,
            PlayTime = _playTime,
            OnLobby = ReturnToLobby,
            OnQuit = QuitGame
        });
    }



    public void Restart()
    {
        UIManager.Instance.CloseAllOpenUI();

        _deadPlayers.Clear();
        _score = 0;

        SceneLoader.Instance.LoadNetworkScene(SceneType.InGame);


    }

    public void ReturnToLobby()
    {
        UIManager.Instance.CloseAllOpenUI();
        PhotonNetwork.LeaveRoom();
    }

    void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

    public override void OnLeftRoom()
    {
        SceneLoader.Instance.LoadScene(SceneType.Lobby);
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Logger.Log($"플레이어 퇴장: {otherPlayer.NickName}");

        _deadPlayers.Add(otherPlayer.ActorNumber);

        if (PhotonNetwork.IsMasterClient)
        {
            int aliveCount = PhotonNetwork.CurrentRoom.PlayerCount - _deadPlayers.Count;
            if (aliveCount <= 0)
                photonView.RPC(nameof(RPC_GameOver), RpcTarget.All);
        }
    }

    public override void OnMasterClientSwitched(Player newMasterClient)
    {
        Logger.Log($"새 마스터: {newMasterClient.NickName}");
    }
}
