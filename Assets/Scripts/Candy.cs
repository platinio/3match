using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum CandyType { BLUE, GREEN, PINK, RED, WHITE, YELLOW, CANE }

public class Candy : MonoBehaviour
{
    public CandyType type;
    public GameObject destroyEffect;
    public Bonus bonus = new Bonus();
    public int score;

    [HideInInspector] public int x;
    [HideInInspector] public int y;

    private InputController _inputController;
    private UIManager _UIManager;

    void Start()
    {
        _inputController = GameObject.FindObjectOfType<InputController>();
        _UIManager = GameObject.FindObjectOfType<UIManager>();
    }

    void OnMouseEnter()
    {
        _inputController.selectedCandy = this;       
    }

    public void Explode()
    {
        if (_UIManager == null)
        {
            _UIManager = GameObject.FindObjectOfType<UIManager>();
        }
        _UIManager.score += score;
        if(destroyEffect != null)
            Instantiate(destroyEffect , transform.position , Quaternion.identity);
        Destroy(gameObject);
    }
        
	
}

[System.Serializable]
public class Bonus
{
    public int amount = 0;
    public Candy candy = null;
}
