using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemBundle", menuName = "GMTK2026/Item Bundle")]
public class ItemBundle : ScriptableObject
{
    public List<ItemData> items;
}
