using UnityEngine;

public class Gate : MonoBehaviour
{
    Animator animator;
    [SerializeField] GameObject animatedBridge;

    private void Awake()
    {
        animator = animatedBridge.GetComponent<Animator>();
    }

    public void OpenGate()
    {
        animator.SetTrigger("CanLowerBridge");
    }
}