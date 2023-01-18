using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Objectives : MonoBehaviour
{
    private GameObject _main_objective;
    private GameObject[] _side_objectives;

    [SerializeField] private GameObject _main_obj_ui;

    [SerializeField] private GameObject _lockpick_side_obj_ui;
    [SerializeField] private GameObject _hack_side_obj_ui;
    [SerializeField] private GameObject _knock_side_obj_ui;
    [SerializeField] private GameObject _disg_side_obj_ui;
    

    // Start is called before the first frame update
    void Start()
    {
        
        // _main_objective = GameObject.FindGameObjectWithTag("Objective");
        _side_objectives = GameObject.FindGameObjectsWithTag("Safe");
        foreach (GameObject side_objective in _side_objectives) {
            Safe safe_obj = side_objective.GetComponent<Safe>();
            side_objective.GetComponent<Lock>().whenUnlocked.AddListener(() => OnUnlock(safe_obj));

            switch (safe_obj.abilityBonus)
            {
                case Safe.AbilityBonus.LockPicking: 
                    _lockpick_side_obj_ui.GetComponent<TextMeshPro>().text = "The Reporter's Juicy News";
                    break;
                case Safe.AbilityBonus.KnockOut:
                    _knock_side_obj_ui.GetComponent<TextMeshPro>().text = "The Cleaner's Dirty Laundry";
                    break;
                case Safe.AbilityBonus.Disguise:
                    _disg_side_obj_ui.GetComponent<TextMeshPro>().text = "The Gentleman's Blackmail";
                    break;
                case Safe.AbilityBonus.Hacking:
                    _hack_side_obj_ui.GetComponent<TextMeshPro>().text = "The Hacker's Incriminating Data";
                    break;
            }
        }
        
    }

    private void OnUnlock(Safe unlockedSafe)
    {
        switch (unlockedSafe.abilityBonus)
        {
            case Safe.AbilityBonus.LockPicking:
                // TEST TEST
                _lockpick_side_obj_ui.GetComponent<TextMeshPro>().color = Color.green;
                _lockpick_side_obj_ui.GetComponent<TextMeshPro>().text = "The Reporter's Juicy News - Reported!";
                break;
            case Safe.AbilityBonus.KnockOut:
                _knock_side_obj_ui.GetComponent<TextMeshPro>().color = Color.green;
                _knock_side_obj_ui.GetComponent<TextMeshPro>().text = "The Cleaner's Dirty Laundry - Aired Out!";
                break;
            case Safe.AbilityBonus.Disguise:
                _disg_side_obj_ui.GetComponent<TextMeshPro>().color = Color.green;
                _disg_side_obj_ui.GetComponent<TextMeshPro>().text = "The Gentleman's Blackmail - Mailed!";
                break;
            case Safe.AbilityBonus.Hacking:
                _hack_side_obj_ui.GetComponent<TextMeshPro>().color = Color.green;
                _disg_side_obj_ui.GetComponent<TextMeshPro>().text = "The Hacker's Incriminating Data - Cracked!";
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }
}
