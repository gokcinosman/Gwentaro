using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class CardStats : MonoBehaviour
{
    public CardType cardType;
    public CardClass cardClass;
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