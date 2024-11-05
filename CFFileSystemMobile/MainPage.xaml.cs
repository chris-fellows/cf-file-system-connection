using CFFileSystemMobile.ViewModels;

namespace CFFileSystemMobile
{
    public partial class MainPage : ContentPage
    {
        private readonly MainPageModel _model;

        //public MainPage()
        //{
        //    InitializeComponent();            
        //}

        public MainPage(MainPageModel model)
        {
            InitializeComponent();

            _model = model;
            this.BindingContext = _model;
        }
    }
}
