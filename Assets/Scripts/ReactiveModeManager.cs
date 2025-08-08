using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class ReactiveModeManager : MonoBehaviour
{
    [Header("Target Configuration")]
    public List<ReactiveTarget> ReactiveTargetList;
    
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
    
    private Coroutine targetActivationCoroutine;
    private Dictionary<ReactiveTarget, Coroutine> friendlyKnockdownCoroutines = new Dictionary<ReactiveTarget, Coroutine>();
    private bool isInitialized = false;
    
    void Awake()
    {
        // Store the Inspector-assigned list if it exists
        List<ReactiveTarget> inspectorList = null;
        if (ReactiveTargetList != null && ReactiveTargetList.Count > 0)
        {
            inspectorList = new List<ReactiveTarget>(ReactiveTargetList);
            Debug.Log($"Found {inspectorList.Count} targets from Inspector");
        }
        
        // Try to auto-populate from children
        var childTargets = GetComponentsInChildren<ReactiveTarget>();
        Debug.Log($"Found {childTargets.Length} ReactiveTargets in children");
        
        // Use Inspector list if available, otherwise use children
        if (inspectorList != null && inspectorList.Count > 0)
        {
            ReactiveTargetList = inspectorList;
            Debug.Log($"Using Inspector-assigned list with {ReactiveTargetList.Count} targets");
        }
        else if (childTargets.Length > 0)
        {
            ReactiveTargetList = new List<ReactiveTarget>(childTargets);
            Debug.Log($"Using auto-detected children with {ReactiveTargetList.Count} targets");
        }
        else
        {
            ReactiveTargetList = new List<ReactiveTarget>();
            Debug.LogWarning("No ReactiveTargets found - list is empty");
        }
    }
    
    void Start()
    {
        Debug.Log($"[START] Initial list count: {ReactiveTargetList?.Count ?? -1}");
        
        // Check each target and log its status
        if (ReactiveTargetList != null)
        {
            for (int i = 0; i < ReactiveTargetList.Count; i++)
            {
                if (ReactiveTargetList[i] == null)
                {
                    Debug.LogWarning($"Target at index {i} is null");
                }
                else
                {
                    Debug.Log($"Target {i}: {ReactiveTargetList[i].name} is valid");
                }
            }
        }
        
        // Clean up the list - remove any null entries
        int removedCount = ReactiveTargetList?.RemoveAll(t => t == null) ?? 0;
        if (removedCount > 0)
        {
            Debug.LogWarning($"Removed {removedCount} null targets from list");
        }
        
        if (ReactiveTargetList == null || ReactiveTargetList.Count == 0)
        {
            Debug.LogError($"No valid ReactiveTargets available for {gameObject.name}!");
            enabled = false;
            return;
        }
        
        Debug.Log($"Starting with {ReactiveTargetList.Count} valid targets");
        
        foreach (var target in ReactiveTargetList)
        {
            target.OnTargetHit += HandleTargetHit;
        }
        
        if (WaitTimeForFriendlyMin > WaitTimeForFriendlyMax)
        {
            Debug.LogError($"WaitTimeForFriendlyMin ({WaitTimeForFriendlyMin}) cannot be greater than WaitTimeForFriendlyMax ({WaitTimeForFriendlyMax})!");
            enabled = false;
            return;
        }
        
        isInitialized = true;
        
        // If we're already enabled, start the mode now that we're initialized
        if (enabled && gameObject.activeInHierarchy)
        {
            StartReactiveMode();
        }
    }
    
    void OnDestroy()
    {
        if (ReactiveTargetList != null)
        {
            foreach (var target in ReactiveTargetList)
            {
                if (target != null)
                {
                    target.OnTargetHit -= HandleTargetHit;
                }
            }
        }
    }
    
    void OnEnable()
    {
        if (isInitialized)
        {
            StartReactiveMode();
        }
    }
    
    void OnDisable()
    {
        StopReactiveMode();
    }
    
    private void StartReactiveMode()
    {
        if (!isInitialized || ReactiveTargetList == null) return;
        
        Debug.Log("Reactive Mode Started");
        
        foreach (var target in ReactiveTargetList)
        {
            if (target != null)
                target.Deactivate();
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
        
        foreach (var coroutine in friendlyKnockdownCoroutines.Values)
        {
            if (coroutine != null)
            {
                StopCoroutine(coroutine);
            }
        }
        friendlyKnockdownCoroutines.Clear();
        
        if (ReactiveTargetList != null && ReactiveTargetList.Count > 0)
        {
            foreach (var target in ReactiveTargetList)
            {
                if (target != null)
                    target.Deactivate();
            }
        }
    }
    
    private IEnumerator TargetActivationLoop()
    {
        Debug.Log("TargetActivationLoop started, waiting 1 second...");
        yield return new WaitForSeconds(1f);
        
        Debug.Log($"Starting activation loop. Total targets: {ReactiveTargetList.Count}");
        
        while (enabled)
        {
            int activeCount = ReactiveTargetList.Count(t => t.IsActive);
            Debug.Log($"Active targets: {activeCount}/{MaxActiveTargets}");
            
            if (activeCount < MaxActiveTargets)
            {
                float activationRoll = Random.value;
                Debug.Log($"Activation roll: {activationRoll} vs probability: {ProbabilityOfActivate}");
                
                if (activationRoll < ProbabilityOfActivate)
                {
                    var inactiveTargets = ReactiveTargetList.Where(t => !t.IsActive).ToList();
                    Debug.Log($"Found {inactiveTargets.Count} inactive targets");
                    
                    if (inactiveTargets.Count > 0)
                    {
                        int randomIndex = Random.Range(0, inactiveTargets.Count);
                        ReactiveTarget targetToActivate = inactiveTargets[randomIndex];
                        
                        bool isFriendly = Random.value < ProbabilityFriendlyTarget;
                        
                        Debug.Log($"Attempting to activate target: {targetToActivate.name} as {(isFriendly ? "Friendly" : "Enemy")}");
                        targetToActivate.Activate(isFriendly);
                        
                        if (isFriendly)
                        {
                            float friendlyWaitTime = Random.Range(WaitTimeForFriendlyMin, WaitTimeForFriendlyMax);
                            var coroutine = StartCoroutine(KnockDownFriendlyAfterDelay(targetToActivate, friendlyWaitTime));
                            friendlyKnockdownCoroutines[targetToActivate] = coroutine;
                        }
                        
                        Debug.Log($"Activated target: {targetToActivate.name} - {(isFriendly ? "Friendly" : "Enemy")}");
                    }
                }
            }
            
            yield return new WaitForSeconds(TimeToWaitForActivation);
        }
    }
    
    private IEnumerator KnockDownFriendlyAfterDelay(ReactiveTarget target, float delay)
    {
        yield return new WaitForSeconds(delay);
        
        if (target.IsActive && target.IsFriendly)
        {
            target.Deactivate();
            Debug.Log($"Friendly target auto-deactivated: {target.name}");
        }
        
        friendlyKnockdownCoroutines.Remove(target);
    }
    
    private void HandleTargetHit(ReactiveTarget target)
    {
        if (friendlyKnockdownCoroutines.ContainsKey(target))
        {
            if (friendlyKnockdownCoroutines[target] != null)
            {
                StopCoroutine(friendlyKnockdownCoroutines[target]);
            }
            friendlyKnockdownCoroutines.Remove(target);
        }
        
        if (target.IsFriendly)
        {
            Debug.LogWarning($"Player shot a friendly target: {target.name}!");
        }
        else
        {
            Debug.Log($"Player successfully hit enemy target: {target.name}");
        }
    }
}