// This script is made to handle the battle in games between the player and enemy

using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public enum BattleState
{
     START,
     PLAYERTURN,
     ENEMYTURN,
     WON,
     LOST
}

public class BattleSystem : MonoBehaviour
{

    public static BattleSystem Instance;

    [Header("Player Input")] public GameObject playerObj;
    public CharacterController playerCC;
    public PlayerInput playerInput;
    public GameObject enemyPrefab;

    [Header("Transform points")]
    public Transform playerPoint;
    public Transform enemyPoint;

    [Header("Cameras")] 
    public GameObject battleCamera;
    public GameObject playerCamera;

    [Header("User HUDS")] 
    public BattleHudManager playerHUD;
    public BattleHudManager enemyHUD;
    
    
    private Unit _playerUnit;
    private Unit _enemyUnit;
    
    public BattleState state;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
    
    
    /// <summary>
    /// This method will set up the battle in the scene
    /// </summary>
    public IEnumerator SetupBattle()
    {
        Debug.Log("Setting up battle..");
        
        // Sets the battle state
        state = BattleState.START; 
        
        // Moves the existing player and spawns the enemy in the battle scene
        TeleportPlayerToBattlePoint();
        LockPlayer();
        SetUpCamera(true);
        
        // Obtaining the enemy and player's information
        _playerUnit = playerObj.GetComponentInChildren<Unit>();
        
        GameObject enemyGo = Instantiate(enemyPrefab, enemyPoint.position, enemyPoint.rotation);
        _enemyUnit = enemyGo.GetComponent<Unit>();
        
        // Set up HUDs
        playerHUD.SetHUD(_playerUnit);
        enemyHUD.SetHUD(_enemyUnit);

        yield return new WaitForSeconds(2f);
        
        state = BattleState.PLAYERTURN;
        PlayerTurn();
    }
    
    /// <summary>
    /// This will begin the players turn
    /// </summary>
    private void PlayerTurn()
    {
        Debug.Log("Player turn, choose an action..");
    }

    private IEnumerator PlayerAttack()
    {
        // Attack the enemy
        
        yield return new WaitForSeconds(2f);
        
        // Check if the enemy is dead
        
        // Change state based off that
    }
    
    /// <summary>
    /// This will teleport the player to the battle scene point 
    /// </summary>
    private void TeleportPlayerToBattlePoint()
    {
        if (playerCC == null || playerPoint == null)
        {
            Debug.LogWarning("BattleSystem is missing a player or player point reference.");
            return;
        }

        playerCC.enabled = false;
        playerCC.transform.SetPositionAndRotation(playerPoint.position, playerPoint.rotation);
        playerCC.enabled = true;
    }
    
    /// <summary>
    /// This will lock the player's movement during the battle
    /// </summary>
    private void LockPlayer()
    {
        if (playerInput == null)
        {
            Debug.LogWarning("Player input is missing.");
        }
        playerInput.enabled = false;
        
    }
    
    /// <summary>
    /// This method will help to set up the player camera during the battle scene
    /// </summary>
    private void SetUpCamera(bool useBattleCamera)
    {
        Debug.Log("camera...");
        playerCamera.SetActive(!useBattleCamera);
        battleCamera.SetActive(useBattleCamera);
        
        Cursor.lockState = useBattleCamera ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = useBattleCamera;
        
        HUDController.instance.SetCrosshair(useBattleCamera);
        HUDController.instance.SetBattleHUD(useBattleCamera);
    }
    
}
