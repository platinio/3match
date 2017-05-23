using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public int score
    {
        set
        {
            _score = value;
            scoreText.text = "Score: " + _score;
        }
        get { return _score; }
    }
    private int _score;

    public Text scoreText;


}
