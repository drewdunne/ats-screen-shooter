using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] private float intensity = 5000f; // Increased for outdoor visibility
    [SerializeField] private float range = 100f; // Increased range for outdoor
    [SerializeField] private float spotAngle = 25f; // Wider beam
    [SerializeField] private float innerSpotAngle = 10f;
    [SerializeField] private Color lightColor = new Color(0.95f, 0.95f, 1f);
    
    [Header("References")]
    [SerializeField] private Light flashlightLight;
    
    private bool isEnabled = false;
    private Transform attachmentPoint;
    private Camera targetCamera;
    private InputHandlers inputHandlers;
    
    void Awake()
    {
        if (flashlightLight == null)
        {
            CreateFlashlight();
        }
        
        SetFlashlightEnabled(false);
    }
    
    void Start()
    {
        // Find the InputHandlers to track crosshair
        inputHandlers = FindObjectOfType<InputHandlers>();
        
        // Find the camera we're attached to
        GameObject projectionCamera = GameObject.Find("ProjectionPlaneCamera");
        if (projectionCamera != null)
        {
            targetCamera = projectionCamera.GetComponent<Camera>();
        }
        else
        {
            targetCamera = Camera.main;
        }
    }
    
    void Update()
    {
        // Track crosshair position if flashlight is enabled
        if (isEnabled && inputHandlers != null && targetCamera != null)
        {
            // Get the first player's aim point (primary crosshair)
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
                        
                        // Convert screen point to world ray
                        Ray ray = targetCamera.ScreenPointToRay(screenPoint);
                        
                        // Point the flashlight along the ray direction
                        if (flashlightLight != null)
                        {
                            flashlightLight.transform.position = ray.origin;
                            flashlightLight.transform.rotation = Quaternion.LookRotation(ray.direction);
                        }
                        
                        break; // Only track the first player
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
        
        // Add URP additional light data component for better light control
        UniversalAdditionalLightData lightData = flashlightObj.AddComponent<UniversalAdditionalLightData>();
    }
    
    public void AttachToTransform(Transform target)
    {
        attachmentPoint = target;
        // Don't parent the flashlight to the attachment point anymore
        // We'll control its position manually in Update()
    }
    
    public void ToggleFlashlight()
    {
        SetFlashlightEnabled(!isEnabled);
    }
    
    public void SetFlashlightEnabled(bool enabled)
    {
        isEnabled = enabled;
        if (flashlightLight != null)
        {
            flashlightLight.enabled = enabled;
        }
    }
    
    public bool IsEnabled()
    {
        return isEnabled;
    }
    
    public void SetIntensity(float newIntensity)
    {
        intensity = newIntensity;
        if (flashlightLight != null)
        {
            flashlightLight.intensity = intensity;
        }
    }
    
    public void SetRange(float newRange)
    {
        range = newRange;
        if (flashlightLight != null)
        {
            flashlightLight.range = range;
        }
    }
}