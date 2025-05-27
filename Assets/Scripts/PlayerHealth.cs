using UnityEngine;

public class PlayerHealth : MonoBehaviour
{
    private PlayerController playerController;
    
    private void Start()
    {
        // Get the player controller
        playerController = GetComponent<PlayerController>();
        
        // Subscribe to health changes
        MyGameManager.Instance.Health = MyGameManager.Instance.MaxHealth;
    }
    
    private void Update()
    {
        // Check if health is depleted
        if (MyGameManager.Instance.Health <= 0)
        {
            // Call the OnHealthDepleted method on the player controller
            if (playerController != null)
            {
                playerController.OnHealthDepleted();
            }
        }
    }
    
    // Method to take damage
    public void TakeDamage(float amount)
    {
        MyGameManager.Instance.Health -= amount;
        
        // Clamp health to 0
        if (MyGameManager.Instance.Health < 0)
        {
            MyGameManager.Instance.Health = 0;
        }
    }
} 