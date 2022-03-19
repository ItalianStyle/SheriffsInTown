using UnityEngine;

public class Key : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        if(other.TryGetComponent(out KeyHolder playerKeyHolder))
        {
            playerKeyHolder.TakeKey();
            Destroy(gameObject);
        }
    }
}
