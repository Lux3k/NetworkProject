using System.Collections.Generic;
using UnityEngine;

public class BulletGroup
{
    public int GroupID { get; private set; }
    public bool IsActive { get; private set; }

    public List<Bullet> MyBullets { get; private set; } = new List<Bullet>(100);

    public PatternData PatternData { get; private set; }
    public BulletPatternSO PatternSO { get; private set; }

    private int _currentPhaseIndex;
    private float _phaseTimer;

    public BulletGroup() { MyBullets = new List<Bullet>(100);}
    public void Initialize(int id, BulletPatternSO so)
    {
        GroupID = id;
        PatternSO = so;
        PatternData = null;
        CommonInit();
    }

    public void Initialize(int id, PatternData data)
    {
        GroupID = id;
        PatternData = data;
        PatternSO = null;
        CommonInit();
    }

    private void CommonInit()
    {
        _currentPhaseIndex = 0;
        _phaseTimer = 0f;
        IsActive = true;
        MyBullets.Clear();
    }

    public void Reset()
    {
        IsActive = false;
        MyBullets.Clear();
        PatternSO = null;
        PatternData = null;
    }

    public void AddBullet(Bullet bullet)
    {
        if (bullet.GroupID == Bullet.NO_GROUP_ID) bullet.GroupID = this.GroupID;
        MyBullets.Add(bullet);
    }

    public void Update(float deltaTime)
    {
        if (!IsActive || MyBullets.Count == 0) return;

        _phaseTimer += deltaTime;

        float currentDuration = 0f;
        int phaseCount = 0;

        if (PatternSO != null && PatternSO.phases != null)
        {
            phaseCount = PatternSO.phases.Length;
            if (_currentPhaseIndex < phaseCount)
                currentDuration = PatternSO.phases[_currentPhaseIndex].duration;
        }
        else if (PatternData != null && PatternData.phases != null)
        {
            phaseCount = PatternData.phases.Count;
            if (_currentPhaseIndex < phaseCount)
                currentDuration = PatternData.phases[_currentPhaseIndex].duration;
        }

        if (_currentPhaseIndex < phaseCount - 1)
        {
            if (_phaseTimer >= currentDuration)
            {
                NextPhase();
            }
        }

        CleanupDeadBullets();
    }

    private void CleanupDeadBullets()
    {
        for (int i = MyBullets.Count - 1; i >= 0; i--)
        {
            if (!MyBullets[i].gameObject.activeSelf)
            {
                if (MyBullets[i].GroupID == this.GroupID) MyBullets[i].ResetGroupID();

                int lastIndex = MyBullets.Count - 1;
                MyBullets[i] = MyBullets[lastIndex];
                MyBullets.RemoveAt(lastIndex);
            }
        }
    }

    private void NextPhase()
    {
        _currentPhaseIndex++;
        _phaseTimer = 0f;

        IBulletStrategy newStrategy = null;

        if (PatternSO != null) newStrategy = PatternSO.phases[_currentPhaseIndex].strategy;
        else if (PatternData != null) newStrategy = DataManager.Instance.GetStrategy(PatternData.phases[_currentPhaseIndex]);

        if (newStrategy == null) return;

        for (int i = MyBullets.Count - 1; i >= 0; i--)
        {
            if (MyBullets[i].gameObject.activeSelf)
                MyBullets[i].SetStrategy(newStrategy);
        }
    }
}