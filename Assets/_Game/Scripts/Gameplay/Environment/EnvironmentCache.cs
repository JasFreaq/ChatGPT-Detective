using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    public class EnvironmentCache : MonoBehaviour
    {
        private static EnvironmentCache s_instance;

        public static EnvironmentCache Instance
        {
            get
            {
                if (!s_instance)
                    s_instance = FindFirstObjectByType<EnvironmentCache>();

                return s_instance;
            }
        }

        private void Awake()
        {
            EnvironmentCache[] caches = FindObjectsByType<EnvironmentCache>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            if (caches.Length > 1)
            {
                Destroy(gameObject);
            }
            else
            {
                s_instance = this;
            }
        }

        private Dictionary<int, Interactable> m_interactableCache = new Dictionary<int, Interactable>();

        private void Start()
        {
            Interactable[] interactables = FindObjectsByType<Interactable>(FindObjectsInactive.Include,
                FindObjectsSortMode.None);

            foreach (Interactable interactable in interactables)
            {
                m_interactableCache.Add(interactable.gameObject.GetHashCode(), interactable);
            }
        }

        public Interactable GetInteractable(int id)
        {
            if (m_interactableCache.TryGetValue(id, out Interactable interactable))
            {
                return interactable;
            }

            return null;
        }
    }
}