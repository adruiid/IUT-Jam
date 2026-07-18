using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/Cookable Item")]
public class CookableItem : Items
{
    [Header("Requirement")]
    public List<ResourceRequirement> requirements;

    public Items resultingItem;
}