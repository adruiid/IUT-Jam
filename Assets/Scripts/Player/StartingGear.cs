using UnityEngine;

public class StartingGear : MonoBehaviour
{
    [SerializeField] private Items[] startingGears;

    private void Start()
    {
        foreach(Items item in startingGears)
        {
            InventoryManager.instance.AddItem(item);
        }
    }
}
