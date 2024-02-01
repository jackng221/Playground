using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class Missile : MonoBehaviour
{
    public Action<Missile> OnTrigger;

    Rigidbody rb;
    [SerializeField] GameObject target;
    Vector3 ogPos;
    Quaternion ogRotation;

    public float flySpeed = 1f;
    public float rotateDegree = 360f;

    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
        ogPos = transform.position;
        ogRotation = transform.rotation;
    }
    private void Update()
    {
        rb.velocity = transform.forward * flySpeed;

        if (target == null) return;

        Vector3 direction;
        if (target.GetComponent<CharacterController>())
        {
            direction = (target.transform.position + target.GetComponent<CharacterController>().center) - transform.position;
        }
        else
        {
            direction = target.transform.position - transform.position;
        }

        Quaternion rotation = Quaternion.LookRotation(direction);
        rb.MoveRotation(Quaternion.RotateTowards(transform.rotation, rotation, rotateDegree * Time.deltaTime));
    }
    public void ReInit(Vector3? position = null, GameObject? target = null)
    {
        if (position == null)
            transform.position = ogPos;
        else
            transform.position = (Vector3)position;

        if (target == null)
            this.target = null;
        else
            this.target = target;

        transform.rotation = ogRotation;
        rb.velocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        rb.AddForce(transform.up * 10f);
    }

    private void OnTriggerEnter(Collider other)
    {
        OnTrigger.Invoke(this);
    }

    [ContextMenu("Trigger")]
    void TestTrigger()
    {
        OnTrigger.Invoke(this);
    }
}
