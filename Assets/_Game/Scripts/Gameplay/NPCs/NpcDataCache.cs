using ChatGPT_Detective;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcDataCache : MonoBehaviour
{
    private struct NpcData
    {
        private NpcPrompter _npcPrompter;

        private NpcPopupDataHolder _npcPopupDataHolder;

        private NpcInteractionHandler _npcInteractionHandler;

        public NpcPrompter Prompter => _npcPrompter;

        public NpcPopupDataHolder PopupData => _npcPopupDataHolder;

        public NpcInteractionHandler InteractionHandler => _npcInteractionHandler;

        public NpcData(NpcPrompter prompter, NpcPopupDataHolder popupData,
            NpcInteractionHandler interactionHandler)

        {
            _npcPrompter = prompter;
            _npcPopupDataHolder = popupData;
            _npcInteractionHandler = interactionHandler;
        }
    }

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

    private Dictionary<int, NpcData> _npcCache = new Dictionary<int, NpcData>();
    
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

            NpcPopupDataHolder popupData = npc.GetComponent<NpcPopupDataHolder>();

            NpcInteractionHandler interactionHandler = npc.GetComponent<NpcInteractionHandler>();

            int id = npc.GetHashCode();

            NpcData npcData = new NpcData(prompter, popupData, interactionHandler);

            _npcCache.Add(id, npcData);
        }
    }

    public NpcPrompter GetPrompter(int id)
    {
        if (_npcCache.TryGetValue(id, out NpcData npcData))
        {
            return npcData.Prompter;
        }

        return null;
    }
    
    public NpcPopupDataHolder GetPopupData(int id)
    {
        if (_npcCache.TryGetValue(id, out NpcData npcData))
        {
            return npcData.PopupData;
        }

        return null;
    }
    
    public NpcInteractionHandler GetInteractionHandler(int id)
    {
        if (_npcCache.TryGetValue(id, out NpcData npcData))
        {
            return npcData.InteractionHandler;
        }

        return null;
    }
}
