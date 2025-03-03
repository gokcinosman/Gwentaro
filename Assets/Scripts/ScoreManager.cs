using System;
using System.Collections.Generic;
using UnityEngine;

// ScoreManager (Subject)
public class ScoreManager : MonoBehaviour, ISubject
{
    private List<IObserver> observers = new List<IObserver>();
    private int totalScore = 0;

    // Observer'ları ekler
    public void AddObserver(IObserver observer)
    {
        if (!observers.Contains(observer))
            observers.Add(observer);
    }

    // Observer'ları çıkarır
    public void RemoveObserver(IObserver observer)
    {
        if (observers.Contains(observer))
            observers.Remove(observer);
    }

    // Observer'lara bildirim gönderir
    public void NotifyObservers(string message)
    {
        foreach (var observer in observers)
        {
            observer.OnNotify(totalScore.ToString(), null);
        }
    }

    // Toplam skoru günceller ve observer'lara bildirim gönderir
    public void UpdateTotalScore(int scoreChange)
    {
        totalScore += scoreChange;
        NotifyObservers(totalScore.ToString());
    }
}
