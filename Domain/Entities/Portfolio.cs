using System.Xml.Serialization;

namespace CryptoPlatform.Domain.Entities;

[XmlRoot("Portfolio")]
public class Portfolio
{
    [XmlElement("Id")]
    public Guid Id { get; set; }

    [XmlElement("UserId")]
    public Guid UserId { get; set; }

    [XmlElement("CryptoId")]
    public string CryptoId { get; set; } = null!;

    [XmlElement("Quantity")]
    public decimal Quantity { get; set; }
}
