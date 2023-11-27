/*chuong trinh nay se lam cho camera follow player 
 nhung camera alway nam sau lung cua player
neu muon camera tro thanh camera thuong thi thay doi gia tri tai line 26 = 0*/
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    [SerializeField]
    private Transform target;

    void LateUpdate()
    {
        if (!target)
        {
            return;
        }

        float currentRotationAngle = transform.eulerAngles.y;
        float wantedRotationAngle = target.eulerAngles.y;

        currentRotationAngle = Mathf.LerpAngle(
            currentRotationAngle,
            wantedRotationAngle,
            0.01f);

        transform.position = new Vector3(
            target.position.x,
            5.0f,
            target.position.z);

        Quaternion currentRotation = Quaternion.Euler(0, currentRotationAngle, 0);

        Vector3 rotatedPosition = currentRotation * Vector3.forward;

        transform.position -= rotatedPosition * 10;

        transform.LookAt(target);
    }
}
