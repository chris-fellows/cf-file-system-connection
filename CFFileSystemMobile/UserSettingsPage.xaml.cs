using CFFileSystemMobile.ViewModels;

namespace CFFileSystemMobile
{
    public partial class UserSettingsPage : ContentPage
    {
        private readonly UserSettingsPageModel _model;

        public UserSettingsPage(UserSettingsPageModel model)
        {
            InitializeComponent();

            _model = model;
            this.BindingContext = _model;
        }
    }
}