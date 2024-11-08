using System;
using UnityEngine;

public class Player : MonoBehaviour
{
    public PlayerController controller;
    public PlayerCondition condition;

    public ItemData itemData;   // 현재 상호작용하는 아이템 정보
    public Action addItem;
    public Equipment equip; // 현재 장착 중인 아이템

    public Transform dropPosition;
    private void Awake()
    {
        CharacterManager.Instance.Player = this;
        controller = GetComponent<PlayerController>();
        condition = GetComponent<PlayerCondition>();
        equip = GetComponent<Equipment>();
    }
}