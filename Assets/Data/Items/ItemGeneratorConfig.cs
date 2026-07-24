using UnityEngine;

[CreateAssetMenu(fileName = "NewItemGeneratorConfig", menuName = "GMTK2026/Item Generator Config")]
public class ItemGeneratorConfig : ScriptableObject
{
    public string[] namePool;
    public Sprite[] spritePool;
    public Vector2 basePriceRange = new Vector2(50f, 200f);
    public Vector2Int defectCountRange = new Vector2Int(0, 3);
    public Vector2Int traitCountRange = new Vector2Int(0, 2);
}
