using System.Collections;
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
    
    private HashSet<GameObject> activeTargets = new HashSet<GameObject>();
    private Coroutine targetActivationCoroutine;
    private List<Animator> targetAnimators = new List<Animator>();
    
    void Start()
    {
        if (ReactiveTargetList == null || ReactiveTargetList.Count == 0)
        {
            Debug.LogError($"ReactiveTargetList is not populated on {gameObject.name}!");
            enabled = false;
            return;
        }
        
        targetAnimators.Clear();
        foreach (var target in ReactiveTargetList)
        {
            if (target == null)
            {
                Debug.LogError($"Null target found in ReactiveTargetList on {gameObject.name}!");
                enabled = false;
                return;
            }
            
            Animator animator = target.GetComponent<Animator>();
            if (animator == null)
            {
                animator = target.GetComponentInChildren<Animator>();
            }
            
            if (animator == null)
            {
                Debug.LogError($"No Animator found on target {target.name}!");
                enabled = false;
                return;
            }
            
            targetAnimators.Add(animator);
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
        activeTargets.Clear();
        
        foreach (var animator in targetAnimators)
        {
            animator.SetTrigger("KnockDown");
        }
        
        if (targetActivationCoroutine != null)
        {
            StopCoroutine(targetActivationCoroutine);
        }
        targetActivationCoroutine = StartCoroutine(TargetActivationLoop());
    }
    
    private void StopReactiveMode()
    {
        Debug.Log("Reactive Mode Stopped");
        
        if (targetActivationCoroutine != null)
        {
            StopCoroutine(targetActivationCoroutine);
            targetActivationCoroutine = null;
        }
        
        activeTargets.Clear();
    }
    
    private IEnumerator TargetActivationLoop()
    {
        yield return new WaitForSeconds(1f);
        
        while (enabled)
        {
            if (activeTargets.Count < MaxActiveTargets)
            {
                if (Random.value < ProbabilityOfActivate)
                {
                    List<GameObject> inactiveTargets = new List<GameObject>();
                    for (int i = 0; i < ReactiveTargetList.Count; i++)
                    {
                        if (!activeTargets.Contains(ReactiveTargetList[i]))
                        {
                            inactiveTargets.Add(ReactiveTargetList[i]);
                        }
                    }
                    
                    if (inactiveTargets.Count > 0)
                    {
                        int randomIndex = Random.Range(0, inactiveTargets.Count);
                        GameObject targetToActivate = inactiveTargets[randomIndex];
                        int targetIndex = ReactiveTargetList.IndexOf(targetToActivate);
                        
                        targetAnimators[targetIndex].SetTrigger("StandUp");
                        activeTargets.Add(targetToActivate);
                        
                        bool isFriendly = Random.value < ProbabilityFriendlyTarget;
                        
                        if (isFriendly)
                        {
                            float friendlyWaitTime = Random.Range(WaitTimeForFriendlyMin, WaitTimeForFriendlyMax);
                            StartCoroutine(KnockDownTargetAfterDelay(targetToActivate, targetIndex, friendlyWaitTime));
                        }
                        
                        Debug.Log($"Activated target: {targetToActivate.name} - {(isFriendly ? "Friendly" : "Enemy")}");
                    }
                }
            }
            
            yield return new WaitForSeconds(TimeToWaitForActivation);
        }
    }
    
    private IEnumerator KnockDownTargetAfterDelay(GameObject target, int targetIndex, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (activeTargets.Contains(target))
        {
            targetAnimators[targetIndex].SetTrigger("KnockDown");
            activeTargets.Remove(target);
            Debug.Log($"Friendly target knocked down: {target.name}");
        }
    }
    
    public void OnTargetHit(GameObject target)
    {
        if (activeTargets.Contains(target))
        {
            activeTargets.Remove(target);
            int targetIndex = ReactiveTargetList.IndexOf(target);
            if (targetIndex >= 0)
            {
                targetAnimators[targetIndex].SetTrigger("KnockDown");
            }
        }
    }
}