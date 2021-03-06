﻿using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public class GameManager : MonoBehaviour {
    public bool lose;
    public int numEnemiesKilled = 0;
    GameObject endText;

    public AudioClip victory_sound;
    public AudioClip fail_sound;

    public GameObject SpellUI_prefab;
    public Transform headset_trans;

    private GameObject current_UI;
    private string current_cast_string;
    public int level = 1;

    public SpellManager spellManager;
    bool aimMode = false;

	private UnityAction someListener;

    public float mana;
    public Material manaSphere;
    public bool mana_charging_on = false;

    public TowerHealthManager tower;

    public float score = 0;

    public GameObject lightning_bolt_prefab;
    public Vector3 spell_hit_pos;

	// Use this for initialization
	void Start () {
        mana = 100;
        current_cast_string = "";
        spellManager = GameObject.Find("SpellManager").GetComponent<SpellManager>();
        endText = GameObject.Find("EndText");
        lose = false;
        manaSphere = GameObject.Find("ManaSphere").GetComponent<Renderer>().material;
        //tower = GameObject.Find("Tower").GetComponent<TowerHealthManager>();
	}

    public void LoseGame()
    {
        endText.GetComponent<TextMesh>().text = "You lose! <}:>( >\nScore: " + numEnemiesKilled * 10;
        endText.GetComponent<AudioSource>().clip = fail_sound;
        endText.GetComponent<AudioSource>().Play();
        endText.GetComponent<AudioSource>().loop = false;
        lose = true;
    }

    // Update is called once per frame
    void Update () {
        if (!lose)
        {
            int numLivesLost = (100 - tower.towerHealth) / 10;
            if (numEnemiesKilled + numLivesLost == GetComponent<SpawnEnemies>().num_enemies)
            {
                endText.GetComponent<TextMesh>().text = "You win! <}:>D >\nScore: " + numEnemiesKilled * 10;
                endText.GetComponent<AudioSource>().clip = victory_sound;
                endText.GetComponent<AudioSource>().Play();
                endText.GetComponent<AudioSource>().loop = false;
                numEnemiesKilled = 0;
                lose = true;
            }
        }
	}

	void Awake ()
	{
	}

	void OnEnable ()
	{
		EventManager.StartListening ("towerTakeDamage", DamageTower);
	}

	void OnDisable ()
	{
		EventManager.StopListening ("towerTakeDamage", DamageTower);
	}

	void DamageTower ()
	{
		//Debug.Log ("Some Function was called!");
	}

    public void StartAimMode()
    {
        aimMode = true;
        // Aiming
        //Debug.Log("Aim mode enabled");
        EventManager.TriggerEvent("AimModeEnable");
        current_cast_string = "";
        Destroy(current_UI);
        EventManager.TriggerEvent("DisableTrail");
        //START POINTER

    }

    public void TriggerSpellUI(Transform trans)
    {
            Debug.Log("trigger spell ui");
            // Instantiate spell ui
            Quaternion spawn_rot = Quaternion.LookRotation(new Vector3(0, 3.8f, 0));
            current_UI = (GameObject)Instantiate(SpellUI_prefab, trans.position, headset_trans.rotation);
        EventManager.TriggerEvent("EnableTrail");
        Debug.Log(name);
    }

    public void UntriggerSpellUI()
    {
        if (aimMode)
        {
            //STOP POINTER
            //FIRE
            Debug.Log("Fire Spell: " + spellManager.currElement);
            EventManager.TriggerEvent("AimModeDisable");
            spellManager.currElement = SpellManager.Element.None;
            aimMode = false;
        }
        else
        {
            EventManager.TriggerEvent("DisableTrail");
            Destroy(current_UI);
            current_cast_string = "";
        }
    }


    public void InstanceSpellHit(Vector3 hit_point)
    {
        Vector3 spawn_pos = hit_point;
        //Quaternion spawn_rot = Quaternion.LookRotation(-(spawn_pos));
        Quaternion spawn_rot = spellManager.getCurrentSpellRotation();
        GameObject spell_prefab = spellManager.getCurrentSpellEffect();
        GameObject spell = (GameObject) Instantiate(spell_prefab, spawn_pos, spawn_rot);
        spell.GetComponent<SpellHitEffect>().type = spellManager.currElement;
        if (spellManager.currElement == SpellManager.Element.Lightning)
        {
            GameObject temp = (GameObject)Instantiate(lightning_bolt_prefab, spawn_pos, spawn_rot);
            temp.transform.parent = spell.transform;
            spell_hit_pos = spawn_pos;

        }
        spell.transform.localScale = spellManager.getCurrentSpellRadius();

    }

    public void RegisterCastPoint(string name)
    {
        if ( name.CompareTo("CastPoint (1)") == 0)
        {
            current_cast_string += "A";
        }
        else if (name.CompareTo("CastPoint (2)") == 0)
        {
            current_cast_string += "B";
        }
        else if (name.CompareTo("CastPoint (3)") == 0)
        {
            current_cast_string += "C";
        }
        else if (name.CompareTo("CastPoint (4)") == 0)
        {
            current_cast_string += "D";
        }
        EventManager.TriggerEvent("HapticPing");
        if (spellManager.loadSpell(current_cast_string))
        {
            StartAimMode();
            UpdateManaUI();
        }
        Debug.Log(current_cast_string);
    }

    public void UpdateManaUI()
    {
        float offset = 0.2f - (mana / 100.0f) * 0.4f;
        GameObject.Find("ManaSphere").GetComponent<Renderer>().material.SetTextureOffset("_MainTex", new Vector2(0, offset));
    }
}
