using Photon.Realtime;
using Photon.Pun;
using UnityEngine;
public class Card : MonoBehaviourPun
{
    public int power;
    public CardType type;
    public Faction faction;
    [PunRPC]
    public void PlayCard(int rowIndex)
    {
        // Kart oynama mantığı
        GameManager.Instance.photonView.RPC("UpdateGameState", RpcTarget.All, rowIndex, power);
    }
    public enum CardType { Unit, Special, Weather }
    public enum Faction { Northern, Nilfgaardian, Scoiatael, Monster }
}