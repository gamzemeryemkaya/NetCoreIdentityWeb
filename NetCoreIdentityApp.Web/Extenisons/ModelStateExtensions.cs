using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace NetCoreIdentityApp.Web.Extenisons
{
    public static class ModelStateExtensions
    {
        // ModelStateDictionary'ye hata mesajlarını eklemek için kullanılan bir genişletme metodu
        public static void AddModelErrorList(this ModelStateDictionary modelState, List<string> errors)
        {
            // Verilen hata listesinin her öğesini döngü ile işler
            errors.ForEach(x =>
            {
                // ModelStateDictionary'ye her hata mesajını ekler
                // İlk parametre olarak string.Empty verilerek, hata mesajının belirli bir model özelliği ile ilişkilendirilmediği belirtilir
                modelState.AddModelError(string.Empty, x);
            });
        }

        public static void AddModelErrorList(this ModelStateDictionary modelState, IEnumerable<IdentityError> errors)
        {
            errors.ToList().ForEach(x =>
            {
                modelState.AddModelError(string.Empty, x.Description);
            });
        }
    }
}

//Bu kod, hataları ModelStateDictionary içinde toplu olarak işlemek için kullanışlıdır ve birden çok hata mesajını eklemeyi kolaylaştırır.
//Bu sayede modelin doğrulama hataları, daha sonra görüntülenmek üzere bir Razor Pages veya Razor View'da kullanılabilir.