# Week4_Standard
### Q1. 숙련 12강 ~ 숙련 14강

**[요구사항 1]**

**분석 문제 :** 분석한 내용을 직접 작성하고, 강의의 코드를 다시 한번 작성하며 복습해봅시다.

- Equipment와 EquipTool 기능의 구조와 핵심 로직을 분석해보세요.
- Equipment
    
    <aside>
    
    Equipable 아이템을 플레이어가 장착할 수 있도록 관리. 새로운 아이템을 장착하거나 기존에 장착했던 아이템 해제
    
    클릭 이벤트를 입력 받아 공격 이벤트 호출
    
    </aside>
    
    ```csharp
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
        curEquip.transform.position = this.transform.position;
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
    ```
    
- EquipTool
    
    <aside>
    
    Equipment 클래스 상속받아 아이템에 무기나 도구로서의 기능을 추가.
    
    </aside>
    
    ```csharp
    public override void OnAttackInput()
    {
        if (!attacking) // 공격 중이 아니면
        {
            // 공격에 필요한 스테미나 있는지 확인
            if (CharacterManager.Instance.Player.condition.UseStamina(useStamina))
            {
                // 공격 중인 상태로 변경
                attacking = true;
                animator.SetTrigger("Attack");
                Invoke("OnCanAttack", attackRate);
            }
        }
    }
    
    public void OnHit()
    {
        Ray ray = camera.ScreenPointToRay(new Vector3(Screen.width / 2, Screen.height / 2, 0));
        RaycastHit hit;
    
        // 공격 범위 내에 raycast된 것 있으면
        if(Physics.Raycast(ray, out hit, attackDistance))
        {
            // raycast 된 대상이 자원이면
            if(doesGatherResources && hit.collider.TryGetComponent(out Resource resource))
            {
                resource.Gather(hit.point, hit.normal); // 자원 수집
            }
            // raycast 된 대상이 피격 가능한 개체면
            if(doesDealDamage && hit.collider.TryGetComponent(out IDamagable damagable))
            {
                damagable.TakePhysicalDamage(damage);   // 데미지 적용
            }
        }
    }
    ```
    
- Resource 기능의 구조와 핵심 로직을 분석해보세요.
    
    <aside>
    
    ItemData와 Resource 타입의 Item(자원)의 속성을 저장, MVC 패턴의 Model에 해당하는 역할
    
    </aside>
    
    ```csharp
    public ItemData itemToGive; // 자원 채집 보상
    public int quantityPerHit = 1;  // 한번에 얻는 보상 수량
    public int capacity;    // 최대 보상 수량
    
    public void Gather(Vector3 hitPoint, Vector3 hitNormal)
    {
        // 한번에 얻는 보상 수량만큼 보상 아이템을 생성
        for(int i = 0; i < quantityPerHit; i++)
        {
            if (capacity <= 0) break;
    
            capacity -= 1;  // 최대 보상 수량 차감
            // 타격 지점보다 위에서 아이템 생성
            Instantiate(itemToGive.dropPrefab, hitPoint + Vector3.up, Quaternion.LookRotation(hitNormal, Vector3.up));
        }
        // 최대 보상 수량이 0보다 작거나 같으면 오브젝트 삭제
        if(capacity <= 0)
        {
            Destroy(gameObject);
        }
    }
    ```
