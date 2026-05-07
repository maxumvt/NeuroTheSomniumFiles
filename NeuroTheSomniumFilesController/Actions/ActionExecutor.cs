namespace NeuroTheSomniumFiles;

using UnityEngine;
using System.Collections;

public class ActionExecutor
{
    public void ExecuteAction(string action_name)
    {
        switch (action_name)
        {
            case "button_up_0":
                PressTimieSequence("BUTTON_WASD_DPAD_UP", 0);
                break;
            case "button_up_1":
                PressTimieSequence("BUTTON_WASD_DPAD_UP", 1);
                break;
            case "button_up_2":
                PressTimieSequence("BUTTON_WASD_DPAD_UP", 2);
                break;
            case "button_up":
                PressButton("BUTTON_WASD_DPAD_UP");
                break;
            case "button_down":
                PressButton("BUTTON_WASD_DPAD_DOWN");
                break;
            case "button_left_0":
                PressTimieSequence("BUTTON_WASD_DPAD_LEFT", 0);
                break;
            case "button_left_1":
                PressTimieSequence("BUTTON_WASD_DPAD_LEFT", 1);
                break;
            case "button_left_2":
                PressTimieSequence("BUTTON_WASD_DPAD_LEFT", 2);
                break;
            case "button_left":
                PressButton("BUTTON_WASD_DPAD_LEFT");
                break;
            case "button_right_0":
                PressTimieSequence("BUTTON_WASD_DPAD_RIGHT", 0);
                break;
            case "button_right_1":
                PressTimieSequence("BUTTON_WASD_DPAD_RIGHT", 1);
                break;
            case "button_right_2":
                PressTimieSequence("BUTTON_WASD_DPAD_RIGHT", 2);
                break;
            case "button_right":
                PressButton("BUTTON_WASD_DPAD_RIGHT");
                break;
            case "look_at_term":
                PressButton("Submit");
                break;
            case "zoom_button_l":
                PressButton("BUTTON_LSTICK");
                break;
            case "thermo_button_l":
                PressButton("BUTTON_LSTICK");
                break;
            case "night_vision_button_l":
                PressButton("BUTTON_LSTICK");
                break;
            case "night_vision_button_r":
                PressButton("BUTTON_RSTICK");
                break;
            case "xray_button_l":
                PressButton("BUTTON_LSTICK");
                break;
            case "xray_button_r":
                PressButton("BUTTON_RSTICK");
                break;
            case "zoom_thermo_button_l":
                PressButton("BUTTON_LSTICK");
                break;
            case "zoom_xray_button_l":
                PressButton("BUTTON_LSTICK"); 
                break;
            case "zoom_xray_button_r":
                PressButton("BUTTON_RSTICK"); 
                break;
            case "zoom_night_vision_button_l":
                PressButton("BUTTON_LSTICK");
                break;
            case "zoom_night_vision_button_r":
                PressButton("BUTTON_RSTICK");
                break;
            
        }
    }

    private void PressButton(string buttonKey)
    {
        Component inputProc = GameObject.Find("$Root/GameController").GetComponent("InputProc"); // Gets InputProc
        var pad_states = (System.Collections.IDictionary)inputProc.GetType().GetField("padstates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(inputProc); // Gets buttons with their state

        var button_obj = pad_states[buttonKey]; // Retrieve the current key's state
        var button_obj_down = button_obj.GetType().GetField("down", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public); // Get the "down" field of the key's state
        button_obj_down.SetValue(button_obj, true); // Set the "down" state of the button to true
    }

    private void PressTimieSequence(string buttonKey, int position)
    {
        CoroutineRunner.Run(PressOnDelay(buttonKey, position));
    }
    
    private IEnumerator PressOnDelay(string buttonKey, int position)
    {
        // navigate to position
        position += 1;
        for (int i = 0; i < position; i++)
        {
            PressButton("R1");
        }
        // select button
        PressButton("Submit");

        yield return new WaitForSeconds(1.5f);
        Component inputProc = GameObject.Find("$Root/GameController").GetComponent("InputProc"); // Gets InputProc
        var pad_states = (System.Collections.IDictionary)inputProc.GetType().GetField("padstates", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic).GetValue(inputProc); // Gets buttons with their state

        var button_obj = pad_states[buttonKey]; // Retrieve the current key's state
        var button_obj_down = button_obj.GetType().GetField("down", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public); // Get the "down" field of the key's state
        button_obj_down.SetValue(button_obj, true); // Set the "down" state of the button to true
    }
}
