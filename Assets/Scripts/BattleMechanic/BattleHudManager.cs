// This script is used to handle the battle hud information during the game
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BattleHudManager : MonoBehaviour
{
   public TMP_Text nameText;
   public TMP_Text levelText;
   public Slider healthSlider;
   
   
   
   public void SetHUD(Unit unit)
   {
      nameText.text = unit.unitName;
      levelText.text = "Lvl " + unit.unitLevel;
      healthSlider.maxValue = unit.maxHp;
      healthSlider.value = unit.currentHp;
   }

   public void SetHp(int hp)
   {
      healthSlider.value = hp;
   }
   
}
