using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcPopupDataHolder : MonoBehaviour
{
    [SerializeField] private Sprite _characterSprite;

    [SerializeField] private string _characterName;

    [SerializeField] private bool _isMale = true;

    [SerializeField] private bool _noInteraction;

    public Sprite CharacterSprite => _characterSprite;

    public string CharacterName => _characterName;

    public bool IsMale => _isMale;

    public bool NoInteraction => _noInteraction;
}
