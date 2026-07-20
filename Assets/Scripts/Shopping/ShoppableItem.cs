using NUnit.Framework;
using System;
using UnityEngine;
using System.Collections.Generic;


[CreateAssetMenu(menuName = "Scriptable Objects/Shoppable Item")]
public class ShoppableItem : Items
{
    [Header("Requirement")]
    public List<ResourceRequirement> requirements;

    public Items resultingItem;

}