using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MoveCamWithPlayer : MonoBehaviour
{
    /// Notes
    /// - Attach this to an empty GameObj, where that GO will be the parent of the Cam + make the Cam 
    ///     to be the child
    
    public Transform camPos;

    void Update()
    {
        transform.position = camPos.position;
        
    }
}
