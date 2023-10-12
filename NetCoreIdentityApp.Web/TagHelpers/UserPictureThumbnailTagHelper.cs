namespace NetCoreIdentityApp.Web.TagHelpers
{
    using Microsoft.AspNetCore.Razor.TagHelpers;

    namespace AspNetCoreIdentityApp.Web.TagHelpers
    {
        // Bu Tag Helper, kullanıcıların profil resimlerini HTML img etiketi içinde göstermek için kullanılır.
        public class UserPictureThumbnailTagHelper : TagHelper
        {
            // Kullanıcı resminin URL'sini almak için kullanılacak özellik.
            public string? PictureUrl { get; set; }

            // Tag Helper işleme aşamasında çalışır ve HTML çıktısını oluşturur.
            public override void Process(TagHelperContext context, TagHelperOutput output)
            {
                // Oluşturulan etiketin adını "img" olarak belirler.
                output.TagName = "img";

                // Eğer PictureUrl özelliği boşsa (null veya boş bir dize), varsayılan bir resim kullanır.
                if (string.IsNullOrEmpty(PictureUrl))
                {
                    // "src" özelliğini varsayılan kullanıcı resmi URL'sine ayarlar.
                    output.Attributes.SetAttribute("src", "/userpictures/default_user_picture.jpg");
                }
                else
                {
                    // "src" özelliğini kullanıcının resim URL'sine ayarlar.
                    output.Attributes.SetAttribute("src", $"/userpictures/{PictureUrl}");
                }
            }
        }
    }

}
