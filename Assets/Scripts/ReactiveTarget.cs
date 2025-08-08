using System;
using UnityEngine;

public enum ReactiveTargetState
{
    Inactive,    // Down and not hittable
    Active,      // Standing and hittable (enemy)
    Friendly     // Standing and hittable but shouldn't be shot
}

public class ReactiveTarget : MonoBehaviour
{
    [Header("State")]
    [SerializeField] private ReactiveTargetState currentState = ReactiveTargetState.Inactive;
    
    [Header("Components")]
    [SerializeField] private Collider targetCollider;
    
    [Header("Visual Feedback")]
    [SerializeField] private Material enemyMaterial;
    [SerializeField] private Material friendlyMaterial;
    [SerializeField] private Material inactiveMaterial;
    [SerializeField] private Renderer targetRenderer;
    
    private Animator targetAnimator;
    
    public ReactiveTargetState CurrentState => currentState;
    public bool IsActive => currentState != ReactiveTargetState.Inactive;
    public bool IsFriendly => currentState == ReactiveTargetState.Friendly;
    
    public event Action<ReactiveTarget> OnTargetHit;
    public event Action<ReactiveTarget> OnStateChanged;
    
    void Awake()
    {
        targetAnimator = GetComponent<Animator>();
        if (targetAnimator == null)
        {
            Debug.LogError($"No Animator found on {gameObject.name}! ReactiveTarget requires an Animator component.");
        }
        
        if (targetCollider == null)
        {
            targetCollider = GetComponentInChildren<Collider>();
        }
        
        if (targetRenderer == null)
        {
            targetRenderer = GetComponentInChildren<Renderer>();
        }
    }
    
    void Start()
    {
        SetState(ReactiveTargetState.Inactive);
    }
    
    public void SetState(ReactiveTargetState newState)
    {
        if (currentState == newState) return;
        
        ReactiveTargetState previousState = currentState;
        currentState = newState;
        
        UpdateVisuals();
        UpdateCollider();
        UpdateAnimation(previousState, newState);
        
        OnStateChanged?.Invoke(this);
    }
    
    private void UpdateAnimation(ReactiveTargetState from, ReactiveTargetState to)
    {
        if (targetAnimator == null) return;
        
        if (to == ReactiveTargetState.Inactive)
        {
            targetAnimator.ResetTrigger("StandUp");
            targetAnimator.SetTrigger("KnockDown");
        }
        else if (from == ReactiveTargetState.Inactive && (to == ReactiveTargetState.Active || to == ReactiveTargetState.Friendly))
        {
            targetAnimator.ResetTrigger("KnockDown");
            targetAnimator.SetTrigger("StandUp");
        }
    }
    
    private void UpdateVisuals()
    {
        if (targetRenderer == null) return;
        
        Material matToUse = inactiveMaterial;
        
        switch (currentState)
        {
            case ReactiveTargetState.Active:
                matToUse = enemyMaterial;
                break;
            case ReactiveTargetState.Friendly:
                matToUse = friendlyMaterial;
                break;
            case ReactiveTargetState.Inactive:
                matToUse = inactiveMaterial;
                break;
        }
        
        if (matToUse != null)
        {
            targetRenderer.material = matToUse;
        }
    }
    
    private void UpdateCollider()
    {
        if (targetCollider != null)
        {
            targetCollider.enabled = IsActive;
        }
    }
    
    public void OnHit(Vector3 hitPoint, Vector3 hitNormal)
    {
        if (!IsActive)
        {
            Debug.Log($"Target {gameObject.name} was hit but is inactive");
            return;
        }
        
        Debug.Log($"Target {gameObject.name} hit! Was {(IsFriendly ? "FRIENDLY" : "ENEMY")}");
        
        OnTargetHit?.Invoke(this);
        
        SetState(ReactiveTargetState.Inactive);
        
        if (IsFriendly)
        {
            Debug.LogWarning("Friendly target was shot!");
        }
    }
    
    public void Activate(bool asFriendly = false)
    {
        SetState(asFriendly ? ReactiveTargetState.Friendly : ReactiveTargetState.Active);
    }
    
    public void Deactivate()
    {
        SetState(ReactiveTargetState.Inactive);
    }
}