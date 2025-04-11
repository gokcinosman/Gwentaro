using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class RemaningCards : MonoBehaviour
{
    public TextMeshProUGUI remainingCardsText;
    public int remainingCardsCount;
    public GameObject cardBackPrefab;
    private float offSetY = 1f;
    public List<GameObject> cardBacks = new List<GameObject>();
    public void CreateCardBacks(int count)
    {
        for (int i = 0; i < count; i++)
        {
            GameObject cardBack = Instantiate(cardBackPrefab, transform);
            cardBack.transform.SetParent(transform);
            cardBack.transform.localPosition = new Vector3(0, i * offSetY, 0);
            cardBacks.Add(cardBack);
        }
    }
    public void RemoveCardBack(int cardsCount)
    {
        if (cardsCount > 0 && cardsCount <= cardBacks.Count)
        {
            for (int i = 0; i < cardsCount; i++)
            {
                GameObject cardBack = cardBacks[cardBacks.Count - 1];
                cardBacks.Remove(cardBack);
                UpdateRemainingCardsCount();
                Destroy(cardBack);
            }
        }
    }
    public void SetRemainingCardsCount(int count)
    {
        remainingCardsCount = count;
        UpdateRemainingCardsCount();
        CreateCardBacks(remainingCardsCount);
    }
    public void UpdateRemainingCardsCount()
    {
        remainingCardsText.text = remainingCardsCount.ToString();
    }
}
