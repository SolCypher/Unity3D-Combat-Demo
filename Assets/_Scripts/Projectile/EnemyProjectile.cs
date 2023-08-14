using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyProjectile : MonoBehaviour
{
    public float enemyProjectile_BaseSpd = 30f;

    [Header("Dmg Value")]
    public float enemy_BaseDmg = 10f;

    Rigidbody rb;
    PlayerStats playerStats;
    Enemy enemy;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        // Set this GameObj's tag to be EnemyProjectile
        if(gameObject.tag != "EnemyProjectile"){
            gameObject.tag = "EnemyProjectile";
        }

    }

    private void Update() {
        // Rotate the projectile according to the velocity
        transform.rotation = Quaternion.LookRotation(rb.velocity);
    }

    private void OnTriggerEnter(Collider other) {
        if(other.gameObject.tag == "Player"){
            DmgPlayer(gameObject);

        }else if(other.gameObject.tag != "PlayerProjectile" && other.gameObject.tag != "Enemy" ){
            ObjPoolManager.ReturnObjToPool(gameObject);
            // Debug.Log("Hit " + other.gameObject.name);

        }

    }

    public void SetEnemy(Enemy enemy){
        this.enemy = enemy;
        
    }

    void DmgPlayer(GameObject projectileObj){
        // Dmg Calculation
        playerStats.player_CurrHP -= enemy_BaseDmg * enemy.enemy_BonusDmg;

        // Return it the projectile to pool
        ObjPoolManager.ReturnObjToPool(projectileObj);

    }

}
