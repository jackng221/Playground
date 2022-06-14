using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Control : MonoBehaviour
{
    void Update()
    {
        if (GetComponent<Animator>().GetCurrentAnimatorStateInfo(0).IsTag("idle"))
        {
            if (Input.GetKeyDown(KeyCode.Mouse0))
            {
                GetComponent<Animator>().SetTrigger("Dance");
            }
            if (Input.GetKeyDown(KeyCode.Mouse1))
            {
                GetComponent<Animator>().SetTrigger("Dance2");
            }
        }
    }

}
