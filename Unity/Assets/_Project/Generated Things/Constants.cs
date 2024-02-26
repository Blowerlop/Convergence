namespace Project
{
    public static class Constants
    {
        public static class Scenes
        {
            /// <summary>
            /// Lobby
            /// </summary>
            public const int Lobby = 0;
            /// <summary>
            /// Menu
            /// </summary>
            public const int Menu = 1;
            /// <summary>
            /// gRPC_HelloWorld_Sample
            /// </summary>
            public const int gRPC_HelloWorld_Sample = 2;
            /// <summary>
            /// NetworkingSample
            /// </summary>
            public const int NetworkingSample = 3;
            /// <summary>
            /// Game
            /// </summary>
            public const int Game = 4;
            /// <summary>
            /// Spell
            /// </summary>
            public const int Spell = 5;
            /// <summary>
            /// Bootstrap
            /// </summary>
            public const int Bootstrap = 6;
        }
        
        public static class AudioMixerParams
        {
            /// <summary>
            /// GameSounds
            /// </summary>
            public const string GameSounds = "GameSounds";
            /// <summary>
            /// Master
            /// </summary>
            public const string Master = "Master";
            /// <summary>
            /// Music
            /// </summary>
            public const string Music = "Music";
        }
        
        public static class AnimatorsParam
        {
            /// <summary>
            /// Movement : Bool
            /// </summary>
            public const int Movement = 229373857;
        }
        
        public static class LayersMask
        {
            /// <summary>
            /// Default
            /// </summary>
            public const int Default = 1;
            /// <summary>
            /// TransparentFX
            /// </summary>
            public const int TransparentFX = 2;
            /// <summary>
            /// Ignore Raycast
            /// </summary>
            public const int Ignore_Raycast = 4;
            /// <summary>
            /// Water
            /// </summary>
            public const int Water = 16;
            /// <summary>
            /// UI
            /// </summary>
            public const int UI = 32;
            /// <summary>
            /// Damageable
            /// </summary>
            public const int Damageable = 64;
            /// <summary>
            /// Ground
            /// </summary>
            public const int Ground = 128;
        }
        
        public static class Resources
        {
            public static class NetworkObjects
            {
                /// <summary>
                /// NetworkObjects/cube
                /// </summary>
                public const string cube = "NetworkObjects/cube";
                /// <summary>
                /// NetworkObjects/FU_UserInstance
                /// </summary>
                public const string FU_UserInstance = "NetworkObjects/FU_UserInstance";
                /// <summary>
                /// NetworkObjects/netobj_test
                /// </summary>
                public const string netobj_test = "NetworkObjects/netobj_test";
            }
            
            /// <summary>
            /// AudioMixer
            /// </summary>
            public const string AudioMixer = "AudioMixer";
            /// <summary>
            /// Bootstrap
            /// </summary>
            public const string Bootstrap = "Bootstrap";
            /// <summary>
            /// DOTweenSettings
            /// </summary>
            public const string DOTweenSettings = "DOTweenSettings";
            /// <summary>
            /// SO References Cache
            /// </summary>
            public const string SO_References_Cache = "SO References Cache";
        }
        
        public static class Tags
        {
            /// <summary>
            /// Untagged
            /// </summary>
            public const string Untagged = "Untagged";
            /// <summary>
            /// Respawn
            /// </summary>
            public const string Respawn = "Respawn";
            /// <summary>
            /// Finish
            /// </summary>
            public const string Finish = "Finish";
            /// <summary>
            /// EditorOnly
            /// </summary>
            public const string EditorOnly = "EditorOnly";
            /// <summary>
            /// MainCamera
            /// </summary>
            public const string MainCamera = "MainCamera";
            /// <summary>
            /// Player
            /// </summary>
            public const string Player = "Player";
            /// <summary>
            /// GameController
            /// </summary>
            public const string GameController = "GameController";
        }
        
    }
}
