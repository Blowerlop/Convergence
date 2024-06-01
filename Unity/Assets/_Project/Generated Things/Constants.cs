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
            /// <summary>
            /// ARPGDemo03
            /// </summary>
            public const int ARPGDemo03 = 7;
            /// <summary>
            /// ARPGDemo01
            /// </summary>
            public const int ARPGDemo01 = 8;
            /// <summary>
            /// ARPGDemo02
            /// </summary>
            public const int ARPGDemo02 = 9;
            /// <summary>
            /// ARPGDemo04
            /// </summary>
            public const int ARPGDemo04 = 10;
            /// <summary>
            /// ARPGDemo05
            /// </summary>
            public const int ARPGDemo05 = 11;
            /// <summary>
            /// ARPGDemo06
            /// </summary>
            public const int ARPGDemo06 = 12;
            /// <summary>
            /// ARPGDemo08
            /// </summary>
            public const int ARPGDemo08 = 13;
            /// <summary>
            /// ARPGDemo09
            /// </summary>
            public const int ARPGDemo09 = 14;
            /// <summary>
            /// ARPGDemo10
            /// </summary>
            public const int ARPGDemo10 = 15;
            /// <summary>
            /// ARPGDemo11
            /// </summary>
            public const int ARPGDemo11 = 16;
            /// <summary>
            /// ARPGDemo12
            /// </summary>
            public const int ARPGDemo12 = 17;
            /// <summary>
            /// ARPGDemo13
            /// </summary>
            public const int ARPGDemo13 = 18;
            /// <summary>
            /// ARPGDemo07
            /// </summary>
            public const int ARPGDemo07 = 19;
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
            /// Idle : Trigger
            /// </summary>
            public const int Idle = 2081823275;
            /// <summary>
            /// Run : Trigger
            /// </summary>
            public const int Run = 1748754976;
            /// <summary>
            /// AutoAttack : Trigger
            /// </summary>
            public const int AutoAttack = -1936938822;
            /// <summary>
            /// Death : Trigger
            /// </summary>
            public const int Death = 646380074;
            /// <summary>
            /// Victory : Trigger
            /// </summary>
            public const int Victory = -1090111034;
            /// <summary>
            /// Cast1 : Trigger
            /// </summary>
            public const int Cast1 = 382807843;
            /// <summary>
            /// Cast2 : Trigger
            /// </summary>
            public const int Cast2 = -1881637223;
            /// <summary>
            /// Cast3 : Trigger
            /// </summary>
            public const int Cast3 = -119583217;
            /// <summary>
            /// Cast4 : Trigger
            /// </summary>
            public const int Cast4 = 1723587500;
            /// <summary>
            /// Movement : Bool
            /// </summary>
            public const int Movement = 229373857;
            /// <summary>
            /// Attack : Trigger
            /// </summary>
            public const int Attack = 1080829965;
            /// <summary>
            /// AttackSpeed : Float
            /// </summary>
            public const int AttackSpeed = -498733101;
            /// <summary>
            /// EndAttackInstant : Trigger
            /// </summary>
            public const int EndAttackInstant = 17244484;
            /// <summary>
            /// Stunned : Bool
            /// </summary>
            public const int Stunned = -449490811;
            /// <summary>
            /// Channeling 1 : Bool
            /// </summary>
            public const int Channeling_1 = -646212687;
            /// <summary>
            /// Channeling 2 : Bool
            /// </summary>
            public const int Channeling_2 = 1081262603;
            /// <summary>
            /// Channeling 3 : Bool
            /// </summary>
            public const int Channeling_3 = 930476701;
            /// <summary>
            /// Channeling 4 : Bool
            /// </summary>
            public const int Channeling_4 = -1458477250;
            /// <summary>
            /// Cast 1 : Bool
            /// </summary>
            public const int Cast_1 = 1920232698;
            /// <summary>
            /// Cast 2 : Bool
            /// </summary>
            public const int Cast_2 = -344113856;
            /// <summary>
            /// Cast 3 : Bool
            /// </summary>
            public const int Cast_3 = -1669722666;
            /// <summary>
            /// Cast 4 : Bool
            /// </summary>
            public const int Cast_4 = 35559541;
            /// <summary>
            /// Dead : Bool
            /// </summary>
            public const int Dead = 1293411866;
            /// <summary>
            /// EmoteIndex : Int
            /// </summary>
            public const int EmoteIndex = -126835265;
        }
        
        public static class Layers
        {
            /// <summary>
            /// Default
            /// </summary>
            public const int DefaultMask = 1;
            public const int DefaultIndex = 0;
            public const string DefaultName = "Default";
            /// <summary>
            /// TransparentFX
            /// </summary>
            public const int TransparentFXMask = 2;
            public const int TransparentFXIndex = 1;
            public const string TransparentFXName = "TransparentFX";
            /// <summary>
            /// Ignore Raycast
            /// </summary>
            public const int Ignore_RaycastMask = 4;
            public const int Ignore_RaycastIndex = 2;
            public const string Ignore_RaycastName = "Ignore Raycast";
            /// <summary>
            /// Water
            /// </summary>
            public const int WaterMask = 16;
            public const int WaterIndex = 4;
            public const string WaterName = "Water";
            /// <summary>
            /// UI
            /// </summary>
            public const int UIMask = 32;
            public const int UIIndex = 5;
            public const string UIName = "UI";
            /// <summary>
            /// Entity
            /// </summary>
            public const int EntityMask = 64;
            public const int EntityIndex = 6;
            public const string EntityName = "Entity";
            /// <summary>
            /// Ground
            /// </summary>
            public const int GroundMask = 128;
            public const int GroundIndex = 7;
            public const string GroundName = "Ground";
            /// <summary>
            /// WorldUI
            /// </summary>
            public const int WorldUIMask = 256;
            public const int WorldUIIndex = 8;
            public const string WorldUIName = "WorldUI";
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
            /// dummy
            /// </summary>
            public const string dummy = "dummy";
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
            /// <summary>
            /// Border
            /// </summary>
            public const string Border = "Border";
        }
        
    }
}
