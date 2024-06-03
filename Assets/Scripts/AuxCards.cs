using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;



[CreateAssetMenu(fileName = "New Aux Card", menuName = "Aux Card")]
public class AuxCards : Cards
{

    public MonoScript modification;

    public void applyMod(ActionCards actionCard)
    {
        System.Type scriptType = modification.GetClass();
        if (scriptType != null)
        {
            actionCard.GetInstance().gameObject.AddComponent(scriptType);
        }

    }



}

