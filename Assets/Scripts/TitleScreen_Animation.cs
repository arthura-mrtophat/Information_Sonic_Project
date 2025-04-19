using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TitleScreen_Animation : MonoBehaviour {
    public Transform Sonic1;
    public Transform Ring;
    public Transform Thing;
    public Transform Wings;
    public GameObject Black;

    private SpriteRenderer SpriteBlack;

    public Animator PressStart;
    public AudioSource TitleMusic;

    private AudioSource sound_Accept;

    private bool requestedMenu = false;

    private float volumetimer = 0;

    [SerializeField] private Vector3 originalPosition;

    private void Start() {
        originalPosition = Ring.position;
        SpriteBlack = Black.GetComponent<SpriteRenderer>();
        sound_Accept = GetComponent<AudioSource>();
    }
    private void Update() {
        requestedMenu |= Input.anyKeyDown && SpriteBlack.color.a <= 0;
    }
    void FixedUpdate() {
        if (SpriteBlack.color.a <= 0 && Time.fixedTime == 27) requestedMenu = true;
        if (requestedMenu) {
            requestedMenu = false;
            PressStart.speed = 4.75f;
            Black.SendMessage("Change");
            volumetimer = Time.fixedTime;
            sound_Accept.Play();
        }
        if (volumetimer != 0) TitleMusic.volume = .5f - (Time.fixedTime - volumetimer) / 3;
        float time = Time.fixedTime;
        Ring.position = originalPosition + new Vector3(0, Mathf.Round(Mathf.Sin(time) * 10) / 100);
        Vector3 currentPosition = Ring.position;
        
        Wings.position = currentPosition 
            + new Vector3(0, Mathf.Round(Mathf.Sin(time) * 15) / 100)
            + new Vector3(.25f, 1.75f);
        Thing.position = currentPosition
            + new Vector3(0, Mathf.Round(Mathf.Sin(time * 2) * 11) / 200)
            - new Vector3(.02f, 2.1f, 1);

        Vector3 thingPosition = Thing.position;

        Sonic1.position = thingPosition
            + new Vector3(0, Mathf.Round(Mathf.Sin(time * 2) * 11) / 1000)
            + new Vector3(-1f, 3.99f);
    }
}
