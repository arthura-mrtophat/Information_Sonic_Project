using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LevelInfo {
    public string Name; // showcase name
    public string Song; // path
    public float Volume;
    //public string Score; // Score Requirements (no implm)

}

public class LevelLogic : MonoBehaviour {
    public LevelInfo LevelInfo { get; private set; }
    public PlayerScript Player;

    public AudioSource StageSong;
    public Canvas StartLevelUI;
    public TextMeshProUGUI TestText;

    private void Start() {
        Player.enabled = false;
        //TestText.text = SceneManager.GetActiveScene().name.Replace(" ", "");

        LevelInfo = JsonUtility.FromJson<LevelInfo>(Resources.Load<TextAsset>($"Levels/{SceneManager.GetActiveScene().name.Replace(" ", "")}").text);
        StageSong = GetComponent<AudioSource>();

        StartLevelUI.GetComponent<LevelTitle>().ReceivePlayer(Player, LevelInfo.Name);

        StageSong.clip = Resources.Load<AudioClip>($"Songs/{LevelInfo.Song}");
        StageSong.volume = LevelInfo.Volume;
        StageSong.Play();
    }
}
