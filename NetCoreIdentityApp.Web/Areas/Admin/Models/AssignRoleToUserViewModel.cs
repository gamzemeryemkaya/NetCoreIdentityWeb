namespace NetCoreIdentityApp.Web.Areas.Admin.Models
{
    //rol atama için
    public class AssignRoleToUserViewModel
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
        //bu role sahip mi değil mi -var mı yok mu
        public bool Exist { get; set; }
    }
}
