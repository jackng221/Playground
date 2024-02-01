using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class MissileManager : MonoBehaviour
{
    public GameObject target;

    bool doSpawn = true;
    float timer;
    public float cooldown = 0.2f;

    ObjectPool<Missile> missilesPool;
    [SerializeField] Missile missilePrefab;

    private void Start()
    {
        missilesPool = new ObjectPool<Missile>(CreateFunc, ActionOnGet, ActionOnRelease, ActionOnDestroy, default, 100);
    }

    private void Update()
    {
        if (doSpawn)
        {
            timer += Time.deltaTime;
            if (timer >= cooldown)
            {
                missilesPool.Get();
                timer = 0;
            }
        }
    }

    Missile CreateFunc()
    {
        Missile missile = Instantiate(missilePrefab);
        missile.OnTrigger += (eventMissile) => missilesPool.Release(eventMissile);

        return missile;
    }
    void ActionOnGet(Missile missile)
    {
        missile.gameObject.SetActive(true);
        missile.ReInit(this.gameObject.transform.position, target);
    }
    void ActionOnRelease(Missile missile)
    {
        missile.gameObject.SetActive(false);
    }
    void ActionOnDestroy(Missile missile)
    {
        Destroy(missile.gameObject);
    }

    [ContextMenu("TestSpawn")]
    void TestSpawn()
    {
        missilesPool.Get();
    }
    [ContextMenu("ToggleSpawn")]
    void ToggleSpawn()
    {
        doSpawn = true;
    }
}
