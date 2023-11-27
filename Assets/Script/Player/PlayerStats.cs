using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace RpgAdventure
{
    public class PlayerStats : MonoBehaviour, IMessageReceiver
    {
        public int maxLevel;
        public int currentLevel;
        public int currentExp;
        public int[] availableLevels;

        public int ExperienceToNextLevel
        {
            get
            {
                return availableLevels[currentLevel] - currentExp;
            }
        }
        private void Awake()
        {
            availableLevels = new int[maxLevel];
            ComputeLevels(maxLevel);
        }
        private void ComputeLevels(int levelCount)
        {
            for (int i = 0; i < levelCount; i++)
            {
                var level = i + 1;
                var levelPow = Mathf.Pow(level, 2);
                var expTolevel = Convert.ToInt32(levelPow * levelCount);
                availableLevels[i] = expTolevel;
            }
        }
        public void OnReceiveMessage(MessageType type, object sender, object msg)
        {
            if(type == MessageType.DEAD)
            {
                GainExperience((sender as Damageable).experience);
            }    
        }
        public void GainExperience(int gainedExp)
        {
            //Debug.Log("Gaining Experience " + exp);
            if (gainedExp > ExperienceToNextLevel)
            {
                var remainderExp = gainedExp - ExperienceToNextLevel;
                currentExp = 0;
                currentLevel++;
                GainExperience(remainderExp);
            }
            else if (gainedExp == ExperienceToNextLevel)
            {
                currentLevel++;
                currentExp = 0;
            }
            else
            {
                currentExp += gainedExp;
            }
        }
    }
}

