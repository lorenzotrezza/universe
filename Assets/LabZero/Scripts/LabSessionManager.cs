using System;
using UnityEngine;

public class LabSessionManager : MonoBehaviour
{
    [SerializeField] private LabSessionSettings settings;

    public event Action StateChanged;

    public LabSessionState RunState { get; private set; }
    public float ElapsedTime { get; private set; }
    public int MistakeCount { get; private set; }
    public float Score => CalculateScore();
    public LabSessionSettings Settings => settings;

    public void Initialize(LabSessionSettings sessionSettings)
    {
        settings = sessionSettings;
        ResetSession();
    }

    public void StartRun()
    {
        if (RunState != LabSessionState.NotStarted)
        {
            return;
        }

        RunState = LabSessionState.Running;
        StateChanged?.Invoke();
    }

    public void RegisterMistake()
    {
        if (RunState != LabSessionState.Running)
        {
            return;
        }

        MistakeCount++;
        StateChanged?.Invoke();
    }

    public void CompleteRun()
    {
        if (RunState != LabSessionState.Running)
        {
            return;
        }

        RunState = LabSessionState.Complete;
        StateChanged?.Invoke();
    }

    public void ResetSession()
    {
        RunState = LabSessionState.NotStarted;
        ElapsedTime = 0f;
        MistakeCount = 0;
        StateChanged?.Invoke();
    }

    private void Update()
    {
        AdvanceElapsedTime(Time.deltaTime);
    }

    private void AdvanceElapsedTime(float deltaTime)
    {
        if (RunState != LabSessionState.Running)
        {
            return;
        }

        ElapsedTime += Mathf.Max(0f, deltaTime);
    }

    private float CalculateScore()
    {
        if (RunState == LabSessionState.NotStarted)
        {
            return 0f;
        }

        const float maxScore = 1000f;
        var timePenalty = ElapsedTime * 1.5f;
        var mistakePenalty = MistakeCount * 50f;

        return Mathf.Max(0f, maxScore - timePenalty - mistakePenalty);
    }
}
