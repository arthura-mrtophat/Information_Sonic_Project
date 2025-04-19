using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class CoolTransition : MonoBehaviour
{
    private Collider2D ccollider2D;
    public Transform MaxCorner;
    public Transform MinCorner;

    public SpriteRenderer ChangeBack1;
    public SpriteRenderer ChangeBack2;

    public Tilemap ChangeColor;
    public Tilemap GetColor;
    public CanvasGroup WhiteOut;

    public AudioSource LevelSong;
    public AudioClip ChangeToSong;

    private void Start() {
        ccollider2D = GetComponent<Collider2D>();
    }
    private void OnTriggerEnter2D(Collider2D collision) {
        if (!collision.transform.CompareTag("Player")) return;
        PlayerScript playerScript = collision.GetComponent<PlayerScript>();
        playerScript.CurrentAction = SonicActions.Grounded;
        ccollider2D.enabled = false;
        WhiteOut.alpha = 1;
        MaxCorner.position = new Vector3(65, -31);
        MinCorner.position = new Vector3(-110, -149);
        ChangeBack1.enabled = false;
        ChangeBack2.enabled = true;
        ChangeColor.color = GetColor.color;

        LevelSong.Stop();
        LevelSong.clip = ChangeToSong;
        LevelSong.time = 37;
        LevelSong.Play();

        WhiteOut.LeanAlpha(0, 2);
        Destroy(gameObject);
    }
}
