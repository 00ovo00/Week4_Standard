using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class UIInventory : MonoBehaviour
{
    public ItemSlot[] slots;

    public GameObject inventoryWindow;
    public Transform slotPanel;
    public Transform dropPosition;

    [Header("Selected Item")]
    private ItemSlot selectedItem;
    private int selectedItemIndex;
    public TextMeshProUGUI selectedItemName;
    public TextMeshProUGUI selectedItemDescription;
    public TextMeshProUGUI selectedItemStatName;
    public TextMeshProUGUI selectedItemStatValue;
    public GameObject useButton;
    public GameObject equipButton;
    public GameObject unEquipButton;
    public GameObject dropButton;

    private int curEquipIndex;

    private PlayerController controller;
    private PlayerCondition condition;

    void Start()
    {
        controller = CharacterManager.Instance.Player.controller;
        condition = CharacterManager.Instance.Player.condition;
        dropPosition = CharacterManager.Instance.Player.dropPosition;

        controller.inventory += Toggle;
        CharacterManager.Instance.Player.addItem += AddItem;

        inventoryWindow.SetActive(false);
        slots = new ItemSlot[slotPanel.childCount];

        for(int i = 0; i < slots.Length; i++)
        {
            slots[i] = slotPanel.GetChild(i).GetComponent<ItemSlot>();
            slots[i].index = i;
            slots[i].inventory = this;
            slots[i].Clear();
        }

        ClearSelectedItemWindow();  // 초기 세팅
    }

    public void Toggle()
    {
        if (inventoryWindow.activeInHierarchy)
        {
            inventoryWindow.SetActive(false);
        }
        else
        {
            inventoryWindow.SetActive(true);
        }
    }

    public bool IsOpen()
    {
        return inventoryWindow.activeInHierarchy;
    }

    public void AddItem()
    {
        // 현재 상호작용 중인 아이템 정보 가져오기
        ItemData data = CharacterManager.Instance.Player.itemData;
        // 중복 가능한 아이템이면
        if (data.canStack)
        {
            // 아이템 정보 가져오기
            ItemSlot slot = GetItemStack(data);
            // 최대 개수보다 적으면
            if(slot != null)
            {
                // 수량만 더해주고 UI 갱신
                slot.quantity++;
                UpdateUI();
                // 현재 상호작용 중인 아이템 없는 상태로 만듦
                CharacterManager.Instance.Player.itemData = null;
                return;
            }
        }
        // 아이템 슬롯이 비어있으면 빈 슬롯 세팅
        ItemSlot emptySlot = GetEmptySlot();
        // 슬롯에 아이템이 있으면
        if(emptySlot != null)
        {
            // 아이템 데이터와 수량을 UI에 갱신
            emptySlot.item = data;
            emptySlot.quantity = 1;
            UpdateUI();
            CharacterManager.Instance.Player.itemData = null;
            return;
        }
        // 이미 해당 아이템 최고 개수인 경우 아이템 버림
        ThrowItem(data);
        CharacterManager.Instance.Player.itemData = null;
    }

    public void ThrowItem(ItemData data)
    {
        Instantiate(data.dropPrefab, dropPosition.position, Quaternion.Euler(Vector3.one * Random.value * 360));
    }

    public void UpdateUI()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item != null)
            {
                slots[i].Set();
            }
            else
            {
                slots[i].Clear();
            }
        }
    }

    ItemSlot GetItemStack(ItemData data)
    {
        for(int i = 0; i < slots.Length; i++)
        {
            // 아이템 데이터와 슬롯의 아이템이 같고 슬롯 수량이 최대값보다 작으면
            if (slots[i].item == data && slots[i].quantity < data.maxStackAmount)
            {
                return slots[i];
            }
        }
        return null;
    }

    ItemSlot GetEmptySlot()
    {
        for(int i = 0; i < slots.Length; i++)
        {
            if (slots[i].item == null)
            {
                return slots[i];
            }
        }
        return null;
    }

    public void SelectItem(int index)
    {
        if (slots[index].item == null) return;

        selectedItem = slots[index];
        selectedItemIndex = index;

        selectedItemName.text = selectedItem.item.displayName;
        selectedItemDescription.text = selectedItem.item.description;
        
        // 스탯 문자열은 공백으로 초기화
        selectedItemStatName.text = string.Empty;
        selectedItemStatValue.text = string.Empty;

        // item이 consumable인 경우에 수치를 출력
        for(int i = 0; i< selectedItem.item.consumables.Length; i++)
        {
            selectedItemStatName.text += selectedItem.item.consumables[i].type.ToString() + "\n";
            selectedItemStatValue.text += selectedItem.item.consumables[i].value.ToString() + "\n";
        }

        useButton.SetActive(selectedItem.item.type == ItemType.Consumable);
        equipButton.SetActive(selectedItem.item.type == ItemType.Equipable && !slots[index].equipped);
        unEquipButton.SetActive(selectedItem.item.type == ItemType.Equipable && slots[index].equipped);
        dropButton.SetActive(true);
    }

    void ClearSelectedItemWindow()
    {
        selectedItem = null;

        selectedItemName.text = string.Empty;
        selectedItemDescription.text = string.Empty;
        selectedItemStatName.text = string.Empty;
        selectedItemStatValue.text = string.Empty;

        useButton.SetActive(false);
        equipButton.SetActive(false);
        unEquipButton.SetActive(false);
        dropButton.SetActive(false);
    }

    public void OnUseButton()
    {
        if(selectedItem.item.type == ItemType.Consumable)
        {
            for(int i = 0; i < selectedItem.item.consumables.Length; i++)
            {
                switch (selectedItem.item.consumables[i].type)
                {
                    case ConsumableType.Health:
                        condition.Heal(selectedItem.item.consumables[i].value); break;
                    case ConsumableType.Hunger:
                        condition.Eat(selectedItem.item.consumables[i].value);break;
                }
            }
            RemoveSelctedItem();
        }
    }

    public void OnDropButton()
    {
        ThrowItem(selectedItem.item);
        RemoveSelctedItem();
    }

    // 버린 아이템 UI 정보 업데이트
    void RemoveSelctedItem()
    {
        selectedItem.quantity--;

        if(selectedItem.quantity <= 0)
        {
            if (slots[selectedItemIndex].equipped)
            {
                UnEquip(selectedItemIndex);
            }

            selectedItem.item = null;
            ClearSelectedItemWindow();
        }

        UpdateUI();
    }
    
    public void OnEquipButton()
    {
        // 현재 장착 중인 아이템 해제
        if (slots[curEquipIndex].equipped)
        {
            UnEquip(curEquipIndex);
        }
        // 선택된 아이템 장착 상태로 만들기
        slots[selectedItemIndex].equipped = true;
        curEquipIndex = selectedItemIndex; 
        CharacterManager.Instance.Player.equip.EquipNew(selectedItem.item);
        // 변경된 상태 UI에 반영        
        UpdateUI();
        SelectItem(selectedItemIndex);
    }

    void UnEquip(int index)
    {
        slots[index].equipped = false;
        CharacterManager.Instance.Player.equip.UnEquip();
        UpdateUI();

        if(selectedItemIndex == index)
        {
            SelectItem(selectedItemIndex);
        }
    }

    public void OnUpEquipButton()
    {
        // 장착 중인 아이템 해제
        UnEquip(selectedItemIndex);
    }

    public bool HasItem(ItemData item, int quantity)
    {
        return false;
    }
}