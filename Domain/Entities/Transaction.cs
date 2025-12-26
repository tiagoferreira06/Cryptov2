using System.Xml.Serialization;
using CryptoPlatform.Domain.Enums;

namespace CryptoPlatform.Domain.Entities;

[XmlRoot("Transaction")]
public class Transaction
{
    [XmlElement("Id")]
    public Guid Id { get; set; }

    [XmlElement("PortfolioId")]
    public Guid PortfolioId { get; set; }

    [XmlElement("Type")]
    public TransactionType Type { get; set; }

    [XmlElement("Quantity")]
    public decimal Quantity { get; set; }

    [XmlElement("PriceEur")]
    public decimal PriceEur { get; set; }

    [XmlElement("CreatedAt")]
    public DateTime CreatedAt { get; set; }
}
