// This script is made to handle the main menu screen interactions by Vonce and Andrew
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
   [SerializeField] private Button playButton;
   [SerializeField] private Button settingsButton;
   [SerializeField] private Button tutorialButton;
   [SerializeField] private Button quitButton;
   
   void Update()
   {
      if (Cursor.lockState != CursorLockMode.None)
      {
         Cursor.lockState = CursorLockMode.None;
         Cursor.visible = true;
      }
   }

   /// <summary>
   /// Starts a brand-new run and loads the dungeon scene
   /// </summary>
   public void PlayGame()
   {
      if (RunManager.Instance != null && RunManager.Instance.hasPlayedTutorial)
      {
         RunManager.Instance.StartNewRun();
         SceneLoader.Instance.ChangeScene("DungeonGenScene");
      }
      else
      {
         TutorialUIManager.Instance.StartCoroutine(TutorialUIManager.Instance.ShowTutorialWarning());
         Debug.LogWarning("No RunManager found - run state won't persist or player hasn't played tutorial!");
      }

   }
   
   /// <summary>
   /// This method will quit the game
   /// </summary>
   public void QuitGame()
   {
      Application.Quit();
   }
}
