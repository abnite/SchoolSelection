using PaypalServerSdk.Standard;
using PaypalServerSdk.Standard.Authentication;

namespace SchoolSelection.Services;

public class PayPalClient
{
    public static PaypalServerSdkClient GetClient()
    {
        return new PaypalServerSdkClient.Builder()
            .ClientCredentialsAuth(new ClientCredentialsAuthModel.Builder(
                "YourClientId", // Replace with your PayPal Client ID
                "YourClientSecret" // Replace with your PayPal Client Secret
            ).Build())
            .Environment(PaypalServerSdk.Standard.Environment.Sandbox) // Use Environment.Production for live
            .Build();
    }
}