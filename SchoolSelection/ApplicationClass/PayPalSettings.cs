namespace SchoolSelection.ApplicationClass;

public class PayPalSettings
{
    public string Environment { get; set; }
    public PayPalEnvironmentConfig Sandbox { get; set; }
    public PayPalEnvironmentConfig Live { get; set; }

    public PayPalEnvironmentConfig GetCurrentEnvironmentConfig()
    {
        return Environment?.ToLower() == "live" ? Live : Sandbox;
    }
}