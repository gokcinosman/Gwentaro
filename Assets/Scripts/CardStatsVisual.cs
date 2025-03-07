using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CardStatsVisual : MonoBehaviour
{
    public CardStats cardStats;
    public Image cartSprite;
    public Image cardTypeSprite;
    public Image cardClassSprite;
    public Image cardHeroValueSprite;
    public Image cardNormalValueSprite;
    public TextMeshProUGUI cardValueText;
    public TextMeshProUGUI cardNameText;
    public TextMeshProUGUI cardDescText;

    private void SetValueSprite()
    {
        if (cardStats.cardStatue == CardStatus.Hero)
        {
            cardNormalValueSprite.gameObject.SetActive(false);
            cardHeroValueSprite.gameObject.SetActive(true);

        }
        else if (cardStats.cardStatue == CardStatus.Special)
        {
            cardNormalValueSprite.gameObject.SetActive(true);
            cardHeroValueSprite.gameObject.SetActive(false);
            cardNormalValueSprite.sprite = cardStats.specialSpriteIcon;
        }
        else if (cardStats.cardStatue == CardStatus.Leader)
        {
            cardNormalValueSprite.gameObject.SetActive(false);
            cardHeroValueSprite.gameObject.SetActive(false);
        }
        else
        {
            cardNormalValueSprite.gameObject.SetActive(true);
            cardHeroValueSprite.gameObject.SetActive(false);
        }
    }
    private void SetValueText()
    {
        if (cardStats.cardType == CardType.None)
        {
            cardValueText.text = "";
        }
        else
        {
            cardValueText.text = cardStats.cardValue.ToString();
        }
    }
    private void SetCardTypeSprite()
    {
        switch (cardStats.cardType)
        {
            case CardType.Melee:
                cardTypeSprite.sprite = cardStats.meleeSpriteIcon;
                break;
            case CardType.Ranged:
                cardTypeSprite.sprite = cardStats.rangedSpriteIcon;
                break;
            case CardType.Siege:
                cardTypeSprite.sprite = cardStats.siegeSpriteIcon;
                break;
            default:
                cardTypeSprite.gameObject.SetActive(false);
                break;
        }
    }
    private void SetCardClassSprite()
    {
        switch (cardStats.cardClass)
        {
            case CardClass.Agile:
                cardClassSprite.sprite = cardStats.agileSpriteIcon;
                break;

            case CardClass.Medic:
                cardClassSprite.sprite = cardStats.medicSpriteIcon;
                break;
            case CardClass.MoraleBooster:
                cardClassSprite.sprite = cardStats.moraleBoosterSpriteIcon;
                break;
            case CardClass.Muster:
                cardClassSprite.sprite = cardStats.musterSpriteIcon;
                break;
            case CardClass.Spy:
                cardClassSprite.sprite = cardStats.spySpriteIcon;
                break;
            case CardClass.TightBond:
                cardClassSprite.sprite = cardStats.tightBondSpriteIcon;
                break;
            case CardClass.None:
                cardClassSprite.gameObject.SetActive(false);
                break;
            default:
                cardClassSprite.gameObject.SetActive(false);
                break;
        }
    }
    private void SetCardName()
    {
        cardNameText.text = cardStats.cardName;
    }
    private void SetCardDescription()
    {
        cardDescText.text = cardStats.cardDesc;
    }
    private void SetCardSprite()
    {
        cartSprite.sprite = cardStats.cardSprite;
    }
}
