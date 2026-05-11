// This script is made to handle the main menu screen interactions
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
   /// This method will start and begin the main game
   /// </summary>
   public void PlayGame()
   {
      SceneLoader.Instance.ChangeScene("TestScene");
   } 
   
   /// <summary>
   /// This method will quit the game
   /// </summary>
   public void QuitGame()
   {
      Application.Quit();
   }
}
