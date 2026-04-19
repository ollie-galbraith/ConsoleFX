using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ConsoleFX
    {
        public class BorderStyle
            {
                [JsonPropertyName("top_left")]          public char TopLeft          { get; set; }
                [JsonPropertyName("top_right")]         public char TopRight         { get; set; }
                [JsonPropertyName("bottom_left")]       public char BottomLeft       { get; set; }
                [JsonPropertyName("bottom_right")]      public char BottomRight      { get; set; }
                [JsonPropertyName("horizontal")]        public char Horizontal       { get; set; }
                [JsonPropertyName("vertical")]          public char Vertical         { get; set; }
                [JsonPropertyName("vertical_left")]     public char VerticalLeft     { get; set; }
                [JsonPropertyName("vertical_right")]    public char VerticalRight    { get; set; }
                [JsonPropertyName("horizontal_top")]    public char HorizontalTop    { get; set; }
                [JsonPropertyName("horizontal_bottom")] public char HorizontalBottom { get; set; }
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
                        _styles = JsonSerializer.Deserialize<Dictionary<string, BorderStyle>>(json);
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
