using System.Collections;
using UnityEngine;

namespace RpgAdventure
{
    public class Dissolve : MonoBehaviour
    {
        public float dissolveTime = 6.0f;

        private void Awake()
        {
            dissolveTime += Time.time;
        }

        void Update()
        {
            if(Time.time >= dissolveTime)
            {
                Destroy(gameObject);
            }
        }
    }
}
