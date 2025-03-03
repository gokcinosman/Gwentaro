using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TextUpdater : MonoBehaviour, IObserver
{
    private TextMeshProUGUI textComponent;
    public BoardRow boardRow;
    public bool isTotalScore = false;

    private ScoreManager scoreManager;

    void Awake()
    {
        textComponent = GetComponentInChildren<TextMeshProUGUI>();

    }

    private void Start()
    {
        // Debug bilgisi ekleyelim

        scoreManager = FindObjectOfType<ScoreManager>();
        if (scoreManager == null)
        {
            return;
        }

        if (isTotalScore)
        {
            scoreManager.AddObserver(this);

            // Başlangıç değeri göster
            textComponent.text = "0";
        }
        else if (boardRow != null)
        {
            boardRow.AddObserver(this);

            // Başlangıç değeri göster
            textComponent.text = boardRow.GetTotalPower().ToString();
        }

    }

    public void OnNotify(string message, BoardRow source)
    {

        if (textComponent != null)
        {
            if (isTotalScore)
            {
                // Toplam skor text'i
                textComponent.text = message;
            }
            else if (source != null && source == boardRow)
            {
                // BoardRow değer text'i
                textComponent.text = message;
            }
        }

    }

    private void OnDestroy()
    {
        if (isTotalScore && scoreManager != null)
        {
            scoreManager.RemoveObserver(this);
        }
        else if (boardRow != null)
        {
            boardRow.RemoveObserver(this);
        }
    }

    // Inspector'da değişiklik yapıldığında çağrılır
    private void OnValidate()
    {
        // İnspector'da isTotalScore değiştiğinde diğer ayarları güncelle
        if (isTotalScore)
        {
            boardRow = null; // Toplam skor gösterilecekse BoardRow bağlantısını kaldır
        }
    }
}