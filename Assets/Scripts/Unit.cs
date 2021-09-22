using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : MonoBehaviour
{
    // Player stats
    public string unitName = "";
    public string unitType = "";
    public float level = 1.0f;
    public float hpMax = 10.0f;
    public float hpCurr = 10.0f;
    public float attack = 5.0f;
    public float defense = 3.0f;
    public float speed = 10.0f;
    public bool isDead = false;
    public HealthBar healthBar;

    // Animation Engine
    protected Animator _animator;

    protected void ResetHealth()
    {
        hpCurr = hpMax;
        healthBar.SetMaxHealth(hpMax);
    }

    public bool TakeDamage(float damageDealt)
    {
        hpCurr = (hpCurr >= damageDealt) ? hpCurr - damageDealt : 0;
        healthBar.SetHealth(hpCurr);
        _animator.SetTrigger("Hurt");

        if (hpCurr == 0)
        {
            Die();
            return true;
        }
        return false;
    }

    public virtual void Die()
    {
        _animator.SetTrigger("Death");
        isDead = true;
    }

    public virtual void Recover()
    {
        ResetHealth();

        if (isDead)
            _animator.SetTrigger("Recover");
        isDead = false;
    }

    public void Attack()
    {
        _animator.SetTrigger("Attack");
    }

    public void SetBattleStance()
    {
        _animator.SetInteger("AnimState", 0);
    }

    public void SetIdle()
    {
        _animator.SetInteger("AnimState", 1);
    }
}
