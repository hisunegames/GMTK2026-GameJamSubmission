using System.Collections.Generic;

public static class ItemMaterialInfo
{
    private static readonly Dictionary<ItemMaterial, float> Multipliers = new Dictionary<ItemMaterial, float>
    {
        { ItemMaterial.Gold, 1.3f },
        { ItemMaterial.Silver, 1.15f },
        { ItemMaterial.Bronze, 1.05f },
        { ItemMaterial.Metal, 1f },
        { ItemMaterial.Ceramic, 1f },
        { ItemMaterial.Gemstone, 1.4f },
        { ItemMaterial.MonsterPart, 1.25f },
    };

    public static float GetMultiplier(ItemMaterial material)
    {
        return Multipliers.TryGetValue(material, out float multiplier) ? multiplier : 1f;
    }
}
