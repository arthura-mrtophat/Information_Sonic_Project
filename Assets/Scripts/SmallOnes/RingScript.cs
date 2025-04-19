using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RingScript : MonoBehaviour
{
    private Collider2D collider2D;
    public ParticleSystem RingShine;
    public AudioClip CollectRing;
    private AudioSource playAudio;

    private float AwakeStart = 0;

    private void Awake() {
        playAudio = gameObject.AddComponent<AudioSource>();
        collider2D = GetComponent<Collider2D>();
        collider2D.enabled = false;
        playAudio.clip = CollectRing;
        AwakeStart = Time.time;
    }
    private void FixedUpdate() {
        if (Time.time - AwakeStart > .5f) collider2D.enabled = true;
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (!collision.transform.CompareTag("Player")) return;
        AwakeStart = Time.time + 1250;
        GetComponent<SpriteRenderer>().enabled = false;
        collider2D.enabled = false;
        EntityScript entityScript = collision.GetComponent<EntityScript>();
        entityScript.EntityClass.TakeDamage(-1);
        var coolEffect = Instantiate(RingShine.gameObject, transform.position-new Vector3(0, 0, 4), Quaternion.identity);
        var doEffect = coolEffect.GetComponent<ParticleSystem>();
        doEffect.Emit(3);
        playAudio.Play();
        Destroy(gameObject, 2);
        Destroy(coolEffect, 2);
    }
}
