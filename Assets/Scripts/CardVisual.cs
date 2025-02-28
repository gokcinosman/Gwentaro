using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class CardVisual : MonoBehaviour
{
    private Transform _transform;
        private Card parentCard;
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
    private void Start()
    {
        parentCard = GetComponentInParent<Card>();
        parentCard.BeginDragEvent.AddListener(BeginDrag);
        parentCard.EndDragEvent.AddListener(EndDrag);
    }
    private void BeginDrag(Card card)
    {
        isMoving = true;
    }
    private void EndDrag(Card card)
    {
        isMoving = false;
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
    }
    public void UpdateIndex(int length)
    {
        transform.SetSiblingIndex(parentCard.transform.parent.GetSiblingIndex());
    }
    public int ParentIndex() => _transform.GetSiblingIndex();
}
