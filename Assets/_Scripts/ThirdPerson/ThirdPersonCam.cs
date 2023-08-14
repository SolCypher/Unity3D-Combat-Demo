using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ThirdPersonCam : MonoBehaviour
{
    /// Notes
    /// - Set the CombatLookAt position to be (1,1,0) in Inspector
    /// - Attach this script to the Camera
    
    // [Header("Script References")]
    PlayerController playerCtrl;
    PlayerStats playerStats;

    public Transform orientation;
    public Transform player;
    public Transform playerObj;
    public Rigidbody rb;

    public Transform combatLookAt;

    public float rotSpd = 7f;

    [Header("CameraMode Controller")]
    public GameObject thirdPersonCam;
    public GameObject combatCam;

    public CameraMode camMode;

    public enum CameraMode{
        Default,
        Combat,

    }

    void Start()
    {
        playerCtrl = PlayerManager.instance.player.GetComponent<PlayerController>();
        playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        
        StartCoroutine(SwitchCamMode(CameraMode.Default));

    }

    private void Update() {
        // Rotate orientation
        Vector3 viewDir = player.position - new Vector3(transform.position.x, player.position.y, transform.position.z);
        orientation.forward = viewDir.normalized;

        // If Player alive
        if(!PlayerManager.instance.player.GetComponent<PlayerStats>().isDead){
            // Rotate player Obj
            if(camMode == CameraMode.Default){
                float horizontal = Input.GetAxisRaw("Horizontal");
                float vertical = Input.GetAxisRaw("Vertical");
                Vector3 inputDir = orientation.forward * vertical + orientation.right * horizontal;

                if(inputDir != Vector3.zero){
                    playerObj.forward = Vector3.Slerp(playerObj.forward, inputDir.normalized, Time.deltaTime * rotSpd);
                }

            }else if(camMode == CameraMode.Combat){
                // Rotate orientation
                Vector3 combatDir = combatLookAt.position - new Vector3(transform.position.x, combatLookAt.position.y, transform.position.z);
                orientation.forward = combatDir.normalized;

                playerObj.forward = combatDir.normalized;

            }
        }
        

    }

    public IEnumerator SwitchCamMode(CameraMode mode){
        combatCam.SetActive(false);
        thirdPersonCam.SetActive(false);
        
        camMode = mode;

        if(mode == CameraMode.Default){
            thirdPersonCam.SetActive(true);
            playerCtrl.crosshair.gameObject.SetActive(false);
            playerStats.inCombat = false;

            yield return new WaitForSeconds(2f);
            

        }else if(mode == CameraMode.Combat){
            combatCam.SetActive(true);
            playerCtrl.crosshair.gameObject.SetActive(true);
            
            yield return new WaitForSeconds(2f);

            playerStats.inCombat = true;

        }

    }

}
