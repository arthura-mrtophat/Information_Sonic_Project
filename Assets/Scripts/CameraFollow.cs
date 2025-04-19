using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    private const float CONST_ChangeOffsetSpeed = .05f;
    private const float CONST_MaxOffsetSpeed = 3.5f;
    private const float CONST_MaximumDifferenceX = .85f;
    private const float CONST_MaximumDifferenceY = 1.15f;

    public Transform MaxCorner;
    public Transform MinCorner;

    private Camera usingCamera;

    public Transform FollowTransform;
    private EntityScript entityScript;
    public float CurrentOffset = 0;

    private void Start(){
        entityScript = FollowTransform.GetComponent<EntityScript>();
        usingCamera = GetComponent<Camera>();

        MaxCorner.GetComponent<SpriteRenderer>().enabled = false;
        MinCorner.GetComponent<SpriteRenderer>().enabled = false;
    }
    void FixedUpdate() {
        if (entityScript.EntityClass.Health <= 0) return;
        Vector3 lerpTo = Vector3.zero;
        Vector3 difference = new Vector3(FollowTransform.position.x + CurrentOffset * Mathf.Sign(entityScript.EntityVelocity.x), FollowTransform.position.y) - transform.position;

        bool isOnX = Mathf.Abs(difference.x) >= CONST_MaximumDifferenceX - CurrentOffset;
        bool isOnY = Mathf.Abs(difference.y) >= CONST_MaximumDifferenceY;

        if (isOnX)
            lerpTo = new(Mathf.Clamp(difference.x, -4, 4) - CONST_MaximumDifferenceX * Mathf.Sign(difference.x), lerpTo.y, 0);
        if (isOnY) 
            lerpTo = new(lerpTo.x, Mathf.Clamp(difference.y - CONST_MaximumDifferenceY * Mathf.Sign(difference.y), -4, 4), 0);

        CurrentOffset = Mathf.Clamp(CurrentOffset + CONST_ChangeOffsetSpeed * (Mathf.Abs(entityScript.EntityVelocity.x) > .3 ? 1 : -135f), 0, CONST_MaxOffsetSpeed);

        transform.Translate(lerpTo);

        Vector3 MinCameraCorner = usingCamera.ViewportToWorldPoint(new(0, 0));
        Vector3 MaxCameraCorner = usingCamera.ViewportToWorldPoint(new(1, 1));
        Vector3 MaxDistanceToCamera = new(Mathf.Abs(Mathf.Abs(transform.position.x) - Mathf.Abs(MaxCameraCorner.x)), Mathf.Abs(Mathf.Abs(transform.position.y) - Mathf.Abs(MaxCameraCorner.y)));
        Vector3 MinDistanceToCamera = new(Mathf.Abs(Mathf.Abs(transform.position.x) - Mathf.Abs(MinCameraCorner.x)), Mathf.Abs(Mathf.Abs(transform.position.y) - Mathf.Abs(MinCameraCorner.y)));

        float setX = transform.position.x;
        if (MaxCameraCorner.x > MaxCorner.position.x) setX = MaxCorner.position.x - MaxDistanceToCamera.x;
        if (MinCameraCorner.x < MinCorner.position.x) setX = MinCorner.position.x + MinDistanceToCamera.x;
        float setY = transform.position.y;
        if (MaxCameraCorner.y > MaxCorner.position.y) setY = MaxCorner.position.y - MaxDistanceToCamera.y;
        if (MinCameraCorner.y < MinCorner.position.y) setY = MinCorner.position.y + MinDistanceToCamera.y;
        transform.position = new(setX, setY, -10);
    }
}
