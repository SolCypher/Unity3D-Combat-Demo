using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour
{
    public float playerProjectile_BaseSpeed = 30f;

    [Header("Dmg Value")]
    public float baseDmg = 10f;
    float player_BaseDmgMultiplier = 1f;
    public float fireDistance;

    public enum DmgType{
        BasicAtk,
        Meteor,
        Test,
    }

    public DmgType dmgType;

    Rigidbody rb;
    Enemy enemy;
    PlayerStats playerStats;
    Animator enemyAnimator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        // Set this GameObj's tag to be PlayerProjectile
        if(gameObject.tag != "PlayerProjectile"){
            gameObject.tag = "PlayerProjectile";
        }

    }

    private void Update() {
        // Calculate the Projectile's Distance from Fire Point, if exceeded the Max Range, return it to pool
        fireDistance = Vector3.Distance(transform.position, PlayerManager.instance.player.GetComponent<PlayerController>().firePoint.position);
        if(fireDistance >= PlayerManager.instance.player.GetComponent<PlayerController>().projectileMaxRange){
            ObjPoolManager.ReturnObjToPool(gameObject);
        }

        // Rotate the projectile according to the velocity
        transform.rotation = Quaternion.LookRotation(rb.velocity);

    }

    private void OnTriggerEnter(Collider other) {
        #region Unused (Old Logic, when takeDmg on the Enemy.cs)
        // Return the obj to pool if it hit anything but an Obj w/ Enemy Tag
        // If it hit Enemy, it will be handled in Enemy.cs
        // if(other.gameObject.tag != "Enemy"){
        //     ObjPoolManager.ReturnObjToPool(gameObject);
        //     // Debug.Log("Hit " + other.gameObject.name);

        // }
        #endregion

        if(other.gameObject.tag == "Enemy"){
            enemy = other.GetComponent<Enemy>();
            enemyAnimator = enemy.animator;

            DmgEnemy(enemy, gameObject);
            // Debug.Log("Hit Enemy");

        }else if(other.gameObject.tag != "EnemyProjectile" && other.gameObject.tag != "Player"){
            ObjPoolManager.ReturnObjToPool(gameObject);
            // Debug.Log("Hit " + other.gameObject.name);

        }

    }

    void DmgEnemy(Enemy enemy, GameObject projectileObj){
        // Dmg Type checking + deal dmg based on that Dmg Type
        if(dmgType == DmgType.BasicAtk){
            // Dmg Calculation Formula
            enemy.enemy_CurrHP -= baseDmg * (player_BaseDmgMultiplier + playerStats.player_BonusDmg);
            if(!enemy.enemyIsDead){
                enemyAnimator.SetTrigger("Get Hit Front");

            }
            // Debug.Log(playerStats.player_BonusDmg);

        }else if(dmgType == DmgType.Meteor){
            enemy.enemy_CurrHP -= baseDmg * (player_BaseDmgMultiplier + playerStats.player_BonusDmg);
            if(!enemy.enemyIsDead){
                enemyAnimator.SetTrigger("Get Hit Front");

            }
        }

        if(enemy.enemy_CurrHP <= 0 && !enemy.enemyIsDead){
            enemy.EnemyDie();

        }

        // Return it the projectile to pool
        ObjPoolManager.ReturnObjToPool(projectileObj);

    }

}
