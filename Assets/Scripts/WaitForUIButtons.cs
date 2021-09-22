// =================================================================================================================================================================== //
// WIKI DESCRIPTION FOR: WaitForUiButtons.cs | URL: http://wiki.unity3d.com/index.php/UI/WaitForUIButtons                                                              //
// =================================================================================================================================================================== //
// This is a simple CustomYieldInstruction that allows you to wait in a coroutine for a click on one UI.Button out of a list of buttons.                               //
// This yield instruction is designed to be reused. Just call "Reset()" before you reuse it. You can pass one or multiple UI.Button instances                          //
// to the constructor and the yield instruction will automatically registers an internal callback on all those buttons.                                                //
// It also takes care of removing those callbacks once a button was pressed. The yield instruction provides the PressedButton property                                 //
// which will hold the reference to the button that was actually pressed. The yield instruction will simply wait until PressedButton is assigned.                      //
// In addition it also has a callback that is directly called from the actual button click event listener. It provides the pressed button as parameter.                //
// This feature allows this class to be used even outside a coroutine just to accumulate multiple buttons into a single callback.                                      //
// Just keep in mind that after each button click the callbacks are unregistered and you have to call Reset again.                                                     //
// The class implements the IDisposable interface. This might help to get rid of unwanted hidden references which might keep objects from being garbage collected.     //
// Note that once disposed the instance can no longer be reused. It will unregister all callbacks and clears all internal data.                                        //
// There are rare cases where this might be necessary. Even when a coroutine is terminated from outside (StopCoroutine, destroying the object, ...)                    //
// this shouldn 't be a huge issue. As soon as one of the buttons is pressed the class will unregister all callbacks from the buttons anyways.                         //
// So there should be no risk of piling up dead listeners.                                                                                                             //
// =================================================================================================================================================================== //

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class WaitForUIButtons : CustomYieldInstruction, System.IDisposable
{
    private struct ButtonCallback
    {
        public Button button;
        public UnityAction listener;
    }
    private List<ButtonCallback> m_Buttons = new List<ButtonCallback>();
    private System.Action<Button> m_Callback = null;

    public override bool keepWaiting { get { return PressedButton == null; } }
    public Button PressedButton { get; private set; } = null;

    public WaitForUIButtons(System.Action<Button> aCallback, params Button[] aButtons)
    {
        m_Callback = aCallback;
        m_Buttons.Capacity = aButtons.Length;
        foreach (var b in aButtons)
        {
            if (b == null)
                continue;
            var bc = new ButtonCallback { button = b };
            bc.listener = () => OnButtonPressed(bc.button);
            m_Buttons.Add(bc);
        }
        Reset();
    }
    public WaitForUIButtons(params Button[] aButtons) : this(null, aButtons) { }

    private void OnButtonPressed(Button button)
    {
        PressedButton = button;
        RemoveListeners();
        if (m_Callback != null)
            m_Callback(button);
    }
    private void InstallListeners()
    {
        foreach (var bc in m_Buttons)
            if (bc.button != null)
                bc.button.onClick.AddListener(bc.listener);
    }
    private void RemoveListeners()
    {
        foreach (var bc in m_Buttons)
            if (bc.button != null)
                bc.button.onClick.RemoveListener(bc.listener);
    }
    public new WaitForUIButtons Reset()
    {
        RemoveListeners();
        PressedButton = null;
        InstallListeners();
        base.Reset();
        return this;
    }
    public WaitForUIButtons ReplaceCallback(System.Action<Button> aCallback)
    {
        m_Callback = aCallback;
        return this;
    }

    public void Dispose()
    {
        RemoveListeners();
        m_Callback = null;
        m_Buttons.Clear();
    }
}