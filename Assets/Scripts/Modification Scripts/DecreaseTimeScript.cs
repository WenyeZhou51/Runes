using UnityEngine;

public class DecreaseTimeScript : MonoBehaviour
{
    private float timeReductionPercent = 0.5f; // Reduces delay by 50%

    private void Start()
    {
        Debug.Log("DecreaseTimeScript started");

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
                    // Get the current action card
                    var (actionCard, _, _) = weapon.GetTargetActionCard(weapon.index);
                    if (actionCard != null)
                    {
                        int originalDelay = actionCard.getDelay();
                        Debug.Log($"Original delay: {originalDelay}");

                        // Calculate and set new delay
                        int newDelay = Mathf.RoundToInt(originalDelay * (1f - timeReductionPercent));
                        actionCard.setDelay(newDelay);

                        Debug.Log($"New delay after {timeReductionPercent * 100}% reduction: {newDelay}");
                    }
                    else
                    {
                        Debug.LogError("No ActionCard found in weapon");
                    }
                }
                else
                {
                    Debug.LogError("No Weapon component found on firepoint's parent");
                }
            }
            else
            {
                Debug.LogError("No parent (firepoint) found for bullet");
            }
        }
        else
        {
            Debug.LogError("No Bullet component found on object");
        }
    }
}