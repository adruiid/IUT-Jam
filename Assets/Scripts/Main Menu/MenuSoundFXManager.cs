using UnityEngine;

public class MenuSoundFXManager : MonoBehaviour
{
    public static MenuSoundFXManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }



}
