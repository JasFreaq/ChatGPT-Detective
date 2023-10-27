using ChatGPT_Detective;
using System.Collections.Generic;
using UnityEngine;

public class NpcDataCache : MonoBehaviour
{
    private struct NpcData
    {
        private NpcPrompter m_npcPrompter;

        private NpcPopupDataHolder m_npcPopupDataHolder;

        private NpcInteractionHandler m_npcInteractionHandler;

        public NpcPrompter Prompter => m_npcPrompter;

        public NpcPopupDataHolder PopupData => m_npcPopupDataHolder;

        public NpcInteractionHandler InteractionHandler => m_npcInteractionHandler;

        public NpcData(NpcPrompter prompter, NpcPopupDataHolder popupData,
            NpcInteractionHandler interactionHandler)

        {
            m_npcPrompter = prompter;
            m_npcPopupDataHolder = popupData;
            m_npcInteractionHandler = interactionHandler;
        }
    }

    private static NpcDataCache s_instance;

    public static NpcDataCache Instance
    {
        get
        {
            if (!s_instance)
                s_instance = FindFirstObjectByType<NpcDataCache>();

            return s_instance;
        }
    }

    [SerializeField] private string m_npcTag = "Npc";

    private Dictionary<int, NpcData> m_npcCache = new Dictionary<int, NpcData>();
    
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
            s_instance = this;
        }
    }

    private void Start()
    {
        GameObject[] npcs = GameObject.FindGameObjectsWithTag(m_npcTag);

        foreach (GameObject npc in npcs)
        {
            NpcPrompter prompter = npc.GetComponent<NpcPrompter>();

            NpcPopupDataHolder popupData = npc.GetComponent<NpcPopupDataHolder>();

            NpcInteractionHandler interactionHandler = npc.GetComponent<NpcInteractionHandler>();

            int id = npc.GetHashCode();

            NpcData npcData = new NpcData(prompter, popupData, interactionHandler);

            m_npcCache.Add(id, npcData);
        }
    }

    public NpcPrompter GetPrompter(int id)
    {
        if (m_npcCache.TryGetValue(id, out NpcData npcData))
        {
            return npcData.Prompter;
        }

        return null;
    }
    
    public NpcPopupDataHolder GetPopupData(int id)
    {
        if (m_npcCache.TryGetValue(id, out NpcData npcData))
        {
            return npcData.PopupData;
        }

        return null;
    }
    
    public NpcInteractionHandler GetInteractionHandler(int id)
    {
        if (m_npcCache.TryGetValue(id, out NpcData npcData))
        {
            return npcData.InteractionHandler;
        }

        return null;
    }
}
