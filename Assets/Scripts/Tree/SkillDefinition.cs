using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName="Game/Skills/Skill", fileName="Skill_")]
public class SkillDefinition : ScriptableObject
{
    public string id;              // unique (z.B. "skill_dash")
    public string displayName;
    public Sprite icon;
    public int cost = 1;           // Skillpunkte
    public List<SkillDefinition> prerequisites = new(); // direkte Vorbedingungen
    [Header("Editor Layout")]
    public Vector2 canvasPosition; // Position im Content (Tree-Canvas)
}
