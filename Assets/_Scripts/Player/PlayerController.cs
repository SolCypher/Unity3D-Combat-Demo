using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    /// Notes
    /// PUT THIS SCRIPT at Player's Parent (Root)
    /// 
    
    [Header("Script References")]
    public ThirdPersonCam tpc;
    PlayerStats playerStats;

    [Header("Position")]
    public Transform firePoint;

    [Header("Projectiles Info")]
    public GameObject basicAtkProjectileObj;
    public GameObject skill_MeteorObj;
    public float projectileMaxRange = 100f;
    
    [Header("UI")]
    public Image crosshair;
    public GameObject gameOver_Overlay;

    [Header("Keybinds")]
    public KeyCode jump_Key = KeyCode.Space;
    public KeyCode thirdPersonMode_Key = KeyCode.Alpha0;
    public KeyCode combatMode_Key = KeyCode.Minus;
    public KeyCode basicAtk_Key = KeyCode.Mouse0;
    public KeyCode toggleCursor = KeyCode.LeftAlt;
    public KeyCode skillMeteor_Key = KeyCode.Alpha1;

    [Header("Skills")]
    public Image bscAtk_Icon;
    public Image skillMeteor_Icon;
    public Image skillMeteor_IconOutline;
    public LineRenderer meteorLine;

    Camera mainCam;

    Vector3 aimTarget;

    bool hitEnemy;

    // Ray meteorRay;
    RaycastHit meteorHit;

    // Add the boolean to check if in combat/not on this script?

    private void Start() {
        playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();

        // Initialization
        mainCam = Camera.main;
        // meteorRay = mainCam.ScreenPointToRay(crosshair.transform.position);

        // Initialize the skills cooldown
        SkillInit();
        
        // Hide Game Over Overlay
        gameOver_Overlay.SetActive(false);

        // Hide Cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    void Update()
    {
        if(!playerStats.isDead){
            // [Alive]

            GetInput();
            SkillsCooldownHandler();

            // Skill Meteor Handler
            if(playerStats.skillMeteor_Active){
                Skill_Meteor();
            }

        }else{
            // [Dead]
            
            // Show Game Over Overlay
            gameOver_Overlay.SetActive(true);

            // Show Cursor
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

        }

    }

#region Input Handler
    // Get Input from player
    void GetInput(){
        // Camera Mode Handler
        if(Input.GetKey(combatMode_Key)){
            StartCoroutine(tpc.SwitchCamMode(ThirdPersonCam.CameraMode.Combat));

        }
        if(Input.GetKey(thirdPersonMode_Key)){
            StartCoroutine(tpc.SwitchCamMode(ThirdPersonCam.CameraMode.Default));

        }

        // Skill Activation Handler
        if(Input.GetKeyUp(basicAtk_Key) && !playerStats.bscAtkIsCD && playerStats.inCombat && !playerStats.skillMeteor_Active){
            ActivateSkill(0);

        }
        if(Input.GetKeyUp(skillMeteor_Key) && !playerStats.skillMeteorIsCD && playerStats.inCombat && !playerStats.skillMeteor_Active){
            ActivateSkill(1);

        }

        // Cursor Toggle
        if(Input.GetKey(toggleCursor)){
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
            playerStats.atkingDisabled = true;

        }else if(Input.GetKeyUp(toggleCursor)){
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
            playerStats.atkingDisabled = false;
        }

    }
#endregion

#region Skills
    // Initialize Skills CD + Icon's fill amount
    void SkillInit(){
        // Basic Atk
        bscAtk_Icon.fillAmount = 0;
        playerStats.bscAtkIsCD = true;

        // Skill - Meteor
        playerStats.skillMeteorIsCD = true;
        skillMeteor_IconOutline.gameObject.SetActive(false);
        skillMeteor_Icon.fillAmount = 0;

    }

    // Handles the UI Indicator for skills CD + the boolean for IsCD
    void SkillsCooldownHandler(){
        // Basic Atk
        if(playerStats.bscAtkIsCD){
            bscAtk_Icon.fillAmount += (1 / playerStats.bscAtk_CD) * Time.deltaTime;
            
            if(bscAtk_Icon.fillAmount >= 1){
                playerStats.bscAtkIsCD = false; 

            }

        }
        
        // Skill Meteor
        if(playerStats.skillMeteorIsCD){
            skillMeteor_Icon.fillAmount += (1 / playerStats.skillMeteor_CD) * Time.deltaTime;
            
            if(skillMeteor_Icon.fillAmount >= 1){
                playerStats.skillMeteorIsCD = false; 

            }
        }

    }

    // Activate skill based on the given SkillCode
    void ActivateSkill(int skillCode){
        /// Notes
        /// 0 -> Basic Atk
        /// 1, 2, etc -> Skills in order, e.g. 1 for skill 1, etc

        if(playerStats.atkingDisabled){
            Debug.Log("Attacking is disabled");
            return;
        }

        // Debug.Log("SkillCode: " + skillCode);

        switch(skillCode){
            case 0:
                // Debug.Log("Basic Atk");
                BasicAtk();
                bscAtk_Icon.fillAmount = 0;
                playerStats.bscAtkIsCD = true;
                break;
            
            case 1:
                // BasicAtk auto disabled if SkillMeteor Active (At GetInput())
                skillMeteor_IconOutline.gameObject.SetActive(true);
                playerStats.skillMeteor_Active = true;
                break;

            default:
                Debug.LogWarning("Invalid Skill Code");
                break;

        }

    }
    
    // Basic Atk
    void BasicAtk(){
        // Shoot Fireball as Basic Atk
        
        // Raycast's ray setup
        Ray ray = mainCam.ScreenPointToRay(crosshair.transform.position);
        RaycastHit hit;

        // Check raycast hits
        if(Physics.Raycast(ray, out hit)){
            // If Raycast hit something

            // Debug.Log("Atk Hit: " + hit.transform.gameObject);
            aimTarget = hit.point;

        }else{
            // If Raycast didnt hit anything
            aimTarget = ray.GetPoint(projectileMaxRange);

        }
        
        // Spawn the projectile
        GameObject projectile = ObjPoolManager.SpawnObj(basicAtkProjectileObj, firePoint.position, Quaternion.identity, ObjPoolManager.PoolType.GameObject);
        projectile.GetComponent<Rigidbody>().velocity = (aimTarget - firePoint.position).normalized * projectile.GetComponent<PlayerProjectile>().playerProjectile_BaseSpeed;

    }
    
    // Skill Meteor
    void Skill_Meteor(){
        // Summon Meteor

        // Setup the LineRenderer for Meteor's Line Indicator 
        meteorLine.enabled = true;
        meteorLine.SetPosition(0, firePoint.position);
        meteorLine.SetPosition(1, firePoint.position + (Camera.main.transform.forward * 200f));

        // Setup racyast
        Ray meteorRay = mainCam.ScreenPointToRay(crosshair.transform.position);

        // Check raycast hits
        if(Physics.Raycast(meteorRay, out meteorHit)){
            // Debug.Log(meteorHit.transform.gameObject.name);

            // If Raycast hit enemy / ground / player/ nothing
            if(meteorHit.transform.gameObject.tag == "Player"){
                return;
            }

            if(meteorHit.transform.gameObject.tag == "Enemy"){
                hitEnemy = true;

                meteorLine.SetPosition(1, meteorHit.point);
                aimTarget = meteorHit.point;
                // Debug.Log("Hit something");

            }else if(meteorHit.transform.gameObject.tag == "Ground"){
                hitEnemy = false;

                meteorLine.SetPosition(1, meteorHit.point);
                aimTarget = meteorHit.point;
                // Debug.Log("Hit ground");

            }else{
                hitEnemy = false;
                aimTarget = meteorRay.GetPoint(projectileMaxRange);
                // Debug.Log("Hit nothing");
            }

        }

        if(Input.GetKeyUp(KeyCode.Mouse0)){
            // Spawn meteor
            // Debug.Log("Spawn Meteor");

            // Set the SpawnPoint of meteor + Spawn the Projectile
            Vector3 meteorSpawnPoint = PlayerManager.instance.player.transform.position + new Vector3(0f, 50f, 0f);
            // Debug.Log("Meteor Spawn: " + meteorSpawnPoint);
            GameObject meteorProjectile = ObjPoolManager.SpawnObj(skill_MeteorObj, meteorSpawnPoint, Quaternion.identity, ObjPoolManager.PoolType.GameObject);

            // If destination set to enemy, set the enemy's position as the destination
            if(hitEnemy){
                meteorProjectile.GetComponent<Rigidbody>().velocity = (meteorHit.transform.position - meteorSpawnPoint).normalized * 10f;
                // Debug.Log("Enemy's Pos: " + (meteorHit.transform.position - meteorSpawnPoint));

            }else{
                // If not enemy, set the Hit.Point as the destination
                meteorProjectile.GetComponent<Rigidbody>().velocity = (aimTarget - meteorSpawnPoint).normalized * 10f;

            }

            // Set the CD + Icon's fill
            skillMeteor_Icon.fillAmount = 0;
            playerStats.skillMeteorIsCD = true;
            skillMeteor_IconOutline.gameObject.SetActive(false);
    
            // Reset
            playerStats.skillMeteor_Active = false;
            meteorLine.enabled = false;
            hitEnemy = false;

        }else if(Input.GetKeyUp(KeyCode.Mouse1)){
            // Reset (To cancel casting the skill)
            playerStats.skillMeteor_Active = false;
            meteorLine.enabled = false;
            skillMeteor_IconOutline.gameObject.SetActive(false);
            hitEnemy = false;
            Debug.Log("Meteor Skill Canceled");
        }

    }

#endregion

    // [To see the raycast]
    // private void OnDrawGizmosSelected() {
    //     Ray ray = mainCam.ScreenPointToRay(crosshair.transform.position);
    //     Gizmos.color = Color.green; 
    //     Gizmos.DrawRay(ray.origin, ray.direction * 100f);
        
    //     Ray meteorRay = mainCam.ScreenPointToRay(crosshair.transform.position);
    //     Gizmos.color = Color.blue; 
    //     Gizmos.DrawRay(meteorRay.origin, meteorRay.direction * 100f);

    // }

}
