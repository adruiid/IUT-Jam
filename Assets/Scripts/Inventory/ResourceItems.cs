using UnityEngine;


[CreateAssetMenu(menuName = "Scriptable Objects/Resource Item")]
public class ResourceItems : Items
{
    public ResourceType resourceType;
}
public enum ResourceType
{
    Wood,
    Stone,
    Iron,
    Leather,
    Meat,
    Metal,
    Screw,
    Rope,
    Toolkit,
    Chicken,
    Beef,
    Apple
}
