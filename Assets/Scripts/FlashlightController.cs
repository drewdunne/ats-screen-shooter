using UnityEngine;
using UnityEngine.Rendering.Universal;

public class FlashlightController : MonoBehaviour
{
    [Header("Flashlight Settings")]
    [SerializeField] private float intensity = 1000f;
    [SerializeField] private float range = 50f;
    [SerializeField] private float spotAngle = 15f;
    [SerializeField] private float innerSpotAngle = 5f;
    [SerializeField] private Color lightColor = new Color(0.95f, 0.95f, 1f);
    
    [Header("References")]
    [SerializeField] private Light flashlightLight;
    
    private bool isEnabled = false;
    private Transform attachmentPoint;
    
    void Awake()
    {
        if (flashlightLight == null)
        {
            CreateFlashlight();
        }
        
        SetFlashlightEnabled(false);
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
        if (flashlightLight != null && attachmentPoint != null)
        {
            transform.SetParent(attachmentPoint);
            transform.localPosition = Vector3.zero;
            transform.localRotation = Quaternion.identity;
        }
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