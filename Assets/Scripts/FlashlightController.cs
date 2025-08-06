using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] private float intensity = 600f; // 2x brightness for better visibility
    [SerializeField] private float range = 100f;
    [SerializeField] private float spotAngle = 25f; // Slightly wider for better coverage
    [SerializeField] private float innerSpotAngle = 10f;
    [SerializeField] private Color lightColor = new Color(0.95f, 0.95f, 1f); // Bright white light
    [SerializeField] private AnimationCurve falloffCurve = AnimationCurve.EaseInOut(0f, 1f, 1f, 0f); // Falloff over distance
    
    [Header("Mounting Position")]
    [SerializeField] private Vector3 mountOffset = new Vector3(0.1f, -0.15f, 0.3f); // Right, down, forward from camera
    
    [Header("References")]
    [SerializeField] private Light flashlightLight;
    
    private bool isEnabled = false;
    private Transform attachmentPoint;
    private Camera targetCamera;
    private InputHandlers inputHandlers;
    
    void Awake()
    {
        Debug.Log($"[{System.DateTime.Now:HH:mm:ss.fff}] FlashlightController: Awake() called");
        
        if (flashlightLight == null)
        {
            Debug.Log($"[{System.DateTime.Now:HH:mm:ss.fff}] FlashlightController: Creating flashlight");
            CreateFlashlight();
        }
        
        SetFlashlightEnabled(false);
    }
    
    void Start()
    {
        Debug.Log($"[{System.DateTime.Now:HH:mm:ss.fff}] FlashlightController: Start() called");
        
        inputHandlers = FindObjectOfType<InputHandlers>();
        
        GameObject projectionCamera = GameObject.Find("ProjectionPlaneCamera");
        if (projectionCamera != null)
        {
            targetCamera = projectionCamera.GetComponent<Camera>();
            Debug.Log($"[{System.DateTime.Now:HH:mm:ss.fff}] FlashlightController: Using ProjectionPlaneCamera as target");
        }
        else
        {
            targetCamera = Camera.main;
            Debug.Log($"[{System.DateTime.Now:HH:mm:ss.fff}] FlashlightController: Using Main Camera as target");
        }
    }
    
    void Update()
    {
        if (isEnabled && inputHandlers != null && targetCamera != null)
        {
            var players = inputHandlers.Players;
            if (players != null)
            {
                foreach (var (index, player) in players)
                {
                    if (player != null && player.point != null)
                    {
                        Vector2 screenPointNormal = player.point;
                        // Convert normalized coordinates to screen coordinates
                        Vector3 screenPoint = new Vector3(
                            screenPointNormal.x * Screen.width,
                            Screen.height - screenPointNormal.y * Screen.height,
                            10f // Distance from camera for ray calculation
                        );
                        
                        Ray ray = targetCamera.ScreenPointToRay(screenPoint);
                        
                        if (flashlightLight != null)
                        {
                            Vector3 lightPosition = targetCamera.transform.position + 
                                targetCamera.transform.right * mountOffset.x +
                                targetCamera.transform.up * mountOffset.y +
                                targetCamera.transform.forward * mountOffset.z;
                            
                            Vector3 targetPoint;
                            RaycastHit hit;
                            if (Physics.Raycast(ray, out hit, range))
                            {
                                targetPoint = hit.point;
                            }
                            else
                            {
                                targetPoint = ray.origin + ray.direction * range;
                            }
                            
                            flashlightLight.transform.position = lightPosition;
                            Vector3 aimDirection = (targetPoint - lightPosition).normalized;
                            flashlightLight.transform.rotation = Quaternion.LookRotation(aimDirection);
                            
                            // Use constant intensity - no dynamic adjustment
                            flashlightLight.intensity = intensity;
                        }
                        
                        break;
                    }
                }
            }
        }
    }
    
    private void CreateFlashlight()
    {
        GameObject flashlightObj = new GameObject("TacticalFlashlight");
        flashlightObj.transform.SetParent(transform);
        flashlightObj.transform.localPosition = Vector3.zero;
        flashlightObj.transform.localRotation = Quaternion.identity;
        
        flashlightLight = flashlightObj.AddComponent<Light>();
        flashlightLight.type = LightType.Spot;
        flashlightLight.intensity = intensity;
        flashlightLight.range = range;
        flashlightLight.spotAngle = spotAngle;
        flashlightLight.innerSpotAngle = innerSpotAngle;
        flashlightLight.color = lightColor;
        flashlightLight.shadows = LightShadows.Soft;
        flashlightLight.shadowStrength = 0.8f;
        flashlightLight.shadowBias = 0.05f;
        flashlightLight.shadowNormalBias = 0.4f;
        
        // Configure for softer light to prevent overexposure
        flashlightLight.renderMode = LightRenderMode.ForcePixel; // Ensure per-pixel lighting
        flashlightLight.bounceIntensity = 0.1f; // Minimal bounce light
        
        UniversalAdditionalLightData lightData = flashlightObj.AddComponent<UniversalAdditionalLightData>();
        
        // Configure URP-specific settings for better control
        lightData.usePipelineSettings = false;
        lightData.lightCookieSize = new Vector2(1f, 1f);
        lightData.lightCookieOffset = Vector2.zero;
        
        // Set soft shadows for less harsh lighting
        lightData.softShadowQuality = UnityEngine.Rendering.Universal.SoftShadowQuality.High;
    }
    
    public void AttachToTransform(Transform target)
    {
        attachmentPoint = target;
        Debug.Log($"[{System.DateTime.Now:HH:mm:ss.fff}] FlashlightController: Attached to transform: {(target != null ? target.name : "null")}");
    }
    
    public void ToggleFlashlight()
    {
        bool newState = !isEnabled;
        Debug.Log($"[{System.DateTime.Now:HH:mm:ss.fff}] FlashlightController: ToggleFlashlight() called - Switching from {isEnabled} to {newState}");
        SetFlashlightEnabled(newState);
    }
    
    public void SetFlashlightEnabled(bool enabled)
    {
        Debug.Log($"[{System.DateTime.Now:HH:mm:ss.fff}] FlashlightController: SetFlashlightEnabled({enabled}) - Previous state: {isEnabled}");
        
        isEnabled = enabled;
        if (flashlightLight != null)
        {
            flashlightLight.enabled = enabled;
            Debug.Log($"[{System.DateTime.Now:HH:mm:ss.fff}] FlashlightController: Flashlight light component enabled: {enabled}");
        }
        else
        {
            Debug.LogWarning($"[{System.DateTime.Now:HH:mm:ss.fff}] FlashlightController: flashlightLight is null, cannot set enabled state");
        }
    }
    
    public bool IsEnabled()
    {
        return isEnabled;
    }
    
    public void SetIntensity(float newIntensity)
    {
        Debug.Log($"[{System.DateTime.Now:HH:mm:ss.fff}] FlashlightController: SetIntensity({newIntensity}) - Previous: {intensity}");
        
        intensity = Mathf.Clamp(newIntensity, 0f, 500f); // Higher cap for terrain visibility
        if (flashlightLight != null)
        {
            flashlightLight.intensity = intensity;
            // Keep color consistent, don't dim it with intensity
            flashlightLight.color = lightColor;
        }
    }
    
    public void SetRange(float newRange)
    {
        Debug.Log($"[{System.DateTime.Now:HH:mm:ss.fff}] FlashlightController: SetRange({newRange}) - Previous: {range}");
        
        range = newRange;
        if (flashlightLight != null)
        {
            flashlightLight.range = range;
        }
    }
}