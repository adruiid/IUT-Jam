using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Objects/Item")]
public class Items : ScriptableObject
{
    public string itemName;
    public Sprite sprite;
    public ItemType type;
    public bool stackable;
    [TextArea(2, 4)]
    public string description;

    [Header("For tools only")]
    public float durabillity;

    [Header("For equipment only")]
    public float speedBoost;
    public float hpUpgade;

}

public enum ItemType
{
    Consumable,
    Weapon,
    Resource,
    Tool,
    Equipment
}
