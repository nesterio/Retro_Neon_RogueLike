using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletScript : MonoBehaviour
{
    Rigidbody rb;

    public Vector3 hitPoint;
    public Vector3 hitNormal;

    public GameObject impactObj;

    public int speed;

    [HideInInspector]
    public int damage;

    void Awake() 
    {
        rb = GetComponent<Rigidbody>();
    }

    public void Initialize(Vector3 _hitPoint, Vector3 _hitNormal, int _damage) 
    {
        hitPoint = _hitPoint;
        hitNormal = _hitNormal;
        damage = _damage;

        rb.AddForce((hitPoint - this.transform.position).normalized * speed);
    }

    void OnCollisionEnter(Collision col) 
    {
        GameObject bulletImpactObj = Instantiate(impactObj, transform.position, Quaternion.LookRotation(hitNormal, Vector3.up) * impactObj.transform.rotation);

        Destroy(bulletImpactObj, 10);
        bulletImpactObj.transform.SetParent(col.transform);

        if (col.gameObject.CompareTag("Player"))
        {
            col.gameObject.GetComponent<PlayerStats>().DrainHealth(damage);
        }

        Destroy(gameObject);
    }

}
