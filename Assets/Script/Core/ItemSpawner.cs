using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace RpgAdventure
{
    public class ItemSpawner : MonoBehaviour
    {
        public GameObject itemPrefab;
        public LayerMask targetLayers;
        public UnityEvent<ItemSpawner> onItemPickup;
        void Awake()
        {
            Instantiate(itemPrefab, transform);
            Destroy(transform.GetChild(0).gameObject);

            onItemPickup.AddListener
                (FindObjectOfType<InventoryManager>().OnItemPickup);
        }

        private void OnTriggerEnter(Collider other)
        {
            //Debug.Log("triggering");
            if (0 != (targetLayers.value & 1 << other.gameObject.layer))
            {
                Debug.Log("adding Item");
                onItemPickup.Invoke(this);
                //Destroy(gameObject);
            }
        }
    }
}

