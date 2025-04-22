using UnityEngine;

public class MyGameManager : MonoBehaviour
{
    public static MyGameManager Instance { get; private set; }
    public float Health { get; set; }
    public float MaxHealth { get; private set; } = 100f;
    public float Mana { get; set; }
    public float MaxMana { get; private set; } = 100f;
    public float ManaRegen { get; private set; } = 30f;
    public float MoveSpeed { get; private set; } = 200f;

    public Vector3? PlayerPosition { get; set; } = null;


    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializePlayerStats();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void InitializePlayerStats()
    {
        Health = MaxHealth;
        Mana = MaxMana;
    }
}
