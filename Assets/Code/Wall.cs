using UnityEngine;
using System.Collections;


public class Wall : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    void onTriggerEnter(Collider collision)
    {
        Debug.Log("hit");
    }
}
