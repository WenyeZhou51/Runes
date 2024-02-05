using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Cards : ScriptableObject {
    public string cardName;
    public int cardCost;
    public Sprite cardImage;

    public int getCost() {
        return this.cardCost;
    }
}
