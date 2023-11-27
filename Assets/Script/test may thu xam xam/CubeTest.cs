/* chuong trinh nay viet ra de test viec di chuyen cube di tu z=0 den z=10
 va cube se giam toc do di chuyen khi den gan diem z=10 nho lerp*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeTest : MonoBehaviour
{

    float current_Z_Pos = 0;

    void Update()
    {
        current_Z_Pos = Mathf.Lerp(current_Z_Pos, 10, 0.005f);

        transform.position = new Vector3(
            transform.position.x,
            transform.position.y,
            current_Z_Pos);
    }
}
