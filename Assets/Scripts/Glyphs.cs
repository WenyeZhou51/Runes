using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "New Glyph", menuName = "Glyph")]
public class Glyphs : ScriptableObject
{
    [SerializeField] private string description;
    [SerializeField] private ScriptableObject card;
    [SerializeField] private Sprite glyphImage;

    public string getGlyphDescription() {
        return this.description;
    }
    public ScriptableObject getGlyphCard()
    {
        return this.card;
    }
    public Sprite getGlyphImage()
    {
        return this.glyphImage;
    }


}
