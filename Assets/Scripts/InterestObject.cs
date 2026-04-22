using UnityEngine;

public class InterestObject : MonoBehaviour
{
    public float health = 100f;
    public void RecieveDamage(float amount) 
    {
        health -= amount;
        if (health <= 0) 
        {
            // El objeto se destruye cuando los agentes terminan de "comerlo"
            Destroy(gameObject); 
        }
    }
}
