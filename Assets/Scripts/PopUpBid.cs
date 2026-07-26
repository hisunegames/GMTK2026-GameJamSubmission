using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PopUpBid : MonoBehaviour
{
    [SerializeField] GameObject[] npcobjects;
    [SerializeField] Animator[] animators;
    [SerializeField] InpectItemScript inpectItemScript;

    void Start()
    {
        foreach (GameObject x in npcobjects)
        {
            x.SetActive(false);  
        }
    }

    public void TriggerBidPopUp(NPCPersonality personality)
    {
        foreach (GameObject x in npcobjects)
        {
            x.SetActive(false);  
        }
        if (inpectItemScript.IsZoomedIn)
        {
            return;
        }

        switch (personality)
        {
            case NPCPersonality.Wizard:
                animators[0].SetBool("isBidding", true);
                npcobjects[0].SetActive(true);
                break;
            case NPCPersonality.Alchemist:
                animators[1].SetBool("isBidding", true);
                npcobjects[1].SetActive(true);
                break;
            case NPCPersonality.Noble:
                animators[2].SetBool("isBidding", true);
                npcobjects[2].SetActive(true);
                break;
            case NPCPersonality.Blacksmith:
                animators[3].SetBool("isBidding", true);
                npcobjects[3].SetActive(true);
                break;
        }
        foreach (Animator x in animators)
        {
            if (x.gameObject.activeSelf)
            {
                x.Rebind();
                x.Update(0f);
            }
        }
    }
    public void ResetChar()
    {
        foreach (GameObject x in npcobjects)
        {
            x.SetActive(false);  
        }
    }
}