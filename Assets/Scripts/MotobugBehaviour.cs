using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotobugBehaviour : MonoBehaviour
{
    private EntityScript entityScript;
    private Animator motobugAnimator;
    [SerializeField] private float PositionDelta = .4f;

    private float BaseXPosition = 0;

    public float PositionXNegative = 5f;
    public float PositionXPositive = 5f;

    private float _timeStartedWaiting = 0;
    private float _timeWait = 2;

    public int Turn = 1;
    void Start() {
        BaseXPosition = transform.position.x;
        entityScript = GetComponent<EntityScript>();
        motobugAnimator = GetComponent<Animator>();
    }
    void Update() {
        motobugAnimator.speed = entityScript.GroundSpeed != 0 ? 1 : 0;
        float currentXPosition = BaseXPosition + (Turn == 1 ? PositionXPositive : -PositionXNegative);
        if (Time.time - _timeStartedWaiting < _timeWait) {
            entityScript.GroundSpeed = 0;
            return;
        }
        if (Mathf.Abs(Mathf.Abs(transform.position.x) - Mathf.Abs(currentXPosition)) < PositionDelta) {
            _timeWait = Random.Range(1, 3);
            _timeStartedWaiting = Time.time;
            Turn = -Turn;
            return;
        }
        entityScript.GroundSpeed = .1f * Turn;
        entityScript.Turn = Turn;
    }
}
