using System;
using System.Collections.Generic;
using System.Linq;
using BepInEx;
using UnityEngine;

namespace OdinUndercroft
{
    class Basement : MonoBehaviour
    {
        Bounds interiorBounds;
        Collider[] localColliders;

        public static List<Basement> allBasements = new List<Basement>();

        GameObject b;

        public string mUID;
        void Awake()
        {

            allBasements.Add(this);

            localColliders = gameObject.GetComponentsInChildren<Collider>();

            b = transform.Find("BlackoutBox").gameObject;
            b.layer = 4; // Allows building without disabling zone detection, idk what this layer is actually for
            interiorBounds = b.GetComponent<BoxCollider>().bounds;
        }

        private void OnEnable()
        {
            mUID = System.Guid.NewGuid().ToString();
        }
        void OnDestroy()
        {
            allBasements.Remove(this);
        }

        public bool CanBeRemoved()
        {
            var ol = Physics.OverlapBox(interiorBounds.center, interiorBounds.extents).Where(x => !localColliders.Contains(x));
            foreach (var item in ol)
            {
                Debug.Log(item.name + " is preventing basement from being destroyed");
            }
            return !ol.Any();
        }
    }
}