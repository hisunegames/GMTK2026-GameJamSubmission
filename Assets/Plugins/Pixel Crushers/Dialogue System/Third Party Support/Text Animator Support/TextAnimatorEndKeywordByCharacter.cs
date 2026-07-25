// Copyright (c) Pixel Crushers. All rights reserved.

using System.Text.RegularExpressions;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add this to your dialogue UI if you want the sequencer {{end}} keyword to
    /// account for Text Animator typing time and typewriter types by character.
    /// </summary>
    public class TextAnimatorEndKeywordByCharacter : MonoBehaviour
    {
        
        public float waitForNormalChars = .03f;
        public float waitLong = .6f;
        public float waitMiddle = .2f;

        protected virtual void Awake()
        {
            ConversationView.overrideGetDefaultSubtitleDuration = GetTextAnimatorSubtitleDuration;
        }

        protected virtual float GetTextAnimatorSubtitleDuration(string text)
        {
            // Remove markup tags:
            var cleanText = text.Contains('<')
                ? Regex.Replace(text, @"<[^> ]+>", string.Empty)
                : text;

            int numMiddle = 0;
            int numLong = 0;
            for (int i = 0; i < cleanText.Length; i++)
            {
                char c = cleanText[i];
                var isMiddleLengthChar = c == ';' || c == ':' || c == ')' || c == '-' || c == ',';
                var isLongChar = c == '!' || c == '?' || c == '.';
                if (isMiddleLengthChar) numMiddle++;
                else if (isLongChar) numLong++;
            }
            int numNormal = cleanText.Length - (numMiddle + numLong);
            return (waitForNormalChars * numNormal) + 
                (waitMiddle * numMiddle) + 
                (waitLong * numLong);
        }

    }
}
