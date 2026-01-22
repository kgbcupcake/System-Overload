using System.Text.Json.Serialization;

namespace Src.GameData.Components
{
    public class AttachmentData
    {
        public string Name { get; set; } = "New Attachment";
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public AttachmentSlot Slot { get; set; } = AttachmentSlot.None;
        public float DamageMod { get; set; } = 1.0f; // Multiplier
        public float SpeedMod { get; set; } = 1.0f; // Multiplier

        public AttachmentData() { }
    }
}
