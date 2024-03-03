using System.Collections.Generic;
using Project.Extensions;
using UnityEngine;

namespace Project
{
    public static class AnimatorStates
    {
        public static Dictionary<int, int> grpcHash = new Dictionary<int, int>
        {
            [UnityAnimatorStringToHash("Idle1")] = "Idle1".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Idle2")] = "Idle2".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Idle3")] = "Idle3".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Idle4")] = "Idle4".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Movement")] = "Movement".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Channeling")] = "Channeling".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Emote1")] = "Emote1".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Emote2")] = "Emote2".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Emote3")] = "Emote3".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Emote4")] = "Emote4".ToHashIsSameAlgoOnUnreal()
        };


        private static int UnityAnimatorStringToHash(string str)
        {
            return Animator.StringToHash(str);
        }
    }
}
