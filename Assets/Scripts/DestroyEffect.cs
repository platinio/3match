using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestroyEffect : MonoBehaviour 
{

    public float lifeTime;

    private float _timer = 0.0f;

    void Update()
    {
        _timer += Time.deltaTime;

        if (_timer >= lifeTime)
            Destroy(gameObject);
    }
}
