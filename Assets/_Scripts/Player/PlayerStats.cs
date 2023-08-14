using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    /// Notes
    /// PUT THIS SCRIPT at Player's Parent (Root)
    /// 
    
    [Header("Player HP")]
    public float player_BaseHP = 100f;
    public float player_CurrHP;
    
    [Header("Stats")]
    public float player_BonusDmg = 0f;
    public float moveSpd = 5f;
    public bool isDead = false;
    public bool inCombat;
    public bool atkingDisabled;
    // public bool basicAtk_Disabled;
    public bool skillMeteor_Active;

    [Header("Skills")]
    [Tooltip("Cooldown for the Basic Atk Skill")]
    public float bscAtk_CD = 0.5f;
    public bool bscAtkIsCD;
    public float skillMeteor_CD = 10f;
    public bool skillMeteorIsCD;

    private void Start() {
        player_CurrHP = player_BaseHP;

    }

    private void Update() {
        if(player_CurrHP <= 0){
            isDead = true;
            
        }

    }


}
