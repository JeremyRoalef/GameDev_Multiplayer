using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * No need to NetworkBehavior as this will only happen on client's screen only
 */
public class Lifetime : MonoBehaviour
{
    [SerializeField]
    [Min(0)]
    float lifetimeDuration = 3f;

    private void Start()
    {
        Destroy(gameObject, lifetimeDuration);
    }
}
