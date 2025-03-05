using System;
using System.Collections.Generic;
using UnityEngine;

public class ScoreManager : MonoBehaviour, ISubject
{
    private List<IObserver> observers = new List<IObserver>();
    private int totalScore = 0;
    private List<BoardRow> boardRows = new List<BoardRow>();
    private int player1TotalScore;
    private int player2TotalScore;

    private void Start()
    {
        // Tüm BoardRow'ları bul ve listeye ekle
        BoardRow[] rows = FindObjectsOfType<BoardRow>();
        foreach (var row in rows)
        {
            AddBoardRow(row);
        }

        // Başlangıç toplam skorunu hesapla ve bildir
        UpdateTotalScores();
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
            observer.OnNotify(GetTotalScoreMessage(), null);
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
    public void NotifyObservers()
    {
        foreach (var observer in observers)
        {
            if (observer != null)
            {
                observer.OnNotify(GetTotalScoreMessage(), null);
            }
        }
    }

    // Toplam skoru günceller ve observer'lara bildirim gönderir
    public void UpdateTotalScores()
    {
        int oldP1Score = player1TotalScore;
        int oldP2Score = player2TotalScore;

        CalculateTotalScores();

        if (oldP1Score != player1TotalScore || oldP2Score != player2TotalScore)
        {
            NotifyObservers();
        }
    }

    private void CalculateTotalScores()
    {
        player1TotalScore = 0;
        player2TotalScore = 0;

        foreach (var row in boardRows)
        {
            if (row != null)
            {
                if (row.ownerPlayerId == 0)
                    player1TotalScore += row.GetTotalPower();
                else if (row.ownerPlayerId == 1)
                    player2TotalScore += row.GetTotalPower();
            }
        }
    }

    public string GetTotalScoreMessage()
    {
        return $"{player1TotalScore}:{player2TotalScore}";
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
            scoreManager.UpdateTotalScores();
        }
    }
}