using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItemData", menuName = "GMTK2026/Item Data")]
public class ItemData : ScriptableObject
{
    public string itemName;
    public Sprite sprite;
    public Sprite defectSprite;

    public float basePrice;

    public List<ItemMaterial> materials;

    public List<ItemDefect> defects;
    public List<MysticTrait> mysticTraits;
}
