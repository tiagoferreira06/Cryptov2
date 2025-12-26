using System.Xml.Serialization;

namespace CryptoPlatform.Domain.Entities;

[XmlRoot("WatchlistItem")]
public class WatchlistItem
{
    [XmlElement("Id")]
    public Guid Id { get; set; }

    [XmlElement("UserId")]
    public Guid UserId { get; set; }

    [XmlElement("CryptoId")]
    public string CryptoId { get; set; } = null!;

    [XmlElement("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
