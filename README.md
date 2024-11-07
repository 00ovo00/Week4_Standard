# Week4_Standard
<details>
    <summary><b> Q1. 숙련 12강 ~ 숙련 14강</b></summary>
    <div markdown="1">
    <ul>

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
    </ul>
  </div>
</details>

<details>
    <summary><b>Q2. 숙련 15강 ~ 숙련 17강</b></summary>
    <div markdown="1">
    <ul>

**[요구사항 1]**

**분석 문제** : 분석한 내용을 직접 작성하고, 강의의 코드를 다시 한번 작성하며 복습해봅시다.

- AI 네비게이션 시스템에서 가장 핵심이 되는 개념에 대해 복습해보세요.
    
     **NavMesh(Navigation Mesh)**
    - AI가 이동할 수 있는 경로와 영역을 나타내는 맵. 바닥, 계단, 경사면 같은 이동할 수 있는 표면을 탐지하여 자동 생성
    - 장애물을 감지하여 벽이나 특정 오브젝트를 뚫고 지나가지 못하도록 경로 설정
    - Scene에 NevMesh 생성하면 AI 캐릭터가 NavMesh 기반으로 최적 이동 경로로 이동
    
    **NavMeshAgent(Navigation Mesh)**
    - AI 캐릭터에 붙는 컴포넌트로 목표 지점까지 NavMesh 따라 이동하는 방식 제어, 강의에서는 곰(NPC)에 붙어있음
    - 이동, 회전 속도, 장애물 회피 등 이동 관련 설정 제어
    > 
- NPC 기능의 구조와 핵심 로직을 분석해보세요.
    
    <aside>
    
    상태 패턴을 사용하여 플레이어의 상태에 따라 애니메이션을 적용,
    
    Update에서 플레이어와의 거리를 지속적으로 확인하고 이에 따라 상태 전환
    
    </aside>
    
    ```csharp
    void PassiveUpdate() // Idle 일 때 호출
    {
      // 돌아다니는 상태이고 목적지까지의 거리가 0.1f보다 작으면
      if(aiState == AIState.Wandering && agent.remainingDistance < 0.1f)
      {
          SetState(AIState.Idle); // idle 상태로 전환
          // 새로운 목적지 탐색하는 메소드 호출
          Invoke("WanderToNewLocation", Random.Range(minWanderWaitTime, maxWanderWaitTime));
      }
      
      if(playerDistance < detectDistance) // 플레이어가 탐지 범위 내에 있으면
      {
          SetState(AIState.Attacking);    // 공격 상태로 전환
      }
    }
    
    void AttackingUpdate() // Attacking 일 때 호출
    {
      // 플레이어와의 거리가 공격범위보다 멀거나 시야각 외부에 있으면
      if(playerDistance > attackDistance || !IsPlayerInFieldOfView())
      {
          agent.isStopped = false;
          NavMeshPath path = new NavMeshPath();
          // 현재 지점에서 새로운 지점으로 이동할 수 있으면
          if(agent.CalculatePath(CharacterManager.Instance.Player.transform.position, path))
          {
              // 새로운 목적지 설정
              agent.SetDestination(CharacterManager.Instance.Player.transform.position);
          }
          else
          {
              SetState(AIState.Fleeing);
          }
      }
      // 플레이어와가 공격범위와 시야각 내부에 있으면
      else
      {
          agent.isStopped = true;
          // 공격 가능한 시간인지 확인
          if(Time.time - lastAttackTime > attackRate)
          {
              lastAttackTime = Time.time;
              // 공격 로직 실행
              CharacterManager.Instance.Player.controller.GetComponent<IDamagable>().TakePhysicalDamage(damage);
              // 공격 애니메이션 설정
              animator.speed = 1;
              animator.SetTrigger("Attack");
          }
      }
    }
    
    Vector3 GetWanderLocation() // 다음 목적지 탐색하는 메소드
    {
      NavMeshHit hit;
    
      // 이동 가능한 영역
      NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);
    
      int i = 0;
      // 이동 가능한 위치와 현재 위치 거리가 탐지거리 보다 작으면(너무 가까우면)
      while (Vector3.Distance(transform.position, hit.position) < detectDistance)
      {
          // 이동 지점 재설정
          NavMesh.SamplePosition(transform.position + (Random.onUnitSphere * Random.Range(minWanderDistance, maxWanderDistance)), out hit, maxWanderDistance, NavMesh.AllAreas);
          i++;
          if (i == 30) break;  // 30번까지 시도
      }
      // 다음 이동 지점 반환
      return hit.position;
    }
    ```
    </ul>
  </div>
</details>
