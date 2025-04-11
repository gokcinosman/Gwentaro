using UnityEngine;
using UnityEngine.UI;
public class CardBack : MonoBehaviour
{
    [SerializeField] private Sprite cardBackSprite; // Kart arkası görseli
    [SerializeField] private Image image; // Sprite renderer bileşeni
    private void Awake()
    {
        // Sprite renderer'ı kontrol et
        if (image == null)
        {
            image = GetComponent<Image>();
            if (image == null)
            {
                image = gameObject.AddComponent<Image>();
            }
        }
        // Kart arkası görselini ayarla
        if (cardBackSprite != null)
        {
            image.sprite = cardBackSprite;
        }
        else
        {
            Debug.LogWarning("[CardBack] Kart arkası görseli atanmamış!");
        }
    }
}