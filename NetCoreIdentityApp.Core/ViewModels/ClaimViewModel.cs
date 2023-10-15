namespace NetCoreIdentityApp.Core.ViewModels
{
    public class ClaimViewModel
    {
        // Kimlik bilgisinin sağlayıcısını (Issuer) temsil eder. Örneğin, "Google", "Facebook" gibi.
        public string Issuer { get; set; } = null!;

        // Kimlik bilgisinin türünü temsil eder. Örneğin, "email", "role", "name" gibi.
        public string Type { get; set; } = null!;

        // Kimlik bilgisinin değerini temsil eder. Örneğin, kullanıcının e-posta adresi veya rol adı gibi.
        public string Value { get; set; } = null!;
    }
}

