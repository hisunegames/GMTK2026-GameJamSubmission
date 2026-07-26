using UnityEngine;
using UnityEngine.UI;

public class MysticEyes : MonoBehaviour
{
    [SerializeField] GameObject Background;
    [SerializeField] GameObject Character;
    [SerializeField] Material invertMaterial;
    [SerializeField] Material normalMaterial;
    [SerializeField] Image aritfactImage;
    [SerializeField] Button mysticeyesButton;
    private bool mysticeyeisActive = false;
    private Material material;

    void Start()
    {
        mysticeyeisActive = false;
        if (mysticeyesButton != null)
        {
            mysticeyesButton.onClick.AddListener(OnClickToggleEyes);
        }
    }

    private void OnClickToggleEyes()
    {
        ItemData item = GameManager.Instance != null ? GameManager.Instance.LotController.CurrentItem : null;
        if (item == null)
        {
            Debug.Log("Item is null");
            return;
        }
        mysticeyeisActive = !mysticeyeisActive;
        Sprite itemsprite = mysticeyeisActive && item.mysticTraitSprite != null ? item.mysticTraitSprite : item.sprite;
        if (item.mysticTraitSprite != null)
        {
            aritfactImage.sprite = itemsprite;
        }
        SpriteRenderer[] bgRenderers = Background.GetComponentsInChildren<SpriteRenderer>(true);
        SpriteRenderer[] charRenderers = Character.GetComponentsInChildren<SpriteRenderer>();
        material = mysticeyeisActive ? invertMaterial : normalMaterial;
        foreach (SpriteRenderer bgsprite in bgRenderers)
        {
            bgsprite.material = material;
        }
        foreach (SpriteRenderer charsprite in charRenderers)
        {
            charsprite.material = material;
        }
    }
}
