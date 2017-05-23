using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GridManager : MonoBehaviour
{
   
    public List<Candy> candies;
    public int rows;
    public int colums;
    public int minCadniesMatch;
    public float animationTime;
    [HideInInspector] public Tile[,] grid;
    public event Action OnAnimationComplete = delegate { };

   
    private float _tileSizeX;
    private float _tileSizeY;
    private List<Candy> _previusCandies;
    private bool _isAnimating = false;
    private Move _previusMove = new Move();

    void Start()
    {
        InitializeGrid();
    }

    private void InitializeGrid()
    {
        grid = new Tile[rows, colums];

        SpriteRenderer sprite = candies[0].GetComponent<SpriteRenderer>();
        _tileSizeX = sprite.bounds.size.x;
        _tileSizeY = sprite.bounds.size.y;

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < colums; y++)
            {

                grid[x, y] = new GameObject("Tile (" + x + " , " + y + ")").AddComponent<Tile>() as Tile;
                int index = UnityEngine.Random.Range(0, candies.Count);
                Candy candy = candies[index];
                candy.x = x;
                candy.y = y;

                //chuquear que no tenemos ninguna combinacion al empezar
                while (CheckPotentialHorizontalMatch(candy) || CheckPotentialVerticalMatch(candy))
                {
                    index = UnityEngine.Random.Range(0, candies.Count);
                    candy = candies[index];
                    candy.x = x;
                    candy.y = y;
                }


                grid[x, y].candy = Instantiate(candy, new Vector2(x * _tileSizeX, y * _tileSizeY), Quaternion.identity);
                grid[x, y].transform.parent = transform;
                grid[x, y].candy.transform.parent = grid[x, y].transform;
                
            }
        }
    }

    private bool CheckPotentialVerticalMatch(Candy candy)
    {
        CandyType type = candy.type;

        if (candy.y >= 2)
        {
            if (grid[candy.x, candy.y - 1].candy.type == type && grid[candy.x, candy.y - 2].candy.type == type)
                return true;
        }
        
        return false;

    }

    private bool CheckPotentialHorizontalMatch(Candy candy)
    {
        CandyType type = candy.type;
        if (candy.x >= 2)
        {
            if (grid[candy.x - 1, candy.y].candy.type == type && grid[candy.x - 2, candy.y].candy.type == type)
                return true;
        }
       

        return false;

    }

    
    public void Swap(Tile candy_1 , Tile candy_2)
    {
        if (_isAnimating)
            return;

        //guardar el movimiento en caso de no ser un movimiento valido podemos retornar
        _previusMove = new Move();
        _previusMove.candy = candy_1.candy;
        _previusMove.otherCandy = candy_2.candy;
       
        //mover
        StartCoroutine(CO_ChangePosition(candy_1.candy.transform , new Vector3(candy_2.candy.x * _tileSizeX , candy_2.candy.y * _tileSizeY , 0.0f)));
        StartCoroutine(CO_ChangePosition(candy_2.candy.transform, new Vector3(candy_1.candy.x * _tileSizeX , candy_1.candy.y * _tileSizeY , 0.0f) , true));

        //intercambiar valores
        Transform tempTransform = candy_1.candy.transform.parent;
        candy_1.candy.transform.parent = candy_2.candy.transform.parent;
        candy_2.candy.transform.parent = tempTransform;

        Candy tempCandy = candy_1.candy;
        candy_1.candy = candy_2.candy;
        candy_2.candy = tempCandy;

        int temp = candy_1.candy.x;
        candy_1.candy.x = candy_2.candy.x;
        candy_2.candy.x = temp;

        temp = candy_1.candy.y;
        candy_1.candy.y = candy_2.candy.y;
        candy_2.candy.y = temp;

        
    }

    //funcion para mover los caramelos hacia abajo, efecto de gravedad
    public void Collapse(bool vertical)
    {
       
        List<Candy> candies = new List<Candy>();
        int move = 0;


        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < colums; y++)
            {
                if (grid[x, y].candy == null)
                {                    
                    int n = 0;
                    for (n = y + 1; n < colums; n++)
                    {
                        if (grid[x, n].candy != null)
                            candies.Add(grid[x, n].candy);
                        else
                        {
                            if (candies.Count > 0)
                                break;
                            else
                                move++; 

                        }
                                                                               
                    }
                    
                    if (vertical)
                    {
                        move = move == 0 ? 1 : (move + 1);
                        //si la explosion sucede en el borde superior solo se rellenara con caramelos nuevos
                        if (candies.Count > 0)
                            StartCoroutine(CO_MoveDown(candies, move , true));
                        else
                            FillEmptySpaces();
                        return;
                    }         
                    
                    
                }
                
            }

        }

        //si la explosion es horizontal debe haber al menos un movimiento hacia abajo
        move = move == 0 ? 1 : move;
        if (candies.Count > 0)
            StartCoroutine(CO_MoveDown(candies, move, vertical));
        else
            FillEmptySpaces();
        
    }

    //coroutine para mover los caramelos seleccionados hacia abajo
    IEnumerator CO_MoveDown(List<Candy> candies , int distance , bool vertical = false)
    {
        float time = 0.0f;
        float duration = animationTime * distance;
        
        List<Vector3> startPos = new List<Vector3>();
        List<Vector3> endPos = new List<Vector3>();

        for (int n = 0; n < candies.Count; n++)
        {
            startPos.Add(candies[n].transform.position);
        }
        
         for (int n = 0; n < candies.Count; n++)
        {
             Vector3 pos = candies[n].transform.position;
             pos.y -= (distance * _tileSizeY);
             endPos.Add(pos);
        }
        
        while (time <= duration)
        {
            float t = time / duration;
            for (int n = 0; n < candies.Count; n++)
            {
                if (n < startPos.Count)
                {
                    candies[n].transform.position = Vector3.Lerp(startPos[n], endPos[n], t);
                }
                
            }
            time += Time.deltaTime;
            yield return null;
        }
        
        for (int n = 0; n < candies.Count; n++)
        {
            //identificar valores vacios dependiendo del tipo de explosion
            if(!vertical)
                grid[candies[n].x, colums - 1].candy = null;
            else
                grid[candies[n].x, candies[n].y].candy = null;
            
            candies[n].y-=distance;

            grid[candies[n].x, candies[n].y].candy = candies[n];
            candies[n].transform.parent = grid[candies[n].x, candies[n].y].transform;

            if (n < endPos.Count)
            {
                candies[n].transform.position = endPos[n];
            }
        }
        
        yield return new WaitForSeconds(0.2f);
        FillEmptySpaces();
    }

    //rellenamos espacios vacios con nuevos caramelos
    private void FillEmptySpaces()
    {
        List<Candy> newCandies = new List<Candy>();

        for (int x = 0; x < rows; x++)
        {
            for (int y = 0; y < colums; y++)
            {
                if (grid[x, y].candy == null)
                {
                    int index = UnityEngine.Random.Range(0, candies.Count);
                    grid[x, y].candy = Instantiate(candies[index], new Vector2(x * _tileSizeX, y * _tileSizeY), Quaternion.identity);
                    grid[x, y].transform.parent = transform;
                    grid[x, y].candy.transform.parent = grid[x, y].transform;
                    grid[x, y].candy.x = x;
                    grid[x, y].candy.y = y;

                    newCandies.Add(grid[x, y].candy);
                }

            }

        }

        //si se añaden nuevos caramelos chequeamos colisiones en todos los caramelos
        bool vertical = false;
        bool horizontal = false;

        if (newCandies.Count > 0)
        {
            for (int x = 0; x < rows; x++)
            {
                for (int y = 0; y < colums; y++)
                {
                    
                    horizontal = CheckHorizontalMatch(y);
                    if (horizontal)
                    {
                        StartCoroutine(CO_CallColapse(false));
                        return;
                    }
                        

                    vertical = CheckVerticalMatch(x);
                    if (vertical)
                    {
                        StartCoroutine(CO_CallColapse(true));
                        return;
                    }
                        
                }

            }


            if (vertical || horizontal)
            {
               StartCoroutine(CO_CallColapse(vertical));
            }

               
        }
        

    }

    public IEnumerator CO_CallColapse(bool verticalExplosion)
    {
        yield return new WaitForSeconds(0.1f);
        Collapse(verticalExplosion);
    }
    
    //funcion para deshacer el movimiento anterior
    public void UndoSwap()
    {
        int x = _previusMove.candy.x;
        int y = _previusMove.candy.y;

        int otherX = _previusMove.otherCandy.x;
        int otherY = _previusMove.otherCandy.y;
                
        Swap(grid[x , y], grid[otherX , otherY]);
        
    }

    IEnumerator CO_ChangePosition(Transform obj , Vector3 targetPos , bool callOnComplete = false)
    {
        _isAnimating = true;

        float time = 0.0f;
        float duration = 0.1f;
        Vector3 startPos = obj.position;

        while (time <= duration)
        {
            float t = time / duration;
            obj.position = Vector3.Lerp(startPos, targetPos, t);
            time += Time.deltaTime;
            yield return null;
        }

        obj.position = targetPos;

        _isAnimating = false;

        if (callOnComplete)
        {
            yield return new WaitForSeconds(0.2f);
            OnAnimationComplete();

        } 

        
    }

    public bool CheckHorizontalMatch(int y)
    {

        bool valid = false;

        for (int x = 0; x < rows; x++)
        {
            if (x == 0)
            {
                _previusCandies = new List<Candy>();
                _previusCandies.Add(grid[x, y].candy);
            }
            else
            {
                if (_previusCandies.Count > 0)
                {
                   
                    if (_previusCandies[_previusCandies.Count - 1].type == grid[x, y].candy.type)
                        _previusCandies.Add(grid[x, y].candy);
                    
                    else
                    {
                        if (_previusCandies.Count >= 3)
                        {                     
                            DestroyCandies(_previusCandies);
                            valid = true;                            
                        }                            
                        
                        _previusCandies = new List<Candy>();
                        _previusCandies.Add(grid[x , y].candy);
                    }
                }

                else
                    _previusCandies.Add(grid[x,y].candy);
                
            }
        }

        if (_previusCandies.Count >= 3)
        {
            DestroyCandies(_previusCandies);
            valid = true;
        }

        return valid;
                
    }

    public bool CheckVerticalMatch(int x)
    {
        bool valid = false;

        for (int y = 0; y < colums; y++)
        {
            if (y == 0)
            {
                _previusCandies = new List<Candy>();
                _previusCandies.Add(grid[x, y].candy);
            }
            else
            {
                if (_previusCandies.Count > 0)
                {

                    if (_previusCandies[_previusCandies.Count - 1].type == grid[x, y].candy.type)
                        _previusCandies.Add(grid[x, y].candy);

                    else
                    {
                        if (_previusCandies.Count >= 3)
                        {
                            DestroyCandies(_previusCandies);
                            valid = true;
                        }

                        _previusCandies = new List<Candy>();
                        _previusCandies.Add(grid[x, y].candy);
                    }
                }

                else
                    _previusCandies.Add(grid[x, y].candy);

            }
        }

        if (_previusCandies.Count >= 3)
        {
            DestroyCandies(_previusCandies);
            valid = true;
        }

        return valid;
            

    }

    private void DestroyCandies(List<Candy> candies)
    {
        //si el caramelo posee bonus
        Candy bonusCandy = null;
        if (_previusCandies[0].bonus.candy != null)
        {
            if (_previusCandies.Count >= _previusCandies[0].bonus.amount)
            {
                bonusCandy = _previusCandies[0].bonus.candy;
            }
        }

        for (int n = 0; n < candies.Count; n++)
        {
            candies[n].Explode();
        }

        
        int x = _previusCandies[0].x;
        int y = _previusCandies[0].y;
              

        if (bonusCandy != null)
        {
            bonusCandy.x = x;
            bonusCandy.y = y;

            grid[bonusCandy.x, bonusCandy.y].candy = Instantiate(bonusCandy, new Vector2(bonusCandy.x * _tileSizeX, bonusCandy.y * _tileSizeY), Quaternion.identity);
            grid[bonusCandy.x, bonusCandy.y].candy.transform.parent = grid[bonusCandy.x, bonusCandy.y].transform;
        }
        
        if(_previusMove != null)
            _previusMove = null;
        
    }

       

}


