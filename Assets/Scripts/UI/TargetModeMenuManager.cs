using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TargetModeMenuManager : MonoBehaviour
{
    [Header("Menu Components")]
    public GameObject menuPanel;
    public Button toggleButton;
    public TextMeshProUGUI toggleButtonText;
    
    [Header("Mode Buttons")]
    public Button qualificationButton;
    public Button reactiveButton;
    
    private bool isMenuOpen = true;
    
    void Start()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.AddListener(ToggleMenu);
            UpdateToggleButtonText();
        }
        
        if (qualificationButton != null)
        {
            qualificationButton.onClick.AddListener(() => SelectMode("Qualification"));
        }
        
        if (reactiveButton != null)
        {
            reactiveButton.onClick.AddListener(() => SelectMode("Reactive"));
        }
        
        if (menuPanel == null)
        {
            menuPanel = gameObject;
        }
    }
    
    void OnDestroy()
    {
        if (toggleButton != null)
        {
            toggleButton.onClick.RemoveListener(ToggleMenu);
        }
        
        if (qualificationButton != null)
        {
            qualificationButton.onClick.RemoveAllListeners();
        }
        
        if (reactiveButton != null)
        {
            reactiveButton.onClick.RemoveAllListeners();
        }
    }
    
    private void ToggleMenu()
    {
        isMenuOpen = !isMenuOpen;
        
        if (menuPanel != null)
        {
            foreach (Transform child in menuPanel.transform)
            {
                if (child.gameObject != toggleButton?.gameObject)
                {
                    child.gameObject.SetActive(isMenuOpen);
                }
            }
        }
        
        UpdateToggleButtonText();
    }
    
    private void UpdateToggleButtonText()
    {
        if (toggleButtonText != null)
        {
            toggleButtonText.text = isMenuOpen ? "X" : "☰";
        }
    }
    
    private void SelectMode(string mode)
    {
        Debug.Log($"Target mode selected: {mode}");
        
        // Add your mode-specific logic here
        switch (mode)
        {
            case "Qualification":
                // Handle qualification mode
                break;
            case "Reactive":
                // Handle reactive mode
                break;
        }
    }
}