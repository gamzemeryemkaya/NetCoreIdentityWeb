namespace NetCoreIdentityApp.Core.PermissionsRoot
{
    public static class Permissions
    {
        // Stock (Stok) ile ilgili izinler
        public static class Stock
        {
            public const string Read = "Permissions.Stock.Read"; // Stok okuma izni
            public const string Create = "Permissions.Stock.Create"; // Stok oluşturma izni
            public const string Update = "Permissions.Stock.Update"; // Stok güncelleme izni
            public const string Delete = "Permissions.Stock.Delete"; // Stok silme izni
        }

        // Order (Sipariş) ile ilgili izinler
        public static class Order
        {
            public const string Read = "Permissions.Order.Read"; // Sipariş okuma izni
            public const string Create = "Permissions.Order.Create"; // Sipariş oluşturma izni
            public const string Update = "Permissions.Order.Update"; // Sipariş güncelleme izni
            public const string Delete = "Permissions.Order.Delete"; // Sipariş silme izni
        }

        // Catalog (Katalog) ile ilgili izinler
        public static class Catalog
        {
            public const string Read = "Permissions.Catalog.Read"; // Katalog okuma izni
            public const string Create = "Permissions.Catalog.Create"; // Katalog oluşturma izni
            public const string Update = "Permissions.Catalog.Update"; // Katalog güncelleme izni
            public const string Delete = "Permissions.Catalog.Delete"; // Katalog silme izni
        }
    }
}
