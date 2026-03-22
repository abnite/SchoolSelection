using System.Text.Json.Serialization;

namespace SchoolSelection.Models;

public class GetOrderResponse
{
    public string Id { get; set; } // Order ID

    public string Status { get; set; } // Status of the order (e.g., COMPLETED, APPROVED)

    [JsonPropertyName("payment_source")]
    public PaymentSourceDetails PaymentSource { get; set; } // Matches `payment_source` in JSON

    [JsonPropertyName("purchase_units")]
    public List<PurchaseUnit> PurchaseUnits { get; set; } // Matches `purchase_units` in JSON

    [JsonPropertyName("payer")]
    public PayerDetails Payer { get; set; } // Matches `payer` in JSON

    public class PaymentSourceDetails
    {
        public PayPal PayPal { get; set; }
    }

    public class PayPal
    {
        [JsonPropertyName("email_address")]
        public string EmailAddress { get; set; }

        [JsonPropertyName("account_id")]
        public string AccountId { get; set; }

        [JsonPropertyName("account_status")]
        public string AccountStatus { get; set; }

        public PayerName Name { get; set; }

        public Address Address { get; set; }
    }

    public class PayerDetails
    {
        public PayerName Name { get; set; }

        [JsonPropertyName("email_address")]
        public string EmailAddress { get; set; }

        [JsonPropertyName("payer_id")]
        public string PayerId { get; set; }

        public Address Address { get; set; }
    }

    public class PayerName
    {
        [JsonPropertyName("given_name")]
        public string GivenName { get; set; }

        [JsonPropertyName("surname")]
        public string Surname { get; set; }
    }

    public class Address
    {
        [JsonPropertyName("address_line_1")]
        public string AddressLine1 { get; set; } // Street address

        [JsonPropertyName("admin_area_2")]
        public string AdminArea2 { get; set; } // City

        [JsonPropertyName("admin_area_1")]
        public string AdminArea1 { get; set; } // State

        [JsonPropertyName("postal_code")]
        public string PostalCode { get; set; }

        [JsonPropertyName("country_code")]
        public string CountryCode { get; set; }
    }

    public class PurchaseUnit
    {
        [JsonPropertyName("reference_id")]
        public string ReferenceId { get; set; }

        public Shipping Shipping { get; set; }

        public Payments Payments { get; set; }
    }

    public class Shipping
    {
        public ShippingName Name { get; set; }

        public Address Address { get; set; }
    }

    public class ShippingName
    {
        [JsonPropertyName("full_name")]
        public string FullName { get; set; }
    }

    public class Payments
    {
        public List<Capture> Captures { get; set; }
    }

    public class Capture
    {
        public string Id { get; set; } // Transaction ID

        public string Status { get; set; } // e.g., COMPLETED

        public Amount Amount { get; set; }

        [JsonPropertyName("final_capture")]
        public bool FinalCapture { get; set; }

        [JsonPropertyName("create_time")]
        public DateTime CreateTime { get; set; }

        [JsonPropertyName("update_time")]
        public DateTime UpdateTime { get; set; }
    }

    public class Amount
    {
        [JsonPropertyName("currency_code")]
        public string CurrencyCode { get; set; }

        public string Value { get; set; }
    }
}
