using Autodesk.Revit.UI;

namespace KeLi.RevitDev.App.Frm
{
    public partial class ViewToolXam : IDockablePaneProvider
    {
        public ViewToolXam()
        {
            InitializeComponent();
        }

        public void SetupDockablePane(DockablePaneProviderData data)
        {
            data.FrameworkElement = this;
            data.InitialState = new DockablePaneState { DockPosition = DockPosition.Bottom };
        }
    }
}