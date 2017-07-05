using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;


public class InputController : MonoBehaviour
{
    public Candy selectedCandy;
    

    Vector2 firstPressPos;
    Vector2 secondPressPos;
    Vector2 currentSwipe;

    private int startX;
    private int startY;
    private int endX;
    private int endY;
    private bool verticalMove;

    private GridManager _gridManager;
    private Candy currentCandy;

   

    void Start()
    {
        _gridManager = GameObject.FindObjectOfType<GridManager>();
    }

    void Update()
    {
        DetectSwipe();
    }

    public void DetectSwipe()
    {
        
         if(Input.GetMouseButtonDown(0))
         {
            
            firstPressPos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);
            currentCandy = selectedCandy;
         }
         if(Input.GetMouseButtonUp(0))
         {
                
            secondPressPos = new Vector2(Input.mousePosition.x,Input.mousePosition.y);             
            currentSwipe = new Vector2(secondPressPos.x - firstPressPos.x, secondPressPos.y - firstPressPos.y);
           
            
            currentSwipe.Normalize();

            startX = currentCandy.x;
            startY = currentCandy.y;
            verticalMove = false;
                       

            //swipe upwards
            if(currentSwipe.y > 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
            {
               if (startY == _gridManager.colums - 1)
                    return;
               endX = startX ;
               endY = startY + 1;
               verticalMove = true;

               _gridManager.OnAnimationComplete += OnAnimationComplete;
               _gridManager.Swap(_gridManager.grid[startX, startY], _gridManager.grid[endX, endY]);
                
                return;
            }
                
            
            //swipe down
            if(currentSwipe.y < 0 && currentSwipe.x > -0.5f && currentSwipe.x < 0.5f)
            {
                if (startY == 0)
                    return;
                endX = startX;
                endY = startY - 1;
                verticalMove = true;

                _gridManager.OnAnimationComplete += OnAnimationComplete;
                _gridManager.Swap(_gridManager.grid[startX, startY], _gridManager.grid[endX, endY]);

                return;
                
            }
            //swipe left
            if(currentSwipe.x < 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
            {
                if (startX == 0)
                    return;
                endX = startX - 1;
                endY = startY;
                
                _gridManager.OnAnimationComplete += OnAnimationComplete;
                _gridManager.Swap(_gridManager.grid[startX, startY], _gridManager.grid[endX, endY]);

                return;
            }
            //swipe right
            if (currentSwipe.x > 0 && currentSwipe.y > -0.5f && currentSwipe.y < 0.5f)
            {
                if (startX == _gridManager.colums - 1)
                    return;
                endX = startX + 1;
                endY = startY;
                
                _gridManager.OnAnimationComplete += OnAnimationComplete;
                _gridManager.Swap(_gridManager.grid[startX, startY], _gridManager.grid[endX, endY]);

                return;
            }

            
    }
    }

    public void OnAnimationComplete()
    {
        bool verticalExplosion = false;
        bool horizontalExplosion = false;

        if (verticalMove)
        {
            horizontalExplosion = horizontalExplosion || _gridManager.CheckHorizontalMatch(startY);
            horizontalExplosion = horizontalExplosion || _gridManager.CheckHorizontalMatch(endY);
            verticalExplosion = verticalExplosion || _gridManager.CheckVerticalMatch(startX);
        }
        else
        {
            verticalExplosion = verticalExplosion || _gridManager.CheckVerticalMatch(startX);
            verticalExplosion = verticalExplosion || _gridManager.CheckVerticalMatch(endX);
            horizontalExplosion = horizontalExplosion || _gridManager.CheckHorizontalMatch(startY);
        }

        _gridManager.OnAnimationComplete -= OnAnimationComplete;

        if (!verticalExplosion && !horizontalExplosion)
            _gridManager.UndoSwap();
        else
        {
            StartCoroutine(_gridManager.CO_CallColapse(verticalExplosion));
            
        }
    }

    
}