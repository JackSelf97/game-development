using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    public float health = 0;
    private float maxHP = 100;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHP;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
