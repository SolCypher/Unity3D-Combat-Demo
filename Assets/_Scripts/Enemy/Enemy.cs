using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Enemy : MonoBehaviour
{
    public NavMeshAgent agent;

    Transform player;

    [Header("Animation")]
    public Animator animator;
    // For rotation when Buff(Roar) animation is played
    public float turnDuration = 0.5f;
    Quaternion initRot;

    [Header("Enemy HP")]
    public float enemy_BaseHP = 100f;
    public float enemy_CurrHP;

    [Header("Enemy Range Detection")]
    public float lookRadius = 10f;
    public float atkRadius = 8f;
    public float patrolRadiusX = 10f;
    public float patrolRadiusZ = 5f;
    public float patrolDelay = 0.5f;

    public bool playerInSight, playerInAtk;

    bool patrolPointSet;
    bool reachedDestination;
    bool roarDone; // For animation

    int inSightCount; // For animation

    float DistanceToPlayer;
    Vector3 patrolDestination;

    [Header("Enemy Stats")]
    public float enemy_BonusDmg = 1f;
    public float enemy_Spd = 10f;
    public bool enemyIsDead;

    [Header("Enemy Attack")]
    public Transform enemyFirePoint;
    public GameObject enemyProjectile;
    public float enemyAtkInterval = 0.5f;
    public bool alreadyAtk;

    // PlayerProjectile plyrProjectile; (Unused, Logic moved to respective Projectile.cs)

    private void Awake() {
        agent = GetComponent<NavMeshAgent>();

    }

    private void Start() {
        // Get Player Singleton
        player = PlayerManager.instance.player.transform; // Dont put it on Awake(), it will cause Script Execution Order Issue

        enemy_CurrHP = enemy_BaseHP;
        agent.speed = enemy_Spd;
        
        reachedDestination = true;

    }

    private void Update() {
        // Calculate the distance between the player + set the boolean respectively
        DistanceToPlayer = Vector3.Distance(player.position, transform.position);
        playerInSight = DistanceToPlayer <= lookRadius;
        playerInAtk = DistanceToPlayer <= atkRadius;

        // If player is alive, do Action based on the result of the booleans
        if(!PlayerManager.instance.player.GetComponent<PlayerStats>().isDead && !enemyIsDead){
            if(!playerInSight && !playerInAtk){
                Patrolling();

            }

            if(playerInSight){
                inSightCount++;
                if(inSightCount <= 1){
                    StartCoroutine("RoarAnimation");
                }
            }

            if(playerInSight && !playerInAtk && roarDone){
                // StartCoroutine("ChasePlayer");
                ChasePlayer();

            }
            if(playerInSight && playerInAtk && roarDone){
                AtkPlayer();

            }
        }
        

    }

    IEnumerator RoarAnimation(){
        // Stop moving
        agent.SetDestination(transform.position);
        // Rotate the enemy towards player for animation
        initRot = transform.rotation;
        float timePassed = 0f;

        while(timePassed < turnDuration){
            float normalizedTime = timePassed / turnDuration;

            Quaternion targetRot = Quaternion.LookRotation(player.position - transform.position);
            transform.rotation = Quaternion.Slerp(initRot, targetRot, normalizedTime);

            timePassed += Time.deltaTime;

            yield return null;

        }

        // Only trigger Animation(Buff, the roaring one), when the player is encountered first time
        // Or if the player run away (out of sight range), then returned
        // Set Animation
        animator.SetBool("Idle", false);
        animator.SetBool("Combat Idle", false);
        animator.SetBool("Run Forward", false);
        animator.SetTrigger("Buff");

        yield return new WaitForSeconds(1.4f);
        roarDone = true;
        
    }

    #region Detect Projectile Logic (Unused, moved to respective Projectile.cs)
    // To check if a PlayerProjectile entered the collider (For taking dmg)
    // private void OnTriggerEnter(Collider other) {
    //     // Debug.Log(other.gameObject.name);
    //     // Check if the object that enter is an object w/ PlayerProjectile Tag
    //     if(other.gameObject.tag == "PlayerProjectile"){
    //         plyrProjectile = other.gameObject.GetComponent<PlayerProjectile>();

    //         // EnemyTakeDamage(plyrProjectile.dmgType, plyrProjectile.gameObject);
    //         // Debug.Log("Hit by Projectile");
    //     }
        
    // }
    #endregion

#region Enemy Actions

    #region Take Dmg Logic (Unused, moved to respective Projectile.cs)
    // // The GameObject projectileObj is used to return the GameObject to the pool
    // void EnemyTakeDamage(PlayerProjectile.DmgType dmgType, GameObject projectileObj){
    //     // Check whether the plyrProjectile null / not
    //     if(plyrProjectile == null){
    //         Debug.LogWarning("Player Projectile isn't found");
    //         return;

    //     }

    //     // Dmg Type checking + deal dmg based on that Dmg Type
    //     if(dmgType == PlayerProjectile.DmgType.BasicAtk){
    //         enemy_CurrHP -= plyrProjectile.basicAtk_BaseDmg;

    //     }else{
    //         Debug.Log("Test");
    //     }

    //     if(enemy_CurrHP <= 0){
    //         EnemyDie();

    //     }

    //     // Return it the projectile to pool
    //     ObjPoolManager.ReturnObjToPool(projectileObj);

    // }
    #endregion

    public void EnemyDie(){
        //Add SFX & other FX here
        StartCoroutine("EnemyDeath");

    }
    IEnumerator EnemyDeath(){
        enemyIsDead = true;
        
        // Set Animation
        animator.SetBool("Idle", false);
        animator.SetBool("Combat Idle", false);
        animator.SetBool("Run Forward", false);
        animator.SetBool("Death", true);

        yield return new WaitForSeconds(3.5f);

        Destroy(gameObject);

    }

#endregion

#region Enemy Behaviour
    void Patrolling(){
        inSightCount = 0;
        roarDone = false;

        // Debug.Log("Patrolling to " + patrolDestination);
        // Set Patrol Destination if not set yet
        if(!patrolPointSet && reachedDestination){
            // SetPatrolPoint();
            reachedDestination = false;
            Invoke(nameof(SetPatrolPoint), patrolDelay);
        }

        // Go to the Destination if Destination is set
        if(patrolPointSet){
            // Move + Set respective animation for moving
            MoveEnemy();

            // Calculate distance + reset the Boolean if Destination has been reached
            Vector3 distanceToDestination = transform.position - patrolDestination;
            // Debug.Log("Magnitude: " + distanceToDestination.magnitude);
            if(distanceToDestination.magnitude < 2f){
                patrolPointSet = false;
                
                // Set Animation
                animator.SetBool("Run Forward", false);
                animator.SetBool("Idle", true);
                
                reachedDestination = true;
                // Debug.Log("Destination reached");
            }
        }
        
        

    }
    void SetPatrolPoint(){
        // Calculate Random Point for the Destination
        float RandomZ = Random.Range(-patrolRadiusZ, patrolRadiusZ);
        float RandomX = Random.Range(-patrolRadiusX, patrolRadiusX);
        // Debug.Log("Random X: " + RandomX);
        // Debug.Log("Random Z: " + RandomZ);

        // Set the Destination + set the Boolean to True
        patrolDestination = new Vector3(transform.position.x + RandomX, transform.position.y, transform.position.z + RandomZ);
        patrolPointSet = true;

    }
    void MoveEnemy(){
        // Set Animation
        animator.SetBool("Idle", false);
        animator.SetBool("Combat Idle", false);
        animator.SetBool("Run Forward", true);

        agent.SetDestination(patrolDestination);
        
    }
    

    void ChasePlayer(){
        // Set Animation
        animator.SetBool("Idle", false);
        animator.SetBool("Combat Idle", false);
        animator.SetBool("Run Forward", true);

        agent.SetDestination(player.position);
        // yield return null;

    }

    void AtkPlayer(){
        // Set Animation
        animator.SetBool("Idle", false);
        animator.SetBool("Run Forward", false);
        animator.SetBool("Combat Idle", true);

        // Stop moving the enemy + look at the player
        agent.SetDestination(transform.position);
        FacePlayer();
        
        // Atk code
        if(!alreadyAtk){
            alreadyAtk = true;

            // Play Atk Animation
            animator.SetTrigger("Attack3");

            Vector3 aimTarget = player.position;
            GameObject projectile = ObjPoolManager.SpawnObj(enemyProjectile, enemyFirePoint.position, Quaternion.identity, ObjPoolManager.PoolType.GameObject);
            // projectile.name = this.name + "_Projectile";
            projectile.GetComponent<EnemyProjectile>().SetEnemy(this);
            projectile.GetComponent<Rigidbody>().velocity = (aimTarget - enemyFirePoint.position).normalized * projectile.GetComponent<EnemyProjectile>().enemyProjectile_BaseSpd;

            // Atk according to the interval
            Invoke(nameof(ResetAtk),enemyAtkInterval);

        }
       
    }
    void ResetAtk(){
        alreadyAtk = false;

    }

    // Make the Enemy face the Player
    void FacePlayer(){
        Vector3 direction = (player.position - transform.position).normalized;
        Quaternion lookRot = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));

        transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 5f);

    }

#endregion

    private void OnDrawGizmosSelected() {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, lookRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, atkRadius);
    }

}
