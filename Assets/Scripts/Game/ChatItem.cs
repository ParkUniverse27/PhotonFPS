using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChatItem : MonoBehaviour
{
    public Text Text;

    public void Init(string name)
    {
        Text.text = name;
    }
}
