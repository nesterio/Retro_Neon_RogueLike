using UnityEngine;
using UnityEngine.Events;

public class TriggerAction : MonoBehaviour
{
    public bool reactToPlayer;
    [Space]
    public UnityEvent OnEnter;
    [Space]
    public UnityEvent OnStay;
    [Space]
    public UnityEvent OnExit;

    private void OnTriggerEnter(Collider other)
    {
        if(!reactToPlayer)
            OnEnter?.Invoke();
        
        if(other.gameObject.CompareTag("Player"))
            OnEnter?.Invoke();
    }

    private void OnTriggerStay(Collider other)
    {
        if(!reactToPlayer)
            OnStay?.Invoke();
        
        if(other.gameObject.CompareTag("Player"))
            OnStay?.Invoke();
    }

    private void OnTriggerExit(Collider other)
    {
        if(!reactToPlayer)
            OnExit?.Invoke();
        
        if(other.gameObject.CompareTag("Player"))
            OnExit?.Invoke();
    }
}
