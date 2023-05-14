using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public Text KillLogText;
    public Text SpectatorText;
    public Image Bar;
    public Image Fade;
    public GameObject RespawnCanvas;
    public Text RespawnTimerText;

    Tween killLogTween;

    bool logShowed;
    float timer;

    private void Awake() 
    {
        Instance = this;
        Fade.gameObject.SetActive(true);
    }

    private void Update() 
    {
        if(logShowed)
        {
            timer += Time.deltaTime;
            if(timer >= 3)
            {
                logShowed = false;
                killLogTween = KillLogText.DOFade(0, 0.1f);
            }
        }    
    }

    public void DoFade(float targetAlpha, Action action)
    {
        Fade.DOFade(targetAlpha, 0.3f).SetEase(Ease.OutSine).OnComplete(()=>action?.Invoke());
    }

    public void SwipeFade(Action action)
    {
        DoFade(1, delegate
        {
            DoFade(0, action);
        });
    }

    public void ShowKillLog(string log)
    {
        timer = 0;
        killLogTween?.Kill(false);
        logShowed = true;
        killLogTween = KillLogText.DOFade(0, 0.1f).OnComplete(delegate
        {
            KillLogText.text = log;
            KillLogText.DOFade(1, 0.1f);
        });
    }

    public void UpdateHP(float cur)
    {
        Bar.fillAmount = cur*0.01f/1f;
    }

    public void SetRespawnText(int sec)
    {
        RespawnTimerText.text = $"부활까지: <b>{sec}</b>초";
    }
}
