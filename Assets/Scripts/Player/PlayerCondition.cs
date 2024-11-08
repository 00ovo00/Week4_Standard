using System;
using UnityEngine;

// 데미지 받는 객체가 사용할 인터페이스
public interface IDamagable
{
    void TakePhysicalDamage(int damageAmount);
}

public class PlayerCondition : MonoBehaviour, IDamagable
{
    public UICondition uiCondition;

    Condition health { get { return uiCondition.health; } }
    Condition hunger { get { return uiCondition.hunger; } }
    Condition stamina { get { return uiCondition.stamina; } }

    public float noHungerHealthDecay;
    public event Action onTakeDamage;

    private void Update()
    {
        hunger.Subtract(hunger.passiveValue * Time.deltaTime);
        //hunger.Add(hunger.passiveValue * Time.deltaTime);
        stamina.Add(stamina.passiveValue * Time.deltaTime);
        
        // hunger가 0이면 체력 감소시키기
        if(hunger.curValue <= 0.0f)
        {
            health.Subtract(noHungerHealthDecay * Time.deltaTime);
        }

        if(health.curValue <= 0.0f)
        {
            Die();
        }
    }

    public void Heal(float amount)
    {
        health.Add(amount);
    }

    public void Eat(float amount)
    {
        hunger.Add(amount);
    }

    public void Die()
    {
        Debug.Log("플레이어가 죽었다.");
    }
    // IDamagable 인터페이스 구현부
    public void TakePhysicalDamage(int damageAmount)
    {
        health.Subtract(damageAmount); 
        onTakeDamage?.Invoke();
        // 데미지 받은 UI 효과 발생
        // DamageIndicator의 Flash
    }
    
    public bool UseStamina(float amount)
    {
        if(stamina.curValue - amount < 0)
        {
            return false;
        }
        stamina.Subtract(amount);
        return true;
    }
}