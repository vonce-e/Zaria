// This script was made by Andrew
// This script is to handle the tutorial UI in the main menu

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using System.Collections;


public class TutorialUIManager : MonoBehaviour
{

    [Header("Tutorial UI Panels")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private List<GameObject> tutorialPages;
    [SerializeField] private List<RectTransform> pageHeaders;
    [SerializeField] private GameObject tutorialWarningPanel;

    [Header("Navigation")]
    [SerializeField] private Button previousBTN;
    [SerializeField] private Button nextBTN;
    [SerializeField] private TMP_Text pageNumber;

    private int currentPageIndex;
    private int pagesDone;

    public static TutorialUIManager Instance;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }

        if (tutorialWarningPanel != null)
        {
            tutorialWarningPanel.SetActive(false);
        }
    }

    void Update()
    {
        for (int i = 0; i < pageHeaders.Count; i++)
        {
            if (pageHeaders[i] == null)
            {
                continue;
            }

            Vector3 targetScale = i == currentPageIndex ? Vector3.one * 1.1f : Vector3.one;

            pageHeaders[i].localScale = Vector3.Lerp(pageHeaders[i].localScale, targetScale, 10f * Time.unscaledDeltaTime);
        }
    }
    
    public void OpenTutorialPanel()
    {
        if (tutorialPanel != null && tutorialPages != null || tutorialPages.Count != 0)
        {
            tutorialPanel.SetActive(true);
            ShowPage(0);

            // Enables that they have at least done the tutorial
            if (RunManager.Instance != null)
            { 
                RunManager.Instance.hasPlayedTutorial = true;
            }
        }
    }

    public void CloseTutorialPanel()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }

    public IEnumerator ShowTutorialWarning()
    {
        if (tutorialWarningPanel == null)
        {
            yield break;
        }
        
        tutorialWarningPanel.SetActive(true);

        yield return new WaitForSeconds(2f);

        tutorialWarningPanel.SetActive(false);
    }

    public void NextTutorialPage()
    {
        ShowPage(currentPageIndex + 1);
    }

    public void ShowPreviousPage()
    {
        ShowPage(currentPageIndex - 1);
    }

    public void ShowPage(int pageIndex)
    {
        if (pageIndex < 0 || pageIndex >= tutorialPages.Count)
        {
            return;
        }

        currentPageIndex = pageIndex;

        for (int i = 0; i < tutorialPages.Count; i++)
        {
            if (tutorialPages[i] != null)
            {
                tutorialPages[i].SetActive(i == currentPageIndex);
            }
        }

        previousBTN.interactable = currentPageIndex > 0;
        nextBTN.interactable = currentPageIndex < tutorialPages.Count - 1;

        if (pageNumber != null)
        {
            pageNumber.text = $"{currentPageIndex + 1}";
        }
    }
}
