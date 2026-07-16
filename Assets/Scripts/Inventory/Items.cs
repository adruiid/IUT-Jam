using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Objects/Item")]
public class Items : ScriptableObject
{
    public string itemName;
    public Sprite sprite;
    public ItemType type;
    public bool stackable;

}

public enum ItemType
{
    Consumable,
    Weapon,
    Resource,
    Tool
}
