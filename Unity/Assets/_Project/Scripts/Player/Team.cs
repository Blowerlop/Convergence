namespace Project
{
    public struct Team
    {
        public UserInstance PcUser;
        public UserInstance MobileUser;
        
        public bool IsComplete => PcUser != null && MobileUser != null;
        public bool IsEmpty => PcUser == null && MobileUser == null;
    }
}