using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    /// <summary>
    /// SetBattlePhaseDisplayText(string): Sets the text
    /// for the Battle Phase display on the UI.
    /// </summary>
    /// <param name="_battleText">String representing the current Battle State.</param>
    public void SetBattlePhaseDisplayText(string _battleText)
    {
        // Set UI Battle Phase Display Text.
        GameObject _battlePhaseDisplayGO = GameObject.Find("BattlePhaseDisplay");
        Text _battlePhaseText = _battlePhaseDisplayGO.GetComponentInChildren<Text>();
        _battlePhaseText.text = _battleText;
    }

    /// <summary>
    /// SetBattlePhaseDisplayText(string): Sets the text
    /// in the combat text Textbox.
    /// </summary>
    /// <param name="_combatText">String representing a combat event.</param>
    public void SetCombatText(string _combatText)
    {
        GameObject _combatTextGO = GameObject.Find("CombatText");
        Text _combatTextbox = _combatTextGO.GetComponentInChildren<Text>();
        _combatTextbox.text = _combatText;
    }

    /// <summary>
    /// HighlightOnMouseover(List<Enemy>, Material, Material):
    /// </summary>
    /// <param name="_enemyList"></param>
    /// <param name="defaultMaterial"></param>
    /// <param name="spriteOutlineMaterial"></param>
    public void HighlightOnMouseover(
        List<Enemy_FlyingEye> _enemyList,
        Material defaultMaterial,
        Material spriteOutlineMaterial
        )
    {
        foreach (Enemy_FlyingEye enemy in _enemyList)
        {
            if (!enemy.isDead)
            {
                enemy.gameObject.AddComponent<HighlightObject>();
                enemy.GetComponentInParent<HighlightObject>().defaultMaterial = defaultMaterial;
                enemy.GetComponentInParent<HighlightObject>().spriteOutlineMaterial = spriteOutlineMaterial;
            }
        }
    }

    /// <summary>
    /// SetEnemySortingLayers(List<Enemy>):
    /// </summary>
    /// <param name="_enemyList"></param>
    public void SetEnemySortingLayers(List<Enemy_FlyingEye> _enemyList)
    {
        foreach (Enemy_FlyingEye enemy in _enemyList)
        {
            if (enemy.isDead)
                enemy.GetComponent<SpriteRenderer>().sortingLayerName = "Default";
            else
                enemy.GetComponent<SpriteRenderer>().sortingLayerName = "Entities";
        }
    }

    /// <summary>
    /// RestoreDefaultMaterial(GameObject, Material):
    /// </summary>
    /// <param name="_unit"></param>
    /// <param name="defaultMaterial"></param>
    private void RestoreDefaultMaterial(GameObject _unit, Material defaultMaterial)
    {
        _unit.GetComponent<SpriteRenderer>().material = defaultMaterial;
    }

    /// <summary>
    /// SetTurnIndicationOutline(GameObject, Material):
    /// </summary>
    /// <param name="_unit"></param>
    /// <param name="spriteOutlineMaterial"></param>
    private void SetTurnIndicationOutline(GameObject _unit, Material spriteOutlineMaterial)
    {
        _unit.GetComponent<SpriteRenderer>().material = spriteOutlineMaterial;
    }
}
