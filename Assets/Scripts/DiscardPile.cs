using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class DiscardPile : MonoBehaviourPun
{
    private List<Card> discardedCards = new List<Card>();

    // Yığındaki kart sayısını döndürür
    public int GetCardCount()
    {
        return discardedCards.Count;
    }

    // Yığına kart ekler
    public void AddCard(Card card)
    {
        if (!discardedCards.Contains(card))
        {
            discardedCards.Add(card);
        }
    }

    // Yığından kart çıkarır (gerekirse)
    public void RemoveCard(Card card)
    {
        if (discardedCards.Contains(card))
        {
            discardedCards.Remove(card);
        }
    }

    // Yığından rastgele bir kart döndürür
    public Card GetRandomCard()
    {
        if (discardedCards.Count > 0)
        {
            int randomIndex = Random.Range(0, discardedCards.Count);
            return discardedCards[randomIndex];
        }
        return null;
    }

    // Yığını temizler
    public void ClearPile()
    {
        discardedCards.Clear();
    }
}