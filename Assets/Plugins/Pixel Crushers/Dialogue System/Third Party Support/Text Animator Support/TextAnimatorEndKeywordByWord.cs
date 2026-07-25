// Copyright (c) Pixel Crushers. All rights reserved.

using System.Text.RegularExpressions;
using UnityEngine;

namespace PixelCrushers.DialogueSystem
{

    /// <summary>
    /// Add this to your dialogue UI if you want the sequencer {{end}} keyword to
    /// account for Text Animator typing time and typewriter types by word.
    /// </summary>
    public class TextAnimatorEndKeywordByWord : MonoBehaviour
    {

        public float waitForNormalWord = 0.3f;
        public float waitForWordWithPunctuation = 0.5f;

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

            int numNormalWords = 0;
            int numPunctuationWords = 0;
            char prevChar = ' ';
            for (int i = 0; i < cleanText.Length; i++)
            {
                char c = cleanText[i];
                var isEndOfNormalWord = c == ' ' && prevChar != ' ';
                var isEndOfPunctuationWord = char.IsPunctuation(c) && char.IsLetterOrDigit(prevChar);
                if (isEndOfNormalWord) numNormalWords++;
                else if (isEndOfPunctuationWord) numPunctuationWords++;
                prevChar = c;
            }
            return
                (waitForNormalWord * numNormalWords) +
                (waitForWordWithPunctuation * numPunctuationWords);
        }

    }
}
