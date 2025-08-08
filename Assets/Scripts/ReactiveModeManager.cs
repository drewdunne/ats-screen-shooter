using System.Collections.Generic;
using UnityEngine;

public class ReactiveModeManager : MonoBehaviour
{
    [Header("Target Configuration")]
    public List<GameObject> ReactiveTargetList = new List<GameObject>();
    
    [Header("Timing Configuration")]
    [Range(0.5f, 10f)]
    public float TimeToWaitForActivation = 2f;
    
    [Header("Probability Configuration")]
    [Range(0f, 1f)]
    public float ProbabilityOfActivate = 0.7f;
    
    [Range(0f, 1f)]
    public float ProbabilityFriendlyTarget = 0.2f;
    
    [Header("Target Limits")]
    [Range(1, 10)]
    public int MaxActiveTargets = 3;
    
    [Header("Friendly Target Timing")]
    [Range(1f, 10f)]
    public float WaitTimeForFriendlyMin = 2f;
    
    [Range(2f, 20f)]
    public float WaitTimeForFriendlyMax = 5f;
    
    private int currentActiveTargets = 0;
    
    void Start()
    {
        if (ReactiveTargetList == null || ReactiveTargetList.Count == 0)
        {
            Debug.LogError($"ReactiveTargetList is not populated on {gameObject.name}!");
            enabled = false;
            return;
        }
        
        foreach (var target in ReactiveTargetList)
        {
            if (target == null)
            {
                Debug.LogError($"Null target found in ReactiveTargetList on {gameObject.name}!");
                enabled = false;
                return;
            }
        }
        
        if (WaitTimeForFriendlyMin > WaitTimeForFriendlyMax)
        {
            Debug.LogError($"WaitTimeForFriendlyMin ({WaitTimeForFriendlyMin}) cannot be greater than WaitTimeForFriendlyMax ({WaitTimeForFriendlyMax})!");
            enabled = false;
            return;
        }
    }
    
    void OnEnable()
    {
        StartReactiveMode();
    }
    
    void OnDisable()
    {
        StopReactiveMode();
    }
    
    private void StartReactiveMode()
    {
        Debug.Log("Reactive Mode Started");
        currentActiveTargets = 0;
    }
    
    private void StopReactiveMode()
    {
        Debug.Log("Reactive Mode Stopped");
        currentActiveTargets = 0;
    }
}