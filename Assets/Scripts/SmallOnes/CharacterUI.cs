using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CharacterUI : MonoBehaviour
{
    public TextMeshProUGUI RingsShowcase;

    public TextMeshProUGUI TimeText;
    public TextMeshProUGUI Score;
    public TextMeshProUGUI Rings;

    public float TimeStarted = -1;

    public void ChangeRingNumber(float Number) {
        Rings.text = Convert.ToString(Mathf.Clamp(Number-1, 0, 999));
    }
    public void StartCounting() {
        TimeStarted = Time.time;
    }

    private void FixedUpdate() {
        if (TimeStarted == -1) return;
        float currentTime = Time.time - TimeStarted;
        RingsShowcase.color = Rings.text == "0" && Mathf.Round(currentTime) % 2 == 0 ? Color.red : Color.yellow;
        float seconds = (int)Mathf.Round(currentTime) % 60;
        float milliseconds = Mathf.Round(currentTime * 100) % 100;
        float minutes = (int)seconds / 60;

        string justIncase = seconds < 10 ? "0" : "";
        string justIncase2 = (milliseconds < 10 ? "0" : "");
        TimeText.text = $"{minutes}:{justIncase}{seconds}.{justIncase2}{milliseconds}";
    }
}
