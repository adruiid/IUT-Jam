using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(menuName = "Scriptable Objects/Craftable Item")]
public class CraftableItem : Items
{
    [Header("Requirement")]
    public List<ResourceRequirement> requirements;

    public Items resultingItem;

    public ResourceType resourceType;
}

[Serializable]
public class ResourceRequirement
{
    public ResourceType resourceType;
    public int amount;
}
