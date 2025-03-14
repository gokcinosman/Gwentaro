using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Card Name", menuName = "CardStats")]
public class CardStats : ScriptableObject
{
    public CardType cardType;
    public CardClass cardClass;
    public CardType targetRowType;
    public CardStatus cardStatue = CardStatus.Unit;
    public int cardValue;
    public string cardName;
    public string cardDesc;
    public Sprite cardSprite;
    public Sprite meleeSpriteIcon;
    public Sprite rangedSpriteIcon;
    public Sprite siegeSpriteIcon;
    public Sprite agileSpriteIcon;
    public Sprite heroSpriteIcon;
    public Sprite medicSpriteIcon;
    public Sprite moraleBoosterSpriteIcon;
    public Sprite musterSpriteIcon;
    public Sprite spySpriteIcon;
    public Sprite tightBondSpriteIcon;
    public Sprite specialSpriteIcon;



}
public enum CardType
{
    None,
    Melee,
    Ranged,
    Siege
}
public enum CardClass
{
    None,
    Agile,
    Medic,
    MoraleBooster,
    Muster,
    Spy,
    TightBond
}
public enum CardStatus
{
    Unit,
    Hero,
    Leader,
    Special
}