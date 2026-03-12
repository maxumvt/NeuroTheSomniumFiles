namespace NeuroSomniumFiles;

using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // if using TextMeshProUGUI

[BepInPlugin("com.yourname.dialoglogger", "NeuroSomniumFiles", "1.0.0")]
public class TextExtractor : BaseUnityPlugin
{
    float searchTimer = 0f;
    bool searchAllowed = true;

    public RawImage characterNamePlate;
    public TextMeshProUGUI characterDialogue;
    public TextMeshProUGUI descriptionDialogue;
    
    public GameObject lookChoices = null;
    public bool interactLook = false; // This one is gonna be important for Neuro to make choices and to let her know that she can't make a choice
    public string interactName = "";


    public string dialogueLastline = "";
    public string descriptionLastline = "";
    public string scene_type = "";

    void Awake()
    {
        Logger.LogInfo("NeuroSomniumFiles started");
    }

    void Update()
    {
        searchTimer += Time.deltaTime;
        if ( searchTimer > 1f) { searchAllowed = true; searchTimer = 0f; }
        else searchAllowed = false;

        CharacterSpeaking(searchAllowed);
        DescriptionText(searchAllowed);
        LookChoicesOptions(searchAllowed);
    }

    // SECTION: Extractors
    void CharacterSpeaking(bool allowSearch)
    {
        if (allowSearch && characterNamePlate == null) {  characterNamePlate = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/MessageWindow/Rig/Name/Text")?.GetComponent<RawImage>();    }
        if (allowSearch && characterDialogue == null) {  characterDialogue = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/MessageWindow/Rig/Background/Text")?.GetComponent<TextMeshProUGUI>();   }

        if (characterNamePlate != null && characterDialogue != null)
        {
            string nameText = characterNamePlate.mainTexture.name;
            string dialogueText = characterDialogue.text;

            if (!string.IsNullOrEmpty(dialogueText) && dialogueLastline != dialogueText)
            {
                EmitTextChange($"[{nameText}]: {dialogueText}");
                dialogueLastline = dialogueText;
            }
        }
    }
    void DescriptionText(bool allowSearch)
    {
        if (allowSearch && descriptionDialogue == null) {  descriptionDialogue = GameObject.Find("$Root/UICanvas/ScreenScaler/UIOff1/PanelNode/NarrationWindow/GameObject/Background/Text")?.GetComponent<TextMeshProUGUI>(); }
        if (descriptionDialogue != null)
        {
            string descrText = descriptionDialogue.text;
            if (!string.IsNullOrEmpty(descrText) && descrText != descriptionLastline)
            {
                EmitTextChange($"[Description]: {descrText}");
                descriptionLastline = descrText;
            }
        }
    }
    void LookChoicesOptions(bool allowSearch)
    {
        if (lookChoices == null) 
        {
            if (!allowSearch) return;

            lookChoices = GameObject.Find("$Root/CommandCanvas/ScreenScaler/Command/Scale");
            if (lookChoices == null) return;
        }

        // code that depends on lookChoices
        bool look = lookChoices.transform.Find("Look").gameObject.activeSelf;
        
        if (interactLook != look)
        {
            string options;
            if (look) options = $"[Options]: Look at description";
            else options = "";
            interactLook = look;

            bool buttonUp = lookChoices.transform.Find("SelectU").gameObject.activeSelf;
            if (buttonUp)
            {
                string buttonUpText = lookChoices.transform.Find("SelectU/Background/Text")?.GetComponent<TextMeshProUGUI>().text;
                options = options + $", Button Up option: {buttonUpText}";
            }

            // Check active buttons
            // if active add option

            EmitTextChange($"look active: {look}");
            EmitTextChange(options);
        
        }

    }

    // SECTION: Communication
    void EmitTextChange(string text)
    {
        Logger.LogInfo(text);
    }
}
