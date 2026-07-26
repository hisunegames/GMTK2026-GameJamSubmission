using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ArcanaSurge : MonoBehaviour
{
    public enum SurgeType
    {
        MidasTouch , PriceyBronze , RevolutionofBlacksmiths , Demeter , Jewelly , Major
    }
    [SerializeField] public Sprite[] surgeimages;
    public static SurgeType surgeType;
    public static bool IsActive { get; private set; }
    public static ArcanaSurge instance;

    public SurgeType RandomSurge()
    {
        Array values = Enum.GetValues(typeof(SurgeType));
        surgeType = (SurgeType)values.GetValue(UnityEngine.Random.Range(0, values.Length));
        IsActive = true;
        return surgeType;
    }

    public void ResetSurge()
    {
        IsActive = false;
    }

    public static string GetDisplayName(SurgeType type)
    {
        switch (type)
        {
            case SurgeType.MidasTouch: return "Midas Touch";
            case SurgeType.PriceyBronze: return "Pricey Bronze";
            case SurgeType.RevolutionofBlacksmiths: return "Revolution of the Blacksmiths";
            case SurgeType.Demeter: return "Blessed by Demeter";
            case SurgeType.Jewelly: return "Jewelry Eras";
            case SurgeType.Major: return "Major Arcana";
            default: return type.ToString();
        }
    }

    public static string GetDescription(SurgeType type)
    {
        switch (type)
        {
            case SurgeType.MidasTouch:
                return "Gold fever grips the market - every ceramic piece turns to gold, and gold itself sells for double.";
            case SurgeType.PriceyBronze:
                return "Bronze takes the gemstone's throne - bronze x3, silver x1.5, gemstones fall to x0.2.";
            case SurgeType.RevolutionofBlacksmiths:
                return "Metalwork is in high demand (x1.5), while monster parts fall out of favor (x0.7).";
            case SurgeType.Demeter:
                return "Ceramics are blessed by the harvest (x3), monster parts see modest gains (x1.5).";
            case SurgeType.Jewelly:
                return "Gemstones are prized above all else (x5), but gold, silver, and bronze dip (x0.8).";
            case SurgeType.Major:
                return "A rare omen - every material's price rises by 50%.";
            default:
                return string.Empty;
        }
    }
    
    private void Awake()
    {
        instance = this;
    }

    public static Sprite GetSprite(SurgeType type)
    {
        if (instance != null && instance.surgeimages != null)
        {
            int index = (int)type;
            if (index >= 0 && index < instance.surgeimages.Length)
            {
                return instance.surgeimages[index];
            }
        }
        return null;
    }

    public float CalculateSurge(ItemData item)
    {
        if (!IsActive)
        {
            return 1f;
        }

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
