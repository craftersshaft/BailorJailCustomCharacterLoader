using UnityEngine;
using BepInEx.IL2CPP.Utils.Collections;
using Il2CppSystem.Collections.Generic;
using System.Linq;

namespace BailCustomChars.Data
{
    public class CustomCharacter
    {
        public int id { get; set; }
		public int textId { get; set; }
		public int modelId { get; set; }

        public int animId { get; set; }
        public int cloneId { get; set; }
        public int cloneModelId { get; set; }
        public int cloneAnimId { get; set; }

        public int ghostExclusiveLoopSE { get; set; } //adding this because something wants to yell at me

        public int emotionId { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string iconName { get; set; }
        public string matchingIconName { get; set; }
        public string resultIconName { get; set; }
        public string announceIconPath { get; set; }
        public string ingameIconName { get; set; }
        public string ingameIconName8p { get; set; }

        public bool replaceBones { get; set; }

        public string animatorPath { get; set; }

//public string smallPortraitPath;
//private Sprite _smallPortrait;
//public Sprite SmallPortrait => _smallPortrait ??= LoadSpriteFromZip(smallPortraitPath);

        public bool isHuman { get; set; }

        public bool shouldCopyMaterialFromClone { get; set; }
        public int[] modelIdArray { get; set; }
        public string[] newModelsPaths { get; set; }

        public string[] animReplaceNames { get; set; }
        public string[] animReplacePaths { get; set; }
    }
}