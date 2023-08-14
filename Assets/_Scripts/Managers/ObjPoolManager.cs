using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjPoolManager : MonoBehaviour
{
    /// Notes
    /// - Currently separating each pool by the obj's name
    /// - The pool size will adjust itself if the current pool size isn't enough, e.g 5 unused already created but the user managed to need to use 6
    ///     the pool will automatically add new unused obj to adjust it meaning it will be 6 unused obj to compensate the need to use 6

    // List to store each pool
    public static List<PooledObjInfo> objPools = new List<PooledObjInfo>();

    GameObject poolParentHolder; // Parent for the PoolType, to store the Parent of each Pool Type

    static GameObject gameObjPoolParent; // Parent for the Gameobj, to Store the gameobjs that spawned for the Gameobj Pool

    // Type for Parent that will be set to the Game Obj
    public enum PoolType{
        GameObject,
        None,

    }
    public static PoolType PoolingType;

    private void Awake() {
        SetupParentPool();

    }

    // Initialization for Parent Pool
    void SetupParentPool(){
        poolParentHolder = new GameObject("Pooled Objs");

        gameObjPoolParent = new GameObject("GameObjs Pool");
        gameObjPoolParent.transform.SetParent(poolParentHolder.transform);

    }

    public static GameObject SpawnObj(GameObject objToSpawn, Vector3 spawnPos, Quaternion spawnRot, PoolType poolType = PoolType.None){
        // Search the objPools list for a pool that have the same tag(tag in PooledObjInfo) as the searched obj's name (objToSpawn)
        PooledObjInfo pool = null;
        foreach(PooledObjInfo p in objPools){
            if(p.tag == objToSpawn.name){
                pool = p;
                break;

            }
        }

        // If the pool doesn't exist, create new w/ a tag of the searched obj's name 
        if(pool == null){
            pool = new PooledObjInfo(){ tag = objToSpawn.name};
            objPools.Add(pool);
            // Debug.Log(pool.tag);

        }

        // Check if there are unused Obj in the pool
        GameObject spawnableObj = null;
        foreach(GameObject obj in pool.unusedObj){
            if(obj != null){
                spawnableObj = obj;
                break;

            }
        }
        
        if(spawnableObj == null){
            // Setting the parent for the obj, based on the inserted PoolType
            GameObject parentObj = SetParentObj(poolType);

            // If unused obj isn't found, create new
            spawnableObj = Instantiate(objToSpawn, spawnPos, spawnRot);
            
            // If a parent want to be set, set it
            if(parentObj != null){
                spawnableObj.transform.SetParent(parentObj.transform);

            }

        }else{
            // If unused obj is found, reactivate it
            spawnableObj.transform.position = spawnPos;
            spawnableObj.transform.rotation = spawnRot;
            pool.unusedObj.Remove(spawnableObj);
            spawnableObj.SetActive(true);

        }

        return spawnableObj;

    }
    
    public static void ReturnObjToPool(GameObject obj){
        // Remove the (Clone) string from the obj's name, because a copy of the same obj will have that string
        string objName = obj.name.Substring(0, obj.name.Length - 7); 

        // Check if there are any pool w/ the same tag as the searched obj
        PooledObjInfo pool = null;
        foreach(PooledObjInfo p in objPools){
            // Debug.Log("1. " + p.tag);
            // Debug.Log("2. " + objName);
            if(p.tag == objName){
                pool = p;
                // Debug.Log(pool.tag);
                break;

            }
        }

        // If not found, return error because trying to spawn an obj that's not pooled yet
        if(pool == null){
            Debug.LogWarning(obj.name + " obj isn't pooled yet");
            // Debug.LogWarning("Pool name: " + pool.tag);

        }else{
            // If found Deactivate the obj and return it to the pool
            obj.SetActive(false);
            pool.unusedObj.Add(obj);

        }

    }

    static GameObject SetParentObj(PoolType poolType){
        switch(poolType){
            case PoolType.GameObject:
                return gameObjPoolParent;

            case PoolType.None:
                return null;
            
            default:
                return null;

        }

    }

}


// Class for Pool Obj, 1 PooledObjInfo = 1 pool of that objects
public class PooledObjInfo{
    public string tag;
    public List<GameObject> unusedObj = new List<GameObject>();

}
