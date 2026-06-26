// This script is made to handle the main menu screen interactions by Vonce and Andrew
using UnityEngine;
using UnityEngine.UI;

public class MainMenuManager : MonoBehaviour
{
   [SerializeField] private Button playButton;
   [SerializeField] private Button settingsButton;
   [SerializeField] private Button socialsButton;
   [SerializeField] private Button logOutButton;
   [SerializeField] private Button quitButton;
   
   /// <summary>
   /// Starts a brand-new run and loads the dungeon scene
   /// </summary>
   public void PlayGame()
   {
      if (RunManager.Instance != null)
         RunManager.Instance.StartNewRun();
      else
         Debug.LogWarning("No RunManager found - run state won't persist.");

      SceneLoader.Instance.ChangeScene("DungeonGenScene");
   }
   
   /// <summary>
   /// This method will quit the game
   /// </summary>
   public void QuitGame()
   {
      Application.Quit();
   }
}
