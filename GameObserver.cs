namespace NeuroSomniumFiles;

using BepInEx;
using UnityEngine;
using UnityEngine.UI;
using TMPro; // if using TextMeshProUGUI


[BepInPlugin("com.yourname.dialoglogger", "NeuroSomniumFiles", "1.0.0")]
public class GameObserver : BaseUnityPlugin
{
    float searchTimer = 0f;
    bool searchAllowed = true;
    public 

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
    }

    // SECTION: Extractors
    void CharacterSpeaking(bool allowSearch)
    {
        // 
    }
    void DescriptionText(bool allowSearch)
    {
        
    }

}
