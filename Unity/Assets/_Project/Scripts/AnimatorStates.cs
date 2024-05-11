using System.Collections.Generic;
using Project.Extensions;
using UnityEngine;

namespace Project
{
    public static class AnimatorStates
    {
        public static Dictionary<int, int> grpcHash = new Dictionary<int, int>
        {
            [UnityAnimatorStringToHash("Idle")] = "Idle".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Movement")] = "Movement".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("AutoAttack")] = "AutoAttack".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Channeling 1")] = "Channeling 1".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Channeling 2")] = "Channeling 2".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Channeling 3")] = "Channeling 3".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Channeling 4")] = "Channeling 4".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Cast1")] = "Cast1".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Cast2")] = "Cast2".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Cast3")] = "Cast3".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Cast4")] = "Cast4".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Death")] = "Death".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Emote")] = "Emote".ToHashIsSameAlgoOnUnreal(),
            [UnityAnimatorStringToHash("Stun")] = "Stun".ToHashIsSameAlgoOnUnreal(),
        };


        private static int UnityAnimatorStringToHash(string str)
        {
            return Animator.StringToHash(str);
        }
    }
}
