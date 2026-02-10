using System;
using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

public class StageManager : MonoBehaviourPunCallbacks, IStageNetwork
{
    [SerializeField] private MonsterSpawner _spawner;
    [SerializeField] private int _startStageID = 1;

    private StageData _currentStage;
    private int _currentWaveIndex;
    private bool _isRunning;

    public int CurrentStageID => _currentStage?.stageID ?? 0;
    public int CurrentWaveIndex => _currentWaveIndex;
    public bool IsRunning => _isRunning;

    public event Action<StageData> OnStageStart;
    public event Action<StageData> OnStageClear;
    public event Action OnAllStageClear;
    public event Action<int, int> OnWaveStart;

    IEnumerator Start()
    {
        yield return new WaitUntil(() => DataManager.Instance != null && DataManager.Instance.IsLoaded);

        if (PhotonNetwork.IsMasterClient)
            RequestStartStage(_startStageID);
    }

    public void RequestStartStage(int stageID)
    {
        photonView.RPC(nameof(RPC_StartStage), RpcTarget.All, stageID);
    }

    public void RequestNextStage()
    {
        if (!_isRunning && PhotonNetwork.IsMasterClient)
        {
            int nextID = _currentStage.stageID + 1;
            if (DataManager.Instance.GetStage(nextID) != null)
                RequestStartStage(nextID);
        }
    }

    [PunRPC]
    void RPC_StartStage(int stageID)
    {
        _currentStage = DataManager.Instance.GetStage(stageID);
        if (_currentStage == null)
        {
            Logger.LogError($"Stage {stageID} not found");
            return;
        }

        _currentWaveIndex = 0;
        _isRunning = true;
        OnStageStart?.Invoke(_currentStage);
        Logger.Log($"스테이지 시작: {_currentStage.stageName}");

        StartCoroutine(RunStage());
    }

    private IEnumerator RunStage()
    {
        for (int i = 0; i < _currentStage.waveIDs.Length; i++)
        {
            _currentWaveIndex = i;
            int waveID = _currentStage.waveIDs[i];
            WaveData wave = DataManager.Instance.GetWave(waveID);

            if (wave == null)
            {
                Logger.LogError($"Wave {waveID} not found");
                continue;
            }

            OnWaveStart?.Invoke(i, _currentStage.waveIDs.Length);
            Logger.Log($"웨이브 {i + 1}/{_currentStage.waveIDs.Length} 시작");

            yield return StartCoroutine(_spawner.RunWave(wave));

            Logger.Log($"웨이브 {i + 1} 완료");
        }

        _isRunning = false;
        OnStageClear?.Invoke(_currentStage);
        Logger.Log($"스테이지 클리어: {_currentStage.stageName}");

        int nextID = _currentStage.stageID + 1;
        bool hasNext = DataManager.Instance.GetStage(nextID) != null;

        if (hasNext)
        {
            yield return new WaitForSeconds(3f);
            RequestNextStage();
        }
        else
        {
            OnAllStageClear?.Invoke();
            Logger.Log("모든 스테이지 클리어!");
        }
    }
}
