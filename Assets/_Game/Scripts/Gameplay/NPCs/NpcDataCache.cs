using ChatGPT_Detective;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcDataCache : MonoBehaviour
{
    private static NpcDataCache _instance;

    public static NpcDataCache Instance
    {
        get
        {
            if (!_instance)
                _instance = FindFirstObjectByType<NpcDataCache>();

            return _instance;
        }
    }

    [SerializeField] private string _npcTag = "Npc";

    private Dictionary<int, NpcPrompter> _npcPrompters = new Dictionary<int, NpcPrompter>();
    
    private Dictionary<int, NpcPopupDataHolder> _npcPopupData = new Dictionary<int, NpcPopupDataHolder>();
    
    private void Awake()
    {
        NpcDataCache[] caches = FindObjectsByType<NpcDataCache>(FindObjectsInactive.Include,
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
            NpcPrompter prompter = npc.GetComponent<NpcPrompter>();

            NpcPopupDataHolder popupData = npc.GetComponentInChildren<NpcPopupDataHolder>();

            int id = npc.GetHashCode();

            _npcPrompters.Add(id, prompter);

            _npcPopupData.Add(id, popupData);
        }
    }

    public NpcPrompter GetPrompter(int id)
    {
        if (_npcPrompters.TryGetValue(id, out NpcPrompter prompter))
        {
            return prompter;
        }

        return null;
    }
    
    public NpcPopupDataHolder GetPopupData(int id)
    {
        if (_npcPopupData.TryGetValue(id, out NpcPopupDataHolder popupData))
        {
            return popupData;
        }

        return null;
    }
}
