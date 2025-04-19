using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

public enum SonicActions {
    Grounded,
    Jumping,
    Spinning,
    FreeFall,
    Hurt,
}

public class PlayerScript : MonoBehaviour {
    private EntityScript entityScript;
    private AudioSource audioSonic;
    private GameObject CharacterUICanvas;

    private Animator sonicAnimator;

    public SonicActions CurrentAction = SonicActions.Grounded;
    public float TakenControl = 0;
    private float StartOfTakenControl = 0;
    public float Invulnerabilty = 5;
    private float StartInvuln = -5;

    public Dictionary<string, AudioClip> PlayableAudioSonic = new();

    [Header("Sonic Physics Stuff")]
    public float acceleration_speed = 0.0046875f;
    public float air_acceleration_speed = 0.0046875f / 2;
    public float deceleration_speed = 0.05f;
    public float friction_speed = 0.0046875f;
    public float top_speed = .45f;

    public float roll_friction_speed = .00234375f;
    public float roll_deceleration_speed = .0125f;

    public float slope_factor_rollup = 0.0078125f;
    public float slope_factor_rolldown = 0.03125f;
    public float slope_factor_normal = .0125f;

    [Header("Some Prefab")]
    public GameObject BouncyRing;

    private bool _jumpAsk = false;
    private bool _spinAsk = false;

    void Start() {
        entityScript = GetComponent<EntityScript>();
        sonicAnimator = GetComponent<Animator>();
        audioSonic = GetComponent<AudioSource>();
        CharacterUICanvas = GameObject.FindGameObjectWithTag("CharacterUI");

        CharacterUICanvas.SendMessage("ChangeRingNumber", entityScript.EntityClass.Health);
        CharacterUICanvas.SendMessage("StartCounting");

        entityScript.EntityClass.ReceivedDamage += SonicGotHurt;

        PlayableAudioSonic["Jump"] = Resources.Load<AudioClip>("Sounds/Jump");
        PlayableAudioSonic["Spin"] = Resources.Load<AudioClip>("Sounds/Sonic_Spin");
        PlayableAudioSonic["Hurt"] = Resources.Load<AudioClip>("Sounds/Sonic_Hurt");
    }

    void Update() {
        _jumpAsk |= Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.JoystickButton3);
        _spinAsk = Input.GetMouseButton(1);
    }
    void FixedUpdate() {
        if (entityScript.EntityClass.Health <= 0) {
            Died();
            return;
        }
        CheckForSpin();

        if (CurrentAction == SonicActions.Spinning) SpinMovement();
        else Movement();

        CheckForJump();
        UpdateAnimator();
        entityScript.DontRotateSensors = Mathf.Abs(entityScript.GroundSpeed) < .1f && CurrentAction != SonicActions.Spinning;
        entityScript.Collider2D.size = CurrentAction == SonicActions.Spinning || CurrentAction == SonicActions.Jumping ? new(1.1f, 1.1f) : new(1.1f, 2.1f);
        entityScript.Spriter.enabled = Time.fixedTime - StartInvuln >= Invulnerabilty || CurrentAction == SonicActions.Hurt || Mathf.Round((Time.fixedTime - StartInvuln) * 10) % 2 == 0;
        if (CurrentAction == SonicActions.Hurt && entityScript.CheckIfGrounded()) {
            StartInvuln = Time.fixedTime;
            TakenControl = 0;
            StartOfTakenControl = 0;
            CurrentAction = SonicActions.Grounded;
        }
        if (Mathf.Abs(entityScript.GroundSpeed) > top_speed * 2) entityScript.GroundSpeed = top_speed * 2 * Mathf.Sign(entityScript.GroundSpeed);
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        bool isAHurt = collision.transform.CompareTag("Hurts");
        bool isAnInteractable = collision.transform.CompareTag("Interacts");
        var isEntity = collision.transform.GetComponent<EntityScript>();
        if ((isAHurt || isAnInteractable) && isEntity && (CurrentAction == SonicActions.Jumping || CurrentAction == SonicActions.Spinning)) {
            bool isJumping = !entityScript.CheckIfGrounded();//CurrentAction == SonicActions.Jumping || (CurrentAction == SonicActions.Spinning && !entityScript.CheckIfGrounded());
            isEntity.EntityClass.TakeDamage(1);
            if (!isJumping) return;
            entityScript.EntityAddVelocity *= new Vector2(0, -1.1f);
            return;
        }
        if (Time.fixedTime - StartInvuln < Invulnerabilty) return;
        if (!isAHurt) return;

        entityScript.EntityClass.TakeDamage(1);

    }

    private void UpdateAnimator() {
        float absoluteSpeed = Mathf.Abs(entityScript.GroundSpeed);
        int walkingState = CurrentAction == 0 ? !entityScript.CheckIfGrounded() ? 6 : (absoluteSpeed > acceleration_speed && absoluteSpeed < .35) ? 1:  absoluteSpeed >= .35 ? 2:  0: 
            CurrentAction == SonicActions.Jumping || CurrentAction == SonicActions.Spinning ? 3 : (int)CurrentAction*3;

        //if (absoluteSpeed > acceleration_speed && entityScript.CheckIfWall()) walkingState = 4;

        sonicAnimator.SetInteger("State", walkingState);
        sonicAnimator.speed = walkingState < 3 ? Mathf.Abs(entityScript.GroundSpeed) * 5f : 3;
    }
    private void Movement() {
        if (Time.fixedTime - StartOfTakenControl < TakenControl) return;
        if (entityScript.GroundAngle % 180 != 0) entityScript.GroundSpeed -= slope_factor_normal * -entityScript.GetWinningGroundRay().normal.x;
        var horizontalMove = Input.GetAxis("Horizontal");
        if (horizontalMove != Mathf.Sign(entityScript.GroundSpeed) && Mathf.Abs(horizontalMove) >= 1 && entityScript.CheckIfGrounded()) entityScript.GroundSpeed -= deceleration_speed * Mathf.Sign(entityScript.GroundSpeed);
        if (horizontalMove == 0 && Mathf.Abs(entityScript.GroundSpeed) > friction_speed && entityScript.CheckIfGrounded()) {
            entityScript.GroundSpeed -= friction_speed * Mathf.Sign(entityScript.GroundSpeed);
            return;
        }
        else if (horizontalMove == 0 && Mathf.Abs(entityScript.GroundSpeed) <= friction_speed) {
            entityScript.GroundSpeed = 0;
            return;
        }
        if (Mathf.Abs(entityScript.GroundSpeed) < top_speed) entityScript.GroundSpeed += (entityScript.CheckIfGrounded()? acceleration_speed: air_acceleration_speed) * horizontalMove;
        if (Mathf.Sign(entityScript.GroundSpeed) != entityScript.Turn) entityScript.Turn = (int)Mathf.Sign(horizontalMove);
    } 
    private void SpinMovement() {
        float signSpeed = Mathf.Sign(entityScript.GroundSpeed);
        float sinAngle = -entityScript.GetWinningGroundRay().normal.x;
        float whichSlopeFactor = entityScript.GroundAngle > 0 ? (signSpeed == Mathf.Sign(sinAngle) ? slope_factor_rollup : slope_factor_rolldown): 0;
        if (entityScript.GroundAngle % 180 != 0) entityScript.GroundSpeed -= whichSlopeFactor * -entityScript.GetWinningGroundRay().normal.x;
        entityScript.GroundSpeed -= roll_friction_speed * signSpeed;

        var horizontalMove = Input.GetAxis("Horizontal");
        if (horizontalMove != signSpeed && horizontalMove != 0)
            entityScript.GroundSpeed -= roll_deceleration_speed * signSpeed;
    }
    private void CheckForSpin() {
        if (CurrentAction == SonicActions.Spinning && Mathf.Abs(entityScript.GroundSpeed) <= .01f && entityScript.IsRotationUp()) {
            CurrentAction = SonicActions.Grounded;
            entityScript.IgnoreFalloff = false;
            return;
        }
        if (!_spinAsk) return;
        if (!entityScript.CheckIfGrounded()) return;
        if (Mathf.Abs(entityScript.GroundSpeed) < .02) return;
        if (CurrentAction == SonicActions.Spinning) return;
        entityScript.Collider2D.size = new Vector2(1.1f, 1.1f);
        var winningRay = Physics2D.Raycast(entityScript.Collider2D.bounds.center, -transform.up, 5, 1 << 3);
        float distance = .55f - winningRay.distance;
        transform.Translate(new(0, distance));
        entityScript.IgnoreFalloff = true;

        playAudioClip(PlayableAudioSonic["Spin"]);
        CurrentAction = SonicActions.Spinning;
    }
    private void CheckForJump() {
        if (entityScript.CheckIfGrounded() && CurrentAction == SonicActions.Jumping){
            entityScript.FastRotate = false;
            CurrentAction = SonicActions.Grounded;
        }
        if (!_jumpAsk) return;
        _jumpAsk = false;
        if (!entityScript.CheckIfGrounded()) return;
        CurrentAction = SonicActions.Jumping;

        playAudioClip(PlayableAudioSonic["Jump"]);

        Vector2 groundNormal = entityScript.GetWinningGroundRay().normal;
        Vector2 jumpVector = new(0, groundNormal.normalized.y * .3f);   
        
        entityScript.FastRotate = true;
        entityScript.Jump();

        if (Mathf.Round(groundNormal.normalized.x*10) != 0) entityScript.GroundSpeed = entityScript.GroundSpeed * groundNormal.normalized.x + groundNormal.normalized.x * .3f;

        entityScript.EntityAddVelocity += jumpVector;
    }
    public void SonicGotHurt(int damage) {
        if (damage <= 0) {
            CharacterUICanvas.SendMessage("ChangeRingNumber", entityScript.EntityClass.Health);
            return;
        }
        playAudioClip(PlayableAudioSonic["Hurt"]);
        if (entityScript.EntityClass.Health > 1) {
            for (int _ = 0; _ < Mathf.Clamp(entityScript.EntityClass.Health+damage, 0, 15); _++) {
                Instantiate(BouncyRing, transform.position, Quaternion.identity);
            }
            entityScript.EntityClass.Health = 1;
            CharacterUICanvas.SendMessage("ChangeRingNumber", entityScript.EntityClass.Health);
            TakenControl = 100;
            StartInvuln = Time.fixedTime + 1000;
            StartOfTakenControl = Time.fixedTime + 100;

            entityScript.GroundSpeed = -.1f * Mathf.Sign(entityScript.GroundSpeed);
            entityScript.Turn = (int)Mathf.Sign(entityScript.GroundSpeed);
            entityScript.Jump();
            entityScript.EntityAddVelocity = new Vector3(0, .3f);

            CurrentAction = SonicActions.Hurt;
            return;
        }
        entityScript.Collider2D.enabled = false;
        entityScript.enabled = false;
        entityScript.EntityAddVelocity = new(0, .3f);
    }
    private void Died() {
        entityScript.EntityAddVelocity -= new Vector2(0, PhysicsConst.Gravity);
        transform.position += (Vector3)entityScript.EntityAddVelocity;
    }
    private void playAudioClip(AudioClip audio) {
        audioSonic.PlayOneShot(audio);
    }
}
