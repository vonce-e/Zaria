using UnityEngine;
using TMPro;

public class HUDController : MonoBehaviour
{
    public static HUDController instance;
    
    [Header("Player HUD")]
    [SerializeField] private GameObject crosshair;
    
    [Header("Battle HUD")]
    [SerializeField] private GameObject battleHUD;

    private void Awake()
    {
        instance = this;
    }

    private void Start()
    {
        battleHUD.SetActive(false);
    }

    [SerializeField]
    private TMP_Text interactionText;

    public void SetCrosshair(bool useCrosshair)
    {
        crosshair.SetActive(!useCrosshair);
    }
    
    public void EnableInteractionText(string text)
    {
        interactionText.text = text + " (" + Settings.interactKey.ToString() + ")";
        interactionText.gameObject.SetActive(true);
    }

    public void DisableInteractionText()
    {
        interactionText.gameObject.SetActive(false);
    }

    public void SetBattleHUD(bool useBattleHUD)
    {
        battleHUD.SetActive(useBattleHUD);
    }
    
}
