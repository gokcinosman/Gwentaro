using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour, ISubject
{
    private List<IObserver> observers = new List<IObserver>();
    private int totalScore = 0;
    private List<BoardRow> boardRows = new List<BoardRow>();

    private void Start()
    {
        // Tüm BoardRow'ları bul ve listeye ekle
        BoardRow[] rows = FindObjectsOfType<BoardRow>();
        foreach (var row in rows)
        {
            AddBoardRow(row);
        }

        // Başlangıç toplam skorunu hesapla ve bildir
        UpdateTotalScore();
    }

    // Yeni bir BoardRow eklemek için
    public void AddBoardRow(BoardRow row)
    {
        if (!boardRows.Contains(row))
        {
            boardRows.Add(row);

            // Her bir BoardRow'dan bildirim almak için observer ekle
            RowObserver rowObserver = new RowObserver(this, row);
            row.AddObserver(rowObserver);
        }
    }

    // Observer'ları ekler
    public void AddObserver(IObserver observer)
    {
        if (!observers.Contains(observer))
        {
            observers.Add(observer);

            // Yeni observer'a mevcut toplam skoru hemen bildir
            observer.OnNotify(totalScore.ToString(), null);
        }
    }

    // Observer'ları çıkarır
    public void RemoveObserver(IObserver observer)
    {
        if (observers.Contains(observer))
        {
            observers.Remove(observer);
        }
    }

    // Observer'lara bildirim gönderir
    public void NotifyObservers(string message)
    {

        foreach (var observer in observers)
        {
            if (observer != null)
            {
                observer.OnNotify(message, null);
            }

        }
    }

    // Toplam skoru günceller ve observer'lara bildirim gönderir
    public void UpdateTotalScore()
    {
        int oldScore = totalScore;
        CalculateTotalScore();

        if (oldScore != totalScore)
        {
            NotifyObservers(totalScore.ToString());
        }
    }

    // Tüm satırların toplam değerini hesaplar
    private void CalculateTotalScore()
    {
        totalScore = 0;
        foreach (var row in boardRows)
        {
            if (row != null)
            {
                totalScore += row.GetTotalPower();
            }
        }
    }

    // BoardRow değişikliklerini dinlemek için özel sınıf
    private class RowObserver : IObserver
    {
        private ScoreManager scoreManager;
        private BoardRow boardRow;

        public RowObserver(ScoreManager scoreManager, BoardRow boardRow)
        {
            this.scoreManager = scoreManager;
            this.boardRow = boardRow;
        }

        public void OnNotify(string message, BoardRow source)
        {
            // BoardRow değiştiğinde toplam skoru güncelle
            scoreManager.UpdateTotalScore();
        }
    }
}