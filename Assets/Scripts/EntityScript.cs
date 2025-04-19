using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum EEntityActions {
    Idle, Falling
}

public interface IEntityClass {
    public int MaxHealth { get; }
    delegate void receiveDamage(int damage);

    public event receiveDamage ReceivedDamage;

}
public class EntityClass: IEntityClass {
    public int MaxHealth { get; }
    public int Health;

    public event IEntityClass.receiveDamage ReceivedDamage;
    public void TakeDamage(int damage) {
        if (Health <= damage) Die();
        Health -= damage;
        this.ReceivedDamage.Invoke(damage);
    }
    public void Die() {
        Debug.Log("Died");
    }
    public EntityClass(int MaxHealth = 1) {
        this.MaxHealth = Health = MaxHealth;
    }
}

public class EntityScript : MonoBehaviour {
    public EntityClass EntityClass;

    private AudioSource audioEntity;
    public BoxCollider2D Collider2D;
    public SpriteRenderer Spriter;

    [Header("Important!")]

    public GameObject ExplosionPrefab;

    [Header("Customizable")]
    [SerializeField] public int MaxHealth = 1;

    public bool FastRotate = false;
    public bool DontRotateSensors = false;
    public bool DontRotateEntity = false;
    public bool IgnoreFalloff = false;

    [Header("Entity Physics")]

    public float GroundSpeed = 0;
    public float GroundAngle = 0;

    public int Turn = 1;

    public EEntityActions CurrentEnemyAction = EEntityActions.Idle;

    public Vector2 EntityVelocity = new(0, 0);
    public Vector2 EntityAddVelocity = new(0, 0);

    public Vector3 currentSensorRotation { get; private set; } = Vector3.up;

    public Dictionary<string, AudioClip> PlayableAudiosEntity = new();

    void Start() {
        EntityClass = new(MaxHealth);
        Collider2D = GetComponent<BoxCollider2D>();
        Spriter = GetComponent<SpriteRenderer>();
        audioEntity = GetComponent<AudioSource>();
        if (!audioEntity) audioEntity = gameObject.AddComponent<AudioSource>();

        PlayableAudiosEntity["Destroyed"] = Resources.Load<AudioClip>("Sounds/Explode");
        if (transform.CompareTag("Player")) return;
        EntityClass.ReceivedDamage += ReceiveDamage;
    }

    void FixedUpdate() {
        if (EntityClass.Health <= 0) return;
        currentSensorRotation = CurrentRotation();
        RaycastHit2D winningRay = GetWinningGroundRay();

        bool checkCeiling = CheckIfCeiling();
        bool checkGrounded = CheckIfGrounded();

        RaycastHit2D checkWalls = CheckIfWall();
        if (CurrentEnemyAction == EEntityActions.Falling && checkGrounded) AdjustSpeedWithAngle();

        transform.rotation = Quaternion.Euler(0, 0, !DontRotateEntity ? -GetGroundAngle() : 0);
        transform.position += (Vector3)EntityVelocity;

        transform.localScale = new Vector3(
            (GroundSpeed != 0 || FastRotate) && Turn != Mathf.Sign(transform.localScale.x) ? -transform.localScale.x: transform.localScale.x, 
            transform.localScale.y, 
            0);

        Vector2 convertedSpeed = TransformVelocityToPlane();
        EntityVelocity = convertedSpeed + EntityAddVelocity;

        if (!checkGrounded) EntityAddVelocity -= new Vector2(0, PhysicsConst.Gravity);
        else if (checkGrounded && EntityAddVelocity.y < 0) EntityAddVelocity = Vector2.zero;
        if (checkCeiling && EntityAddVelocity.y > 0) EntityAddVelocity = Vector2.zero;
        if (checkWalls)
        {
            float direction = Mathf.Abs(GroundSpeed) > 0 ? Mathf.Sign(GroundSpeed) : Turn;
            float distance = Mathf.Abs(Collider2D.size.x / 2 + .05f) - checkWalls.distance;
            EntityVelocity = new(distance * -direction, EntityVelocity.y);
            EntityAddVelocity = new(0, EntityAddVelocity.y);

            GroundSpeed = 0;
        }

        CurrentEnemyAction = !checkGrounded ? EEntityActions.Falling : EEntityActions.Idle;

        EntityVelocity = new(Mathf.Round(EntityVelocity.x * 10000)/10000, Mathf.Round(EntityVelocity.y * 10000) / 10000);
        RaycastHit2D winningRay2 = GetWinningGroundRay();
        float distanceCalculation = Collider2D.size.y / 2 - winningRay2.distance;
        Vector3 groundPush = winningRay2.normal * distanceCalculation;
        transform.position += groundPush;
    }
    
    //Enemy damage
    private void ReceiveDamage(int damage)
    {
        if (EntityClass.Health > damage) {

            return;
        }
        Collider2D.enabled = false;
        Spriter.enabled = false;
        audioEntity.PlayOneShot(PlayableAudiosEntity["Destroyed"]);
        var newExplosion = Instantiate(ExplosionPrefab, transform);
        newExplosion.transform.position = transform.position;
    }
    //Velocity
    public Vector2 TransformVelocityToPlane() {
        RaycastHit2D winningRay = GetWinningGroundRay();

        return new Vector2((winningRay.normal.y == 0 ? 1 : winningRay.normal.y) * GroundSpeed, -winningRay.normal.x * GroundSpeed);
    }
    public void Jump() {
        GroundAngle = 12600;
        currentSensorRotation = Vector2.zero;
    }
    public void AdjustSpeedWithAngle() {
        float groundAngle = GetGroundAngle();

        if (Mathf.Abs(groundAngle) <= 23) return;
        else if (Mathf.Abs(groundAngle) <= 45) GroundSpeed = EntityAddVelocity.y * 1.5f * -Mathf.Sign(groundAngle);
        else GroundSpeed = EntityAddVelocity.y * 2 * -Mathf.Sign(groundAngle);
        EntityAddVelocity = Vector3.zero;
    }

    //Walls
    public RaycastHit2D CheckIfWall() {
        if (!IsRotationUp()) return new RaycastHit2D();
        float direction = (Mathf.Abs(GroundSpeed) > 0 ? Mathf.Sign(GroundSpeed): Turn);

        Vector3 adjustWithYSpeed = new(0,
            Mathf.Abs(EntityAddVelocity.y) <= 0.001f ? 0 : 
            Collider2D.size.y / 4 * Mathf.Sign(EntityAddVelocity.y));
        Debug.DrawRay(Collider2D.bounds.center + adjustWithYSpeed + (Vector3)EntityVelocity, (Collider2D.size.x / 2 + .2f) * direction * transform.right, Color.magenta);

        return Physics2D.Raycast(Collider2D.bounds.center + adjustWithYSpeed + (Vector3)EntityVelocity, transform.right.normalized * direction, Collider2D.size.x / 2 + .2f, 1 << 3);
    }

    //Ceiling
    public bool CheckIfCeiling() {
        if (EntityVelocity.y < 0 && CurrentEnemyAction == EEntityActions.Falling) return new RaycastHit2D();
        return GetWinningGroundRay(1);
    }

    //Ground
    public float GetGroundAngle() {
        RaycastHit2D winningGround = GetWinningGroundRay();

        float groundAngle = Vector2.SignedAngle(winningGround.normal.normalized, Vector2.up);
        float _savedSign = Mathf.Sign(groundAngle);

        float difference = Mathf.Abs(Mathf.Abs(GroundAngle) - Mathf.Abs(groundAngle));

        if (difference > 79 && groundAngle != 0) groundAngle = GroundAngle;
        if (difference >= 87 && difference < 200) {
            currentSensorRotation = Vector2.zero;
            var _ga = Mathf.Round(GroundAngle); 
            groundAngle = 0;

            bool onCeiling = _ga >= 179 && _ga <= 181;
            EntityAddVelocity.y = !onCeiling ? Mathf.Sin(_ga) * Mathf.Abs(GroundSpeed) * _savedSign : 0;

            GroundSpeed = Mathf.Cos(_ga) * GroundSpeed;
            GroundSpeed = _ga >= 87 && _ga <= 95 ? 0 : GroundSpeed;
        }
        GroundAngle = Mathf.Clamp(Mathf.Abs(groundAngle), 0, 361);

        return groundAngle;
    }
    public RaycastHit2D GetWinningGroundRay(float Direction = -1) {
        RaycastHit2D winningSensor;

        RaycastHit2D sensorA = GetRayAtPosition(Collider2D.size.x / 2, Direction);
        RaycastHit2D sensorB = GetRayAtPosition(-Collider2D.size.x / 2, Direction);

        if (sensorA.distance <= sensorB.distance) winningSensor = sensorA;
        else winningSensor = sensorB;

        if (winningSensor.distance == 0) winningSensor = sensorA;
        if (winningSensor.distance == 0) winningSensor = sensorB;

        Debug.DrawRay(winningSensor.point, winningSensor.normal, Color.green);

        return winningSensor;
    }
    public RaycastHit2D GetRayAtPosition(float Position, float Direction = 1, string AorB = "A") {
        Vector3 directionDown = Direction * currentSensorRotation;
        var Ray = Physics2D.Raycast(Collider2D.bounds.center + (Vector3)EntityVelocity - new Vector3(Direction * directionDown.y * Position, directionDown.x * Direction * Position), directionDown, Collider2D.size.y / 2 + .2f, 1 << 3);
        Debug.DrawRay(Collider2D.bounds.center + (Vector3)EntityVelocity - new Vector3(Direction * directionDown.y * Position, directionDown.x * Direction * Position), directionDown * (Collider2D.size.y / 2 + .2f), Direction < 0? Position < 0 ? Color.magenta : Color.yellow : Color.red);
        //Debug.DrawLine(Collider2D.bounds.center + (Vector3)EntityVelocity - new Vector3(Direction * directionDown.y * Position, directionDown.x * Direction * Position), Ray.point, Direction < 0 ? AorB == "A" ? Color.magenta : Color.yellow : Color.red);
        return Ray;
    }
    public bool CheckIfGrounded() {
        if (EntityVelocity.y > 0 && CurrentEnemyAction == EEntityActions.Falling) return new RaycastHit2D();
        return GetWinningGroundRay();
    }

    //Other
    public Vector3 CurrentRotation() {
        if (DontRotateSensors) return Vector3.up;
        //float groundAngle = GetGroundAngle();
        Vector3 newRotation = transform.up;
        //if (Mathf.Abs(groundAngle) <= 48) newRotation = Vector3.up;
        //else if (groundAngle > 48 && groundAngle <= 125) newRotation = Vector3.right;
        //else if (groundAngle > -125 && groundAngle < -45) newRotation = Vector3.left;
        //else newRotation = Vector3.down;

        return newRotation;
    }
    public bool IsRotationUp() {
        float groundAngle = GetGroundAngle();
        return Mathf.Abs(groundAngle) <= 48;
    }
}

public class PhysicsConst {
    public const float Gravity = 0.021875f / 2;//9.8f * 2;
}