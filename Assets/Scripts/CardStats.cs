using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card Name", menuName = "CardStats")]
public class CardStats : ScriptableObject
{
    public CardType cardType;
    public CardClass cardClass;
    public int cardValue;
    public string cardName;
    public string cardDesc;
    public SpriteRenderer cardSprite;
    public SpriteRenderer meleeSpriteIcon;
    public SpriteRenderer rangedSpriteIcon;
    public SpriteRenderer siegeSpriteIcon;
    public SpriteRenderer agileSpriteIcon;
    public SpriteRenderer heroSpriteIcon;
    public SpriteRenderer medicSpriteIcon;
    public SpriteRenderer moraleBoosterSpriteIcon;
    public SpriteRenderer musterSpriteIcon;
    public SpriteRenderer spySpriteIcon;
    public SpriteRenderer tightBondSpriteIcon;


    public enum CardClass
    {
        None,
        Agile,
        Hero,
        Medic,
        MoraleBooster,
        Muster,
        Spy,
        TightBond
    }
}
public enum CardType
{
    None,
    Melee,
    Ranged,
    Siege
}