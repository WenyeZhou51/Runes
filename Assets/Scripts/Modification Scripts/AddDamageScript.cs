using UnityEngine;

public class AddDamageScript : MonoBehaviour
{
    private int additionalDamage = 10;

    private void Start()
    {
        Bullet bullet = GetComponent<Bullet>();
        if (bullet != null)
        {
            Debug.Log($"Original damage: {bullet.damage}, Adding damage: {additionalDamage}");
            bullet.damage += additionalDamage;

            ExplodeBolt explodeBolt = bullet as ExplodeBolt;
            if (explodeBolt != null)
            {
                Debug.Log($"Original explosion damage: {explodeBolt.explosionDamage}, Adding damage: {additionalDamage}");
                explodeBolt.explosionDamage += additionalDamage;
                Debug.Log($"New explosion damage: {explodeBolt.explosionDamage}");
            }

            Debug.Log($"New damage: {bullet.damage}");
        }
        else
        {
            Debug.LogError("No Bullet component found on object");
        }
    }
}