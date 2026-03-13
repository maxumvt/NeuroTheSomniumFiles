namespace NeuroSomniumFiles;

using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // if using TextMeshProUGUI

using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;

[BepInPlugin("com.yourname.dialoglogger", "NeuroSomniumFiles", "1.0.0")]
public class TextExtractor : BaseUnityPlugin
{
    private NeuroSender sender;

    float searchTimer = 0f;
    bool searchAllowed = true;

    public RawImage characterNamePlate;
    public TextMeshProUGUI characterDialogue;
    public TextMeshProUGUI descriptionDialogue;
    
    public GameObject lookChoices = null;
    public bool interactLook = false; // NOTE This one is gonna be important for Neuro to make choices and to let her know that she can't make a choice
    public string interactName = "";


    public bool descriptionShow = false;
    public string dialogueLastline = "";
    public string descriptionLastline = "";
    public string scene_type = "";

    void Awake()
    {
        Logger.LogInfo("NeuroSomniumFiles started");
        sender = new NeuroSender();
        sender.Connect();
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
            // ERROR Clicking description type windows with one line only, can cause the description not to be logged again. This is bad feedback and needs some kind of solution
            if (!string.IsNullOrEmpty(descrText) && (descrText != descriptionLastline || descriptionShow))
            {
                EmitTextChange($"[Description]: {descrText}");
                descriptionLastline = descrText;
                descriptionShow = false;
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
            interactLook = look;
            string term = lookChoices.transform.Find("Term/Background/Text")?.GetComponent<TextMeshProUGUI>().text; // IMPROVEMENT This object should be more global, so that the description can also use Terms
            EmitTextChange($"look active: {look} on {term}");
            if (look) options = $"[Options]: Look at description";
            else return;

            bool buttonUp = lookChoices.transform.Find("SelectU").gameObject.activeSelf;
            if (buttonUp)
            {
                string buttonUpText = lookChoices.transform.Find("SelectU/Background/Text")?.GetComponent<TextMeshProUGUI>().text;
                options = options + $", Button Up option: {buttonUpText}";
            }

            bool buttonDown = lookChoices.transform.Find("SelectD").gameObject.activeSelf;
            if (buttonDown)
            {
                string buttonDownText = lookChoices.transform.Find("SelectD/Background/Text")?.GetComponent<TextMeshProUGUI>().text;
                options = options + $", Button Down option: {buttonDownText}";
            }

            bool buttonLeft = lookChoices.transform.Find("SelectL").gameObject.activeSelf;
            if (buttonLeft)
            {
                string buttonLeftText = lookChoices.transform.Find("SelectL/Background/Text")?.GetComponent<TextMeshProUGUI>().text;
                options = options + $", Button Left option: {buttonLeftText}";
            }

            bool buttonRight = lookChoices.transform.Find("SelectR").gameObject.activeSelf;
            if (buttonRight)
            {
                string buttonRightText = lookChoices.transform.Find("SelectR/Background/Text")?.GetComponent<TextMeshProUGUI>().text;
                options = options + $", Button Right option: {buttonRightText}";
            }

            bool buttonZoom = lookChoices.transform.Find("Zoom").gameObject.activeSelf;
            if (buttonZoom)
            {
                options += $", Zoom possible";
            }
            
            bool buttonThermo = lookChoices.transform.Find("Thermo").gameObject.activeSelf;
            if (buttonThermo)
            {
                options += $", Thermo possible";
            }
            
            bool buttonXray = lookChoices.transform.Find("XRay").gameObject.activeSelf;
            if (buttonXray)
            {
                options += $", XRay possible";
            }
            
            bool buttonNV = lookChoices.transform.Find("NV").gameObject.activeSelf; // NOTE NV is Night Vision
            if (buttonNV)
            {
                options += $", NV possible";
            }
            
            bool buttonZoomThermo = lookChoices.transform.Find("ZoomThermo").gameObject.activeSelf;
            if (buttonZoomThermo)
            {
                options += $", Zoom Thermo possible";
            }
            
            bool buttonZoomXray = lookChoices.transform.Find("ZoomXRay").gameObject.activeSelf;
            if (buttonZoomXray)
            {
                options += $", Zoom XRay possible";
            }
            
            bool buttonZoomNV = lookChoices.transform.Find("ZoomNV").gameObject.activeSelf;
            if (buttonZoomNV)
            {
                options += $", Zoom NV possible";
            }


            EmitTextChange(options);
        
        }

    }

    // SECTION: Communication
    void EmitTextChange(string text)
    {
        Logger.LogInfo(text);
        string json = "{\"text\": \"" + text.Replace("\"", "\\\"") + "\"}";
        sender.SendString(json);
    }
}
