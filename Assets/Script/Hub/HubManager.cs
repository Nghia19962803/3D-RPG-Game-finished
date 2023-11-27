using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace RpgAdventure
{
    public class HubManager : MonoBehaviour
    {
        public Slider healhSlider;

        public void SetMaxHealth(int health)
        {
            healhSlider.maxValue = health;
            SetHealth(health);
        }
        public void SetHealth(int health)
        {
            healhSlider.value = health;
        }
    }
}

