using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace RpgAdventure
{
    public class FixedFollow : MonoBehaviour
    {
        public Transform toFollow = null;


        private void FixedUpdate()
        {
            if (toFollow == null) { return; }
            transform.position = toFollow.position;
            transform.rotation = toFollow.rotation;
        }
        public void SetFolowee(Transform folowee)
        {
            toFollow = folowee;
        }
    }
}

