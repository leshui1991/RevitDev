using System;
using System.Windows.Forms;
using Autodesk.Revit.UI;

namespace KeLi.RevitDev.App.Frm
{
    public partial class DoTaskFrm : Form
    {
        private readonly ExternalEvent _eventStart;

        private readonly ExternalEvent _eventStop;

        public DoTaskFrm()
        {
            InitializeComponent();
            _eventStart = ExternalEvent.Create(new WorkTask());
            _eventStop = ExternalEvent.Create(new TaskStop());
        }

        private void BtnStart_Click(object sender, EventArgs e)
        {
            _eventStart.Raise();
        }

        private void BtnStop_Click(object sender, EventArgs e)
        {
            _eventStop.Raise();
        }
    }

    public class WorkTask : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            TaskDialog.Show("Task Start", "The Task starts.");
        }

        public string GetName()
        {
            return "TaskStart";
        }
    }

    public class TaskStop : IExternalEventHandler
    {
        public void Execute(UIApplication app)
        {
            TaskDialog.Show("Task Stop", "The Task is stopped.");
        }

        public string GetName()
        {
            return "TaskStop";
        }
    }
}