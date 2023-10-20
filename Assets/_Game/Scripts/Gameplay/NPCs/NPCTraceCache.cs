using ChatGPT_Detective;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCTraceCache : MonoBehaviour
{
    private static NPCTraceCache _instance;

    public static NPCTraceCache Instance
    {
        get
        {
            if (!_instance)
                _instance = FindFirstObjectByType<NPCTraceCache>();

            return _instance;
        }
    }

    [SerializeField] private string _npcTag = "NPC";

    private Dictionary<int, NPCPopupDataHolder> _npcPopupData = new Dictionary<int, NPCPopupDataHolder>();

    private void Awake()
    {
        NPCTraceCache[] caches = FindObjectsByType<NPCTraceCache>(FindObjectsInactive.Include,
            FindObjectsSortMode.None);

        if (caches.Length > 1)
        {
            Destroy(gameObject);
        }
        else
        {
            _instance = this;
        }
    }

    private void Start()
    {
        GameObject[] npcs = GameObject.FindGameObjectsWithTag(_npcTag);

        foreach (GameObject npc in npcs)
        {
            BoxCollider collider = npc.GetComponentInChildren<BoxCollider>();
            NPCPopupDataHolder popupData = npc.GetComponentInChildren<NPCPopupDataHolder>();

            _npcPopupData.Add(collider.GetHashCode(), popupData);
        }
    }

    public NPCPopupDataHolder GetPopupData(int id)
    {
        if (_npcPopupData.TryGetValue(id, out NPCPopupDataHolder popupData))
        {
            return popupData;
        }

        return null;
    }
}
