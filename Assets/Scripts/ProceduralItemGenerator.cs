using System.Collections.Generic;
using UnityEngine;

public static class ProceduralItemGenerator
{
    public static ItemData GenerateItem(ItemGeneratorConfig config)
    {
        ItemData item = ScriptableObject.CreateInstance<ItemData>();

        item.itemName = config.namePool != null && config.namePool.Length > 0
            ? config.namePool[Random.Range(0, config.namePool.Length)]
            : "Unknown Item";

        item.sprite = config.spritePool != null && config.spritePool.Length > 0
            ? config.spritePool[Random.Range(0, config.spritePool.Length)]
            : null;

        item.basePrice = Random.Range(config.basePriceRange.x, config.basePriceRange.y);
        item.defects = RollValues((ItemDefect[])System.Enum.GetValues(typeof(ItemDefect)), config.defectCountRange);
        item.mysticTraits = RollValues((MysticTrait[])System.Enum.GetValues(typeof(MysticTrait)), config.traitCountRange);

        return item;
    }

    private static List<T> RollValues<T>(T[] allValues, Vector2Int countRange)
    {
        List<T> pool = new List<T>(allValues);
        int count = Mathf.Clamp(Random.Range(countRange.x, countRange.y + 1), 0, pool.Count);
        List<T> result = new List<T>();

        for (int i = 0; i < count; i++)
        {
            int index = Random.Range(0, pool.Count);
            result.Add(pool[index]);
            pool.RemoveAt(index);
        }

        return result;
    }
}
