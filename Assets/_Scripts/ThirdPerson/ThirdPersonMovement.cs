using System.Collections;
using System.Collections.Generic;
using UnityEditor.Callbacks;
using UnityEngine;

public class ThirdPersonMovement : MonoBehaviour
{
    /// Notes
    /// PUT THIS SCRIPT at Player's Parent (Root)
    /// Player Hierarchy:
    /// Player (with Rigidbody + ATTACH this script here)
    ///     - PlayerObj (Player Model)
    ///     - Orientation (To store the direction, where its facing) 
    ///     - CamPos (For First Person, to store the Cam Pos) [Set Pos to (0, 0.5, 0)]
    
    // [Header("Script References")]
    PlayerController playerCtrl;
    PlayerStats playerStats;

    [Header("Movement")]
    public Transform orientation;

    public float groundDrag = 3f;
    public float jumpForce = 5f;
    public float jumpCoolDown = 0.25f;
    public float airMultiplier = 0.1f;

    [Header("Ground Checker")]
    public float playerHeight;
    public LayerMask groundLayer;

    bool isGrounded;
    bool isJumping;

    float horizontal;
    float vertical;
    float baseSpdMultiplier = 10f;

    Vector3 moveDir;
    Rigidbody rb;

    private void Start() {
        playerCtrl = PlayerManager.instance.player.GetComponent<PlayerController>();
        playerStats = PlayerManager.instance.player.GetComponent<PlayerStats>();
        rb = GetComponent<Rigidbody>();
        rb.freezeRotation = true;

    }

    private void Update() {
        // Ground Check
        isGrounded = Physics.Raycast(transform.position, Vector3.down, playerHeight * 0.5f + 0.2f, groundLayer);

        // If Player alive
        if(!PlayerManager.instance.player.GetComponent<PlayerStats>().isDead){
            GetInput();
            
        }

        // Drag Handler
        if(isGrounded){
            rb.drag = groundDrag;
        }else{
            rb.drag = 0;
        }

    }

    private void FixedUpdate() {
        MovePlayer();

    }

    void GetInput(){
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        if(Input.GetKey(playerCtrl.jump_Key) && !isJumping && isGrounded){
            isJumping = true;

            Jump(); // Do Jumping

            Invoke(nameof(ResetJump), jumpCoolDown); // Give cooldown for jumping

        }

    }

    void MovePlayer(){
        // Calculate Movement Direction
        moveDir = orientation.forward * vertical + orientation.right * horizontal;

        // Check if grounded
        if(isGrounded == true){
            rb.AddForce(moveDir.normalized * playerStats.moveSpd * baseSpdMultiplier, ForceMode.Force);

        }else{
            rb.AddForce(moveDir.normalized * playerStats.moveSpd * baseSpdMultiplier * airMultiplier, ForceMode.Force);

        }

    }

    void Jump(){
        // reset Y velocity
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        rb.AddForce(transform.up * jumpForce, ForceMode.Impulse);

    }
    
    void ResetJump(){
        isJumping = false;

    }

    void SpdController(){
        Vector3 velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);

        // Limit the speed
        if(velocity.magnitude > playerStats.moveSpd){
            Vector3 limit = velocity.normalized * playerStats.moveSpd;
            rb.velocity = new Vector3(limit.x, rb.velocity.y, limit.z);

        }

    }

}
