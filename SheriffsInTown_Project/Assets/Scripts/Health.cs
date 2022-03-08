using UnityEngine;

public class Health : MonoBehaviour
{
    public float Life;
    public UI_Manager manager;

    public void DecreaseHealth(float value)
    {
        Life -= value;
        if (Life <= 0)
        {
            manager.score++;
            Destroy(gameObject);
        }
    }
}
