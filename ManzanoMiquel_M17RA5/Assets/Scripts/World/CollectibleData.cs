using UnityEngine;
using UnityEngine.Events;

// ════════════════════════════════════════════════════════════════════════════
// CollectibleData  (ScriptableObject)
// Crea via: clic dret → Create → RPG → CollectibleData
// L'enunciat requereix que els objectes tinguin ScriptableObjects associats.
// ════════════════════════════════════════════════════════════════════════════

[CreateAssetMenu(menuName = "RPG/CollectibleData", fileName = "NouCollectible")]
public class CollectibleData : ScriptableObject
{
    public string itemName;
    [TextArea]
    public string description;
    public Sprite icon;           // Icona per al HUD
    public GameObject weaponPrefab;   // Prefab visual que s'adjunta al personatge
}