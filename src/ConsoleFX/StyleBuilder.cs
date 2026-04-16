namespace ConsoleFX
    {
        public class Styled
            {
                public static string Apply(string content, params string[] codes)
                    {
                        return $"{string.Join("",codes)}{content}{ConsoleFX.Format.Reset}";
                    }
            }
    }