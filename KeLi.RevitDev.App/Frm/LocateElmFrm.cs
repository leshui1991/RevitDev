using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Form = System.Windows.Forms.Form;

namespace KeLi.RevitDev.App.Frm
{
    public partial class LocateElmFrm : Form
    {
        private static UIDocument Uidoc { get; set; }

        public List<Element> Elms { get; set; }

        public LocateElmFrm(UIDocument uidoc)
        {
            InitializeComponent();
            Uidoc = uidoc;
            Elms = new List<Element>();
        }

        private void BtnLocate_Click(object sender, EventArgs e)
        {
            var b1 = int.TryParse(tbId1.Text.Trim(), out var id1);
            var b2 = int.TryParse(tbId2.Text.Trim(), out var id2);
            var b3 = int.TryParse(tbId1.Text.Trim(), out var id3);
            var b4 = int.TryParse(tbId2.Text.Trim(), out var id4);
            var b5 = int.TryParse(tbId1.Text.Trim(), out var id5);
            var ids = new FilteredElementCollector(Uidoc.Document)
                .ToElementIds()
                .Select(s => s.IntegerValue)
                .ToList();

            if (b1 && ids.Contains(id1))
                Elms.Add(Uidoc.Document.GetElement(new ElementId(id1)));

            if (b2 && ids.Contains(id2))
                Elms.Add(Uidoc.Document.GetElement(new ElementId(id2)));

            if (b3 && ids.Contains(id3))
                Elms.Add(Uidoc.Document.GetElement(new ElementId(id3)));

            if (b4 && ids.Contains(id4))
                Elms.Add(Uidoc.Document.GetElement(new ElementId(id4)));

            if (b5 && ids.Contains(id5))
                Elms.Add(Uidoc.Document.GetElement(new ElementId(id5)));

            DialogResult = DialogResult.OK;
            Close();
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}