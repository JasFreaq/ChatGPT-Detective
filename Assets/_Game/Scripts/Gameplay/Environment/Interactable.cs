using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ChatGPT_Detective
{
    public abstract class Interactable : MonoBehaviour
    {
        public abstract string GetInteractionMessage();

        public abstract void Interact();
    }
}