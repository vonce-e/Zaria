// This script turns the UI panels on and off.
// Made by Vonce Chew

using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Panels")]
    public GameObject settingsPanel;
    public GameObject shopPanel;
    public GameObject blacksmithPanel;
    public GameObject enchantPanel;
    public GameObject discardPanel;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        // Make sure every panel starts hidden when the scene loads.
        HideAll();
    }

    // Turns off every panel. Called once at the start.
    public void HideAll()
    {
        if (settingsPanel != null) settingsPanel.SetActive(false);
        if (shopPanel != null) shopPanel.SetActive(false);
        if (blacksmithPanel != null) blacksmithPanel.SetActive(false);
        if (enchantPanel != null) enchantPanel.SetActive(false);
        if (discardPanel != null) discardPanel.SetActive(false);
    }

    // Settings
    public void OpenSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void CloseSettings()
    {
        settingsPanel.SetActive(false);
    }

    // Shop
    public void OpenShop()
    {
        shopPanel.SetActive(true);
    }

    public void CloseShop()
    {
        shopPanel.SetActive(false);
    }

    // Blacksmith
    public void OpenBlacksmith()
    {
        blacksmithPanel.SetActive(true);
    }

    public void CloseBlacksmith()
    {
        blacksmithPanel.SetActive(false);
        enchantPanel.SetActive(false);
        discardPanel.SetActive(false);
    }

    // Blacksmith sub-pages
    public void OpenEnchant()
    {
        blacksmithPanel.SetActive(false); // hide the blacksmith menu behind it
        enchantPanel.SetActive(true);
    }

    public void CloseEnchant()
    {
        enchantPanel.SetActive(false);
        blacksmithPanel.SetActive(true); // go back to the blacksmith menu
    }

    public void OpenDiscard()
    {
        blacksmithPanel.SetActive(false);
        discardPanel.SetActive(true);
    }

    public void CloseDiscard()
    {
        discardPanel.SetActive(false);
        blacksmithPanel.SetActive(true);
    }
}
