using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CardVisual : MonoBehaviour
{
    private Transform _transform;
    private RectTransform _rectTransform;
    private Vector3 _targetPosition;
    private bool isMoving;
    private Vector3 velocity = Vector3.zero;
    [SerializeField] private float moveSpeed = 0.1f;
    [SerializeField] private float xOffset = 200f;
    private void Awake()
    {
        _transform = transform;
        _rectTransform = GetComponent<RectTransform>();
    }
    public void Swap(int direction) // -1: sola, 1: sağa
    {
        // Yeni hedef pozisyonu hesapla
        _targetPosition = new Vector3(
            direction * xOffset,
            _rectTransform.anchoredPosition.y,
            _transform.localPosition.z
        );
        isMoving = true;
    }
    private void Update()
    {
        if (!isMoving) return;
        // Yumuşak geçiş animasyonu
        _rectTransform.anchoredPosition = Vector3.SmoothDamp(
            _rectTransform.anchoredPosition,
            _targetPosition,
            ref velocity,
            moveSpeed
        );
        if (Vector3.Distance(_rectTransform.anchoredPosition, _targetPosition) < 0.1f)
        {
            _rectTransform.anchoredPosition = _targetPosition;
            isMoving = false;
        }
    }
    public void UpdateIndex(int totalCards)
    {
        // Parent'ın genişliğine göre pozisyonu yeniden hesapla
        float parentWidth = _rectTransform.parent.GetComponent<RectTransform>().rect.width;
        float xPosition = (-parentWidth / 2) + (xOffset * ParentIndex());
        _targetPosition = new Vector3(xPosition, _transform.localPosition.y, _transform.localPosition.z);
        _rectTransform.anchoredPosition = _targetPosition;
    }
    public int ParentIndex() => _transform.GetSiblingIndex();
}
