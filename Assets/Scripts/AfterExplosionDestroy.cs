using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterExplosionDestroy : MonoBehaviour
{
    public float TimeToDie = 2f;
    private float TimeStarted;
    public void Start() {
        TimeStarted = Time.fixedTime;
    }
    public void FixedUpdate() {
        if (Time.fixedTime - TimeStarted < TimeToDie) return;
        Destroy(transform.parent.gameObject);
    }
}
