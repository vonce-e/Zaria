// This script was made by Andrew
// This script is to handle the tutorial UI in the main menu

using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;


public class TutorialUIManager : MonoBehaviour
{

    [Header("Tutorial UI Panels")]
    [SerializeField] private GameObject tutorialPanel;
    [SerializeField] private List<GameObject> tutorialPages;

    [Header("Navigation")]
    [SerializeField] private Button previousBTN;
    [SerializeField] private Button nextBTN;
    [SerializeField] private TMP_Text pageNumber;

    private int currentPageIndex;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
    }
    
    public void OpenTutorialPanel()
    {
        if (tutorialPanel != null && tutorialPages != null || tutorialPages.Count != 0)
        {
            tutorialPanel.SetActive(true);
            ShowPage(0);
        }
    }

    public void CloseTutorialPanel()
    {
        if (tutorialPanel != null)
        {
            tutorialPanel.SetActive(false);
        }
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
            pageNumber.text = $"{currentPageIndex + 1} / {tutorialPages.Count}";
        }
    }
}
