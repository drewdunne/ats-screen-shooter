using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Gaia;

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
    
    // Gaia components
    private SceneProfile gaiaSceneProfile;
    private int originalLightingProfileIndex = -1;
    private int dayProfileIndex = -1;
    private int nightProfileIndex = -1;
    
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
        
        // Check for Gaia Scene Lighting
        CheckForGaiaSceneLighting();
    }
    
    private void StoreOriginalLightingSettings()
    {
        originalAmbientLight = RenderSettings.ambientLight;
        originalAmbientIntensity = RenderSettings.ambientIntensity;
        originalAmbientMode = RenderSettings.ambientMode;
        normalModeAmbientColor = originalAmbientLight;
        normalModeAmbientIntensity = originalAmbientIntensity;
    }
    
    private void CheckForGaiaSceneLighting()
    {
        // Try to get Gaia Global instance which contains the scene profile
        GaiaGlobal gaiaGlobal = GaiaGlobal.Instance;
        if (gaiaGlobal != null && gaiaGlobal.SceneProfile != null)
        {
            gaiaSceneProfile = gaiaGlobal.SceneProfile;
            originalLightingProfileIndex = gaiaSceneProfile.m_selectedLightingProfileValuesIndex;
            
            // Find Day and Night profile indices by name
            for (int i = 0; i < gaiaSceneProfile.m_lightingProfiles.Count; i++)
            {
                string profileName = gaiaSceneProfile.m_lightingProfiles[i].m_typeOfLighting.ToLower();
                if (profileName.Contains("day") || profileName.Contains("noon"))
                {
                    dayProfileIndex = i;
                }
                else if (profileName.Contains("night") || profileName.Contains("midnight"))
                {
                    nightProfileIndex = i;
                }
            }
            
            Debug.Log($"Gaia Scene Profile found. Current index: {originalLightingProfileIndex}, Day: {dayProfileIndex}, Night: {nightProfileIndex}");
        }
        else
        {
            Debug.Log("No Gaia Scene Profile found");
        }
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
        
        // Restore Gaia lighting profile to original or Day
        if (gaiaSceneProfile != null && dayProfileIndex >= 0)
        {
            // Use original profile if it wasn't night, otherwise use day
            int targetProfile = (originalLightingProfileIndex != nightProfileIndex) ? originalLightingProfileIndex : dayProfileIndex;
            gaiaSceneProfile.m_selectedLightingProfileValuesIndex = targetProfile;
            
            // Force update of Gaia lighting
            GaiaGlobal gaiaGlobal = GaiaGlobal.Instance;
            if (gaiaGlobal != null)
            {
                gaiaGlobal.UpdateGaiaTimeOfDay(false);
            }
            
            // Manually update skybox if needed
            ApplySkyboxFromProfile(targetProfile);
            
            Debug.Log($"Gaia: Restored lighting profile to index {targetProfile}");
        }
        
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
        
        // Set Gaia lighting profile to Night
        if (gaiaSceneProfile != null && nightProfileIndex >= 0)
        {
            gaiaSceneProfile.m_selectedLightingProfileValuesIndex = nightProfileIndex;
            
            // Force update of Gaia lighting
            GaiaGlobal gaiaGlobal = GaiaGlobal.Instance;
            if (gaiaGlobal != null)
            {
                gaiaGlobal.UpdateGaiaTimeOfDay(false);
            }
            
            // Manually update skybox if needed
            ApplySkyboxFromProfile(nightProfileIndex);
            
            Debug.Log($"Gaia: Set lighting profile to Night (index {nightProfileIndex})");
        }
        else if (gaiaSceneProfile != null && nightProfileIndex < 0)
        {
            Debug.LogWarning("No Night profile found in Gaia lighting profiles");
        }
        
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
    
    private void ApplySkyboxFromProfile(int profileIndex)
    {
        if (gaiaSceneProfile == null || profileIndex < 0 || profileIndex >= gaiaSceneProfile.m_lightingProfiles.Count)
            return;
            
        GaiaLightingProfileValues profile = gaiaSceneProfile.m_lightingProfiles[profileIndex];
        if (profile == null)
            return;
            
        // Apply skybox settings directly to RenderSettings
        Material skyboxMat = RenderSettings.skybox;
        if (skyboxMat != null)
        {
            // Update common skybox properties
            if (skyboxMat.HasProperty("_Tint"))
                skyboxMat.SetColor("_Tint", profile.m_skyboxTint);
                
            if (skyboxMat.HasProperty("_Exposure"))
                skyboxMat.SetFloat("_Exposure", profile.m_skyboxExposure);
                
            if (skyboxMat.HasProperty("_Rotation"))
                skyboxMat.SetFloat("_Rotation", profile.m_skyboxRotationOffset);
                
            // For HDRI skyboxes
            if (skyboxMat.HasProperty("_Tex") && profile.m_skyboxHDRI != null)
                skyboxMat.SetTexture("_Tex", profile.m_skyboxHDRI);
                
            // For procedural skyboxes
            if (skyboxMat.HasProperty("_SunSize"))
                skyboxMat.SetFloat("_SunSize", profile.m_sunSize);
                
            if (skyboxMat.HasProperty("_AtmosphereThickness"))
                skyboxMat.SetFloat("_AtmosphereThickness", profile.m_atmosphereThickness);
                
            if (skyboxMat.HasProperty("_SkyTint"))
                skyboxMat.SetColor("_SkyTint", profile.m_skyboxTint);
                
            if (skyboxMat.HasProperty("_GroundColor"))
                skyboxMat.SetColor("_GroundColor", profile.m_groundColor);
        }
        
        // Also update ambient settings from the profile
        RenderSettings.ambientMode = profile.m_ambientMode;
        RenderSettings.ambientIntensity = profile.m_ambientIntensity;
        RenderSettings.ambientSkyColor = profile.m_skyAmbient;
        RenderSettings.ambientEquatorColor = profile.m_equatorAmbient;
        RenderSettings.ambientGroundColor = profile.m_groundAmbient;
        
        // Update fog settings
        RenderSettings.fogColor = profile.m_fogColor;
        RenderSettings.fogMode = profile.m_fogMode;
        if (profile.m_fogMode == FogMode.Linear)
        {
            RenderSettings.fogStartDistance = profile.m_fogStartDistance;
            RenderSettings.fogEndDistance = profile.m_fogEndDistance;
        }
        else
        {
            RenderSettings.fogDensity = profile.m_fogDensity;
        }
    }
}