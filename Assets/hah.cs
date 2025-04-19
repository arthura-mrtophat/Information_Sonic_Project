using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class hah : MonoBehaviour {
    void Start() {
        GetComponent<Button>().onClick.AddListener(() => funnyAction());
    }
    private void funnyAction() {
        SceneManager.LoadScene(2);
    }
}
