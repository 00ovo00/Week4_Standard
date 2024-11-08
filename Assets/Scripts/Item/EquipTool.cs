using UnityEngine;

public class EquipTool : Equip
{
    public float attackRate;
    private bool attacking;
    public float attackDistance;
    public float useStamina;

    [Header("Resource Gathering")] public bool doesGatherResources; // 자원 수집 가능 여부

    [Header("Combat")] public bool doesDealDamage; // 데미지 적용 여부 
    public int damage;

    private Animator animator;
    private Camera camera;

    private void Awake()
    {
        camera = Camera.main;
        animator = GetComponent<Animator>();
    }

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

    void OnCanAttack()
    {
        attacking = false;
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
}