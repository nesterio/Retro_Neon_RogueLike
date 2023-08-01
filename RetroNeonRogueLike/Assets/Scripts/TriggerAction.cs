using UnityEngine;
using UnityEngine.Events;

public class TriggerAction : MonoBehaviour
{
    public bool reactToPlayer;
    [Space]
    public UnityEvent OnEnterAction;
    [Space]
    public UnityEvent OnStayAction;
    [Space]
    public UnityEvent OnExitAction;

    private void OnTriggerEnter(Collider other)
    {
        if(!reactToPlayer || reactToPlayer && other.gameObject.CompareTag("Player"))
            OnEnter();
    }
    protected virtual void OnEnter() => OnEnterAction?.Invoke();

    private void OnTriggerStay(Collider other)
    {
        if(!reactToPlayer || reactToPlayer && other.gameObject.CompareTag("Player"))
            OnStay();
    }
    protected virtual void OnStay() => OnStayAction?.Invoke();
    
    private void OnTriggerExit(Collider other)
    {
        if(!reactToPlayer || reactToPlayer && other.gameObject.CompareTag("Player"))
            OnExit();
    }
    protected virtual void OnExit() => OnExitAction?.Invoke();
}
