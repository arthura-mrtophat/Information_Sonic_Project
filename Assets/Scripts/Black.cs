using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Black : MonoBehaviour {
    public SpriteRenderer spriteRenderer;

    private float timer = 0f;
    private bool doneFirst = false;

    private void Start() => spriteRenderer = GetComponent<SpriteRenderer>();

    private void FixedUpdate() {
        if (timer == 0) return;
        if (!doneFirst) {
            float expect = (100 - (Time.fixedTime - timer) * 30) / 100;

            spriteRenderer.color = new Color(1, 1, 1, expect);
            if (expect < 0) {
                doneFirst = true;
                timer = 0;
            }
        } else {
            float expect = (Time.fixedTime - timer);

            spriteRenderer.color = new Color(0, 0, 0, expect);
            if (expect >= 3) SceneManager.LoadScene("Main Menu");
        }
    }

    public void Change() {
        timer = Time.fixedTime;
        spriteRenderer.color = !doneFirst ? Color.white : new Color(0, 0, 0, 0);
    }
}
