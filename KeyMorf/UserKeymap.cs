namespace KeyMorf
{
    public class UserKeymap
    {
        public Keys ToggleKey { get; }

        public int ToggleTimeMs { get; }

        public UserMapping[] Mappings { get; }

        public UserKeymap(Keys toggleKey, int toggleTimeMs, UserMapping[] mappings)
        {
            ToggleKey = toggleKey;
            ToggleTimeMs = toggleTimeMs;
            Mappings = mappings;
        }

        public class UserMapping
        {
            public Keys From { get; }

            public Keys To { get; }

            public Keys[]? Mods { get; }

            public UserMapping(Keys from, Keys to, Keys[]? mods)
            {
                From = from;
                To = to;
                Mods = mods;
            }
        }
    }
}
