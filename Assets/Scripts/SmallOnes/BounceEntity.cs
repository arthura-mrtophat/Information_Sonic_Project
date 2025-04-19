using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BounceEntity : MonoBehaviour
{
    private EntityScript entityScript;
    private SpriteRenderer spriteRenderer;
    private Collider2D collisioner;
    private System.Random randomizer = new();

    private float timeAppeared = 0;

    [SerializeField] public const float friction_speed = 0.001176875f;
    private void Start() {
        entityScript = GetComponent<EntityScript>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        collisioner = GetComponent<Collider2D>();
        entityScript.GroundSpeed = randomizer.Next(-500, 500)/1000f;
        entityScript.EntityAddVelocity = new(0, randomizer.Next(5, 100) / 1000f);
        timeAppeared = Time.time;
    }
    private void FixedUpdate() {
        if (entityScript.EntityClass.Health < 0 || !collisioner.enabled) return;
        if (Time.time - timeAppeared > 15f){
            Destroy(gameObject);
            return;
        }
        spriteRenderer.enabled = Mathf.Round(Time.time*10) % 2 == 0;
        entityScript.GroundSpeed -= friction_speed * MathF.Sign(entityScript.GroundSpeed);
        bool ground = entityScript.CheckIfGrounded();
        bool ceiling = entityScript.CheckIfCeiling();
        bool wall = entityScript.CheckIfWall();
        if (wall) entityScript.GroundSpeed *= -1;
        if (ground || ceiling) entityScript.EntityAddVelocity *= -.9f;
    }
}
