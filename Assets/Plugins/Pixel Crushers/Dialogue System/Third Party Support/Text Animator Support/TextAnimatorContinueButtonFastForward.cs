// Copyright (c) Pixel Crushers. All rights reserved.

using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// This script replaces the normal continue button functionality with
    /// a two-stage process. If Text Animator's typewriter is still playing, it
    /// simply fast forwards to the end. Otherwise it sends OnContinue to the UI.
    /// </summary>
    public class TextAnimatorContinueButtonFastForward : MonoBehaviour
    {

        [Tooltip("Dialogue UI that the continue button affects.")]
        public StandardDialogueUI dialogueUI;

        [Tooltip("Text Animator Player to fast forward if it's not done playing.")]
        public Febucci.UI.Core.TypewriterCore textAnimatorPlayer;

        [Tooltip("Hide the continue button when continuing.")]
        public bool hideContinueButtonOnContinue = false;

        private UnityEngine.UI.Button continueButton;

        protected AbstractDialogueUI m_runtimeDialogueUI;
        protected virtual AbstractDialogueUI runtimeDialogueUI
        {
            get
            {
                if (dialogueUI != null) return dialogueUI;
                var panel = GetComponentInParent<StandardUISubtitlePanel>();
                if (panel != null) return panel.dialogueUI;
                else return GetComponentInParent<AbstractDialogueUI>() ?? DialogueManager.dialogueUI as AbstractDialogueUI;
            }
        }

        public virtual void Awake()
        {
            continueButton = GetComponent<UnityEngine.UI.Button>();
        }

        public virtual void OnFastForward()
        {
            if (textAnimatorPlayer != null && !textAnimatorPlayer.TextAnimator.allLettersShown)
            {
                textAnimatorPlayer.SkipTypewriter();
            }
            else
            {
                if (hideContinueButtonOnContinue && continueButton != null) continueButton.gameObject.SetActive(false);
                if (runtimeDialogueUI != null) runtimeDialogueUI.OnContinue();
            }
        }

    }
}
