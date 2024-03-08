using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class Cards : ScriptableObject {
    [SerializeField] string cardName;
    [SerializeField] private int delay;
    [SerializeField] private float manaCost;
    [SerializeField] private Sprite cardImage;

    public int getDelay() {
        return this.delay;
    }
    public float getManaCost()
    {
        return this.manaCost;
    }
    public string getCardName()
    {
        return this.cardName;
    }
    public Sprite getCardImage()
    {
        return this.cardImage;
    }

}
