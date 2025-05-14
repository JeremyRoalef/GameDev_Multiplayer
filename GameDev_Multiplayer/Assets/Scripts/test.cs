using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class test : MonoBehaviour
{
    [SerializeField]
    InputReader inputreader;

    // Start is called before the first frame update
    void Start()
    {
        inputreader.MoveEvent += HandleMove;
    }

    private void HandleMove(Vector2 vector)
    {
        Debug.Log(vector);
    }

    private void OnDestroy()
    {
        inputreader.MoveEvent -= HandleMove;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
