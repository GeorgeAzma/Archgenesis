using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Map : MonoBehaviour
{
    public GameObject cube;
    void Start()
    {
        Vector3 spawn = new Vector3(16, -3, 16);
        Vector3 size = new Vector3(2f, 2f, 2f);
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                for (int z = 0; z < 8; z++)
                {
                    Instantiate(cube, Vector3.Scale(new Vector3(x, y, z), size) + spawn, Quaternion.identity);
                }
            }
        }
    }
}
