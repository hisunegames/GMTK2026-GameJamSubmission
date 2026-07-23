using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "GMTK2026/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite sprite;

    [Range(0f, 1f)]
    public float trueQuality;

    public float basePrice;

    [TextArea(2, 4)]
    public List<string> clues;
}
