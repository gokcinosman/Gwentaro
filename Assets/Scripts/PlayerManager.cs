using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour
{
    public int playerId;
    public List<Card> hand = new List<Card>(); // Oyuncunun elindeki kartlar
    public bool hasPassed = false; // Pas geçip geçmediğini takip eder

    public void DrawCard(Card card)
    {
        hand.Add(card);
        Debug.Log($"Oyuncu {playerId} kart çekti: {card.cardStats.cardName}");
    }

    public void PlayCard(Card card)
    {
        if (hand.Contains(card))
        {
            hand.Remove(card);
            Debug.Log($"Oyuncu {playerId} kart oynadı: {card.cardStats.cardName}");
        }
    }
    public bool PlayCardOnRow(Card card, BoardRow row)
    {
        if (row.ownerPlayerId == playerId) // Eğer row oyuncuya aitse
        {
            return row.AddCard(card, playerId);
        }
        Debug.Log($"Bu satır oyuncu {row.ownerPlayerId}’a ait, oyuncu {playerId} buraya kart oynayamaz!");
        return false;
    }


     public void PassTurn()
    {
        hasPassed = true;
        Debug.Log($"Oyuncu {playerId} pas geçti!");
    }

    public int GetHandCount()
    {
        return hand.Count;
    }
}
