using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace ConsoleFX
    {
        public class BorderStyle
            {
                [JsonProperty("top_left")]          public char TopLeft          { get; set; }
                [JsonProperty("top_right")]         public char TopRight         { get; set; }
                [JsonProperty("bottom_left")]       public char BottomLeft       { get; set; }
                [JsonProperty("bottom_right")]      public char BottomRight      { get; set; }
                [JsonProperty("horizontal")]        public char Horizontal       { get; set; }
                [JsonProperty("vertical")]          public char Vertical         { get; set; }
                [JsonProperty("vertical_left")]     public char VerticalLeft     { get; set; }
                [JsonProperty("vertical_right")]    public char VerticalRight    { get; set; }
                [JsonProperty("horizontal_top")]    public char HorizontalTop    { get; set; }
                [JsonProperty("horizontal_bottom")] public char HorizontalBottom { get; set; }
            }

        public class BorderCollection
            {
                public Dictionary<string, BorderStyle> Styles { get; set; } = new();
            }

        public class BorderManager
            {
                private Dictionary<string, BorderStyle>? _styles;
                public void LoadJson(string json)
                    {
                        _styles = JsonConvert.DeserializeObject<Dictionary<string, BorderStyle>>(json) ?? throw new InvalidOperationException("Failed to deserialize border styles from JSON.");
                    }

                public void LoadJsonFromFile(string filePath)
                    {
                        string json = System.IO.File.ReadAllText(filePath);
                        LoadJson(json);
                    }

                public void LoadJsonFromStream(System.IO.Stream stream)
                    {
                        using var reader = new System.IO.StreamReader(stream);
                        string json = reader.ReadToEnd();
                        LoadJson(json);
                    }

                public BorderStyle? GetStyle(string name)
                    {
                        if (_styles != null && _styles.TryGetValue(name, out var style))
                            {
                                return style;
                            }
                        return null;
                    }
            }
    }
