using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArcanaSurge : MonoBehaviour
{
    public enum SurgeType
    {
        MidasTouch , PriceyBronze , RevolutionofBlacksmiths , Demeter , Jewelly , Major
    }
    public static SurgeType surgeType;

    public SurgeType RandomSurge()
    {
        Array values = Enum.GetValues(typeof(SurgeType));
        surgeType = (SurgeType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        return surgeType;
    }

    public float CalculateSurge(ItemData item)
    {
        switch (surgeType)
        {
            case SurgeType.MidasTouch:
                for (int i = 0 ; i < item.materials.Count ; i++)
                {
                    if (item.materials[i] == ItemMaterial.Ceramic)
                    {
                        item.materials[i] = ItemMaterial.Gold;
                        break;
                    }
                }
                if (item.materials.Contains(ItemMaterial.Gold))
                {
                    return 2.0f;
                }
                break;
            case SurgeType.PriceyBronze:
                for (int i = 0 ; i < item.materials.Count ; i++)
                {
                    if (item.materials[i] == ItemMaterial.Bronze)
                    {
                        return 3.0f;
                    }
                    else if (item.materials[i] == ItemMaterial.Silver)
                    {
                        return 1.5f;
                    }
                    else if (item.materials[i] == ItemMaterial.Gemstone)
                    {
                        return 0.2f;
                    }
                }
                break;
            case SurgeType.RevolutionofBlacksmiths:
                for (int i = 0 ; i < item.materials.Count ; i++)
                {
                    if (item.materials[i] == ItemMaterial.Metal)
                    {
                        return 1.5f;
                    }
                    else if (item.materials[i] == ItemMaterial.MonsterPart)
                    {
                        return 0.7f;
                    }
                }
                break;
            case SurgeType.Demeter:
                for (int i = 0 ; i < item.materials.Count ; i++)
                {
                    if (item.materials[i] == ItemMaterial.Ceramic)
                    {
                        return 3.0f;
                    }
                    else if (item.materials[i] == ItemMaterial.MonsterPart)
                    {
                        return 1.5f;
                    }
                }
                break;
            case SurgeType.Jewelly:
                if (item.materials.Contains(ItemMaterial.Gemstone))
                {
                    return 5.0f;
                }
                else if (item.materials.Contains(ItemMaterial.Gold) || item.materials.Contains(ItemMaterial.Silver) || item.materials.Contains(ItemMaterial.Bronze))
                {
                    return 0.8f;
                }
                break;
            case SurgeType.Major:
                return 1.5f;
        }
        return 1f;
    }
}
