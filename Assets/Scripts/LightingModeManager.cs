using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class LightingModeManager : MonoBehaviour
{
    public enum LightingMode
    {
        Normal,
        Dark
    }
    
    [Header("Settings")]
    [SerializeField] private LightingMode currentMode = LightingMode.Normal;
    [SerializeField] private float darkModeAmbientIntensity = 0.05f;
    [SerializeField] private float normalModeAmbientIntensity = 1f;
    [SerializeField] private Color darkModeAmbientColor = new Color(0.05f, 0.05f, 0.1f);
    [SerializeField] private Color normalModeAmbientColor = Color.white;
    
    [Header("References")]
    [SerializeField] private FlashlightController flashlightController;
    
    private List<Light> sceneLights = new List<Light>();
    private Dictionary<Light, float> originalIntensities = new Dictionary<Light, float>();
    private Color originalAmbientLight;
    private float originalAmbientIntensity;
    private AmbientMode originalAmbientMode;
    
    void Awake()
    {
        if (flashlightController == null)
        {
            GameObject flashlightObj = new GameObject("FlashlightSystem");
            flashlightController = flashlightObj.AddComponent<FlashlightController>();
        }
        
        StoreOriginalLightingSettings();
        FindSceneLights();
    }
    
    
    void Start()
    {
        GameObject projectionCamera = GameObject.Find("ProjectionPlaneCamera");
        if (projectionCamera != null)
        {
            flashlightController.AttachToTransform(projectionCamera.transform);
        }
        else
        {
            Camera mainCam = Camera.main;
            if (mainCam != null)
            {
                flashlightController.AttachToTransform(mainCam.transform);
            }
        }
    }
    
    private void StoreOriginalLightingSettings()
    {
        originalAmbientLight = RenderSettings.ambientLight;
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        originalAmbientMode = RenderSettings.ambientMode;
        normalModeAmbientColor = originalAmbientLight;
        normalModeAmbientIntensity = originalAmbientIntensity;
    }
    
    private void FindSceneLights()
    {
        sceneLights.Clear();
        originalIntensities.Clear();
        
        Light[] allLights = FindObjectsOfType<Light>();
        foreach (Light light in allLights)
        {
            if (light.name.Contains("Area Light") || 
                light.type == LightType.Spot || 
                light.type == LightType.Point || 
                light.type == LightType.Directional)
            {
                if (!light.name.Contains("Flashlight"))
                {
                    sceneLights.Add(light);
                    originalIntensities[light] = light.intensity;
                }
            }
        }
        
        Debug.Log($"LightingModeManager: Found {sceneLights.Count} scene lights");
    }
    
    public void ToggleLightingMode()
    {
        SetLightingMode(currentMode == LightingMode.Normal ? LightingMode.Dark : LightingMode.Normal);
    }
    
    public void SetLightingMode(LightingMode mode)
    {
        currentMode = mode;
        
        switch (mode)
        {
            case LightingMode.Normal:
                SetNormalLighting();
                break;
            case LightingMode.Dark:
                SetDarkLighting();
                break;
        }
    }
    
    private void SetNormalLighting()
    {
        foreach (Light light in sceneLights)
        {
            if (light != null && originalIntensities.ContainsKey(light))
            {
                light.intensity = originalIntensities[light];
                light.enabled = true;
            }
        }
        
        RenderSettings.ambientLight = normalModeAmbientColor;
        RenderSettings.ambientIntensity = normalModeAmbientIntensity;
        
        if (flashlightController != null)
        {
            flashlightController.SetFlashlightEnabled(false);
        }
        
        Debug.Log("Lighting Mode: Normal");
    }
    
    private void SetDarkLighting()
    {
        foreach (Light light in sceneLights)
        {
            if (light != null)
            {
                light.intensity = 0f;
                light.enabled = false;
            }
        }
        
        RenderSettings.ambientLight = darkModeAmbientColor;
        RenderSettings.ambientIntensity = darkModeAmbientIntensity;
        
        if (flashlightController != null)
        {
            flashlightController.SetFlashlightEnabled(true);
        }
        
        Debug.Log("Lighting Mode: Dark (Flashlight Active)");
    }
    
    public LightingMode GetCurrentMode()
    {
        return currentMode;
    }
    
    public FlashlightController GetFlashlightController()
    {
        return flashlightController;
    }
}