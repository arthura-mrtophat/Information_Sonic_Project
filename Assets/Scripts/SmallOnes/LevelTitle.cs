using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class LevelTitle : MonoBehaviour
{
    private PlayerScript Player;

    public Transform ribbonImage;
    public Transform actImage;
    public CanvasGroup blackOut;
    public Transform stageText;
    public TextMeshProUGUI stageNameText;
    private int currentStage = 1;

    private string StageName = "L";

    private void Start() {
        GetComponent<Canvas>().enabled = true;
    }
    public void ReceivePlayer(PlayerScript player, string Name) {
        Player = player;
        StageName = Name;
    }

    private void FixedUpdate() {
        if (Time.timeSinceLevelLoad > 5) { Destroy(gameObject); return; }
        if (Time.timeSinceLevelLoad > 1 && currentStage == 1)
        {
            stageNameText.text = StageName.ToUpper();
            currentStage = 2;
            ribbonImage.LeanMoveLocal(new Vector3(-238, 68, 0), .4f);
            actImage.LeanMoveLocal(new Vector3(283, -155, 0), .45f);
            stageText.LeanMoveLocal(new Vector3(125, 103), .55f);
        }
        if (Time.timeSinceLevelLoad > 2.5f && currentStage == 2)
        {
            currentStage = 3;
            blackOut.LeanAlpha(0, .5f);
        }
        if (Time.timeSinceLevelLoad > 3.1f && currentStage == 3)
        {
            currentStage = 4;
            Player.enabled = true;
        }
        if (Time.timeSinceLevelLoad > 4f && currentStage == 4)
        {
            currentStage++;
            ribbonImage.LeanMoveLocal(new Vector3(-238, 605, 0), .3f);
            actImage.LeanMoveLocal(new Vector3(770, -155, 0), .3f);
            stageText.LeanMoveLocal(new Vector3(770, 103), .3f);
        }
    }
}
