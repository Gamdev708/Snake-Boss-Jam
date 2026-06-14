using UnityEngine;
using FMODUnity;

public class FMODEvents : MonoBehaviour
{
    //References the Boss Music Loop from the FMOD Project
    [field: Header("Boss Music")]
    [field: SerializeField] public EventReference bossMusic {get; private set;}
    public static FMODEvents instance {get; private set;}
    
    void Awake()
    {
        if(instance != null)
        {
            Debug.Log("There is more than instance found!");
        }
        instance = this;    
    }


}
