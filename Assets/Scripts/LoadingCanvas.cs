using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadingCanvas : MonoBehaviour
{
    public static LoadingCanvas Instance;
    public Text Text;

    private void Awake() 
    {
        Instance = this;
        Show(false);
    }

    public void Show(bool isShow)
    {
        gameObject.SetActive(isShow);
    }   

    public void UpdateError(string text)
    {
        Text.text = "에러: " + text + "\n" + "다시 시도중";
    }

    public void UpdateText(string text)
    {
        Text.text = text;
    }
}
