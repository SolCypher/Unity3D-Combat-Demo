using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorldManager : MonoBehaviour
{
    [Header("Skybox")]
    public Material skyboxDayMat;
    public Material skyboxNightMat;

    [Header("Lighting")]
    public List<Light> dayLighting = new List<Light>();
    public List<Light> nightLighting = new List<Light>();
    
    public enum TimeOfDay{
        Day,
        Night,
    }

    public void SetDaytime(){
        ChangeTimeCycle(TimeOfDay.Day);
    }
    public void SetNightTime(){
        ChangeTimeCycle(TimeOfDay.Night);
    }

    void ChangeTimeCycle(TimeOfDay timeOfDay){
        if(timeOfDay == TimeOfDay.Day){
            RenderSettings.skybox = skyboxDayMat;
            SetLighting(nightLighting, false);
            SetLighting(dayLighting, true);

        }else if(timeOfDay == TimeOfDay.Night){
            RenderSettings.skybox = skyboxNightMat;
            SetLighting(dayLighting, false);
            SetLighting(nightLighting, true);

        }

    }
    
    void SetLighting(List<Light> lightList, bool status){
        for(int i = 0; i < lightList.Count; i++){
            lightList[i].gameObject.SetActive(status);
        }
    }

}
