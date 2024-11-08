using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

using UnityEngine;
using UnityEngine.InputSystem;

public class Equipment : MonoBehaviour
{
    public Equip curEquip;  // 현재 장착중인 아이템
    public Transform equipParent;   // 카메라 위치

    private PlayerController controller;
    private PlayerCondition condition;

    void Start()
    {
        controller = CharacterManager.Instance.Player.controller;
        condition = CharacterManager.Instance.Player.condition;
    }
    
    public void OnAttackInput(InputAction.CallbackContext context)
    {
        // 마우스 좌클릭 입력 들어오고 현재 장착된 아이템이 있고 카메라 고정되어 있지 않으면
        if(context.phase == InputActionPhase.Performed && curEquip != null && controller.canLook) 
        {
            curEquip.OnAttackInput();   // 공격 실행
        }
    }

    public void EquipNew(ItemData data)
    {
        UnEquip();  // 기존 아이템 해제
        // 새로운 아이템 장착        
        curEquip = Instantiate(data.equipPrefab, equipParent).GetComponent<Equip>();
        //curEquip.transform.position = this.transform.position;
    }

    public void UnEquip()
    {
        // 현재 장착 중인 아이템 있으면 파괴
        if(curEquip != null)
        {
            Destroy(curEquip.gameObject);
            curEquip = null;
        }
    }
}