using UnityEngine;

public class Knife : MonoBehaviour, ToolBehavior
{
    [SerializeField] private LayerMask hitMask; // can probably remove unless only want to hit certain layers

    private float attackCooldown = 0.75f;
    private float damageAmount = 75f;
    private float attackRange = 1.0f;
    private float attackRadius = 0.5f;

    private float lastAttackTime = 0f;

    public void mainAction() 
    {
        PerformAttack();
    }

    public void secondAction() { } // Crosshair handled in ToolManager.cs
    public void thirdAction() { }
    public void fourthAction() { }
    public void StopAiming() { }

    void PerformAttack()
    {
        if (Time.time - lastAttackTime >= attackCooldown)
        {
            Debug.Log("Knife attack");
            lastAttackTime = Time.time;

            Vector3 origin = Camera.main.transform.position;
            Vector3 direction = Camera.main.transform.forward;

            RaycastHit[] hits = Physics.SphereCastAll(origin, attackRadius, direction, attackRange, hitMask);
            foreach (RaycastHit hit in hits)
            {
                IDamageable target = hit.collider.GetComponent<IDamageable>();
                if (target != null)
                {
                    target.TakeDamage(damageAmount);
                    // TODO: Add animation, audio, or blood effects here
                }
            }

            // TODO: Play swing sound or animation
        }
    }
}