using UnityEngine;

public class DecreaseTimeScript : MonoBehaviour
{
    private float timeReductionPercent = 0.5f; // Reduces delay by 50%

    private void Start()
    {
        Debug.Log("[AST DEBUG] DecreaseTimeScript started");

        // Get the bullet component
        Bullet bullet = GetComponent<Bullet>();
        if (bullet != null)
        {
            // Get the firepoint (parent of the bullet)
            Transform firePoint = transform.parent;
            if (firePoint != null)
            {
                // Get the weapon component from firepoint's parent
                Weapon weapon = firePoint.parent.GetComponent<Weapon>();
                if (weapon != null)
                {
                    Debug.Log($"[AST DEBUG] DecreaseTimeScript found weapon, current Index: {weapon.Index}");
                    // Get the current action card
                    var (actionCard, actionIndex, manaCost) = weapon.GetTargetActionCard(weapon.Index);
                    Debug.Log($"[AST DEBUG] DecreaseTimeScript got action card: {(actionCard != null ? actionCard.name : "null")}, index: {actionIndex}, manaCost: {manaCost}");
                    
                    if (actionCard != null)
                    {
                        int originalDelay = actionCard.getDelay();
                        Debug.Log($"[AST DEBUG] Original delay: {originalDelay}");

                        // Calculate and set new delay
                        int newDelay = Mathf.RoundToInt(originalDelay * (1f - timeReductionPercent));
                        actionCard.setDelay(newDelay);

                        Debug.Log($"[AST DEBUG] New delay after {timeReductionPercent * 100}% reduction: {newDelay}");
                    }
                    else
                    {
                        Debug.LogError("[AST DEBUG] No ActionCard found in weapon");
                    }
                }
                else
                {
                    Debug.LogError("[AST DEBUG] No Weapon component found on firepoint's parent");
                }
            }
            else
            {
                Debug.LogError("[AST DEBUG] No parent (firepoint) found for bullet");
            }
        }
        else
        {
            Debug.LogError("[AST DEBUG] No Bullet component found on object");
        }
    }
}