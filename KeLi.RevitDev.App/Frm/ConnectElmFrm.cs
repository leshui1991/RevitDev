using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Form = System.Windows.Forms.Form;

namespace KeLi.RevitDev.App.Frm
{
    public partial class ConnectElmFrm : Form
    {
        private static UIDocument Uidoc { get; set; }

        public List<Element> Elms { get; set; }

        public ConnectElmFrm(UIDocument uidoc)
        {
            InitializeComponent();
            Uidoc = uidoc;
            Elms = Uidoc.Selection.GetElementIds().Select(s => Uidoc.Document.GetElement(s)).ToList();

            if (Elms.Count < 2)
                return;

            tbId1.Enabled = false;
            tbId2.Enabled = false;
        }

        private void BtnConnect_Click(object sender, EventArgs e)
        {
            if (tbId1.Enabled || tbId2.Enabled)
            {
                Elms = new List<Element>();

                var b1 = int.TryParse(tbId1.Text.Trim(), out var id1);
                var b2 = int.TryParse(tbId2.Text.Trim(), out var id2);
                var ids = new FilteredElementCollector(Uidoc.Document)
                    .ToElementIds()
                    .Select(s => s.IntegerValue)
                    .ToList();

                if (b1 && b2 && ids.Contains(id1) && ids.Contains(id2))
                {
                    var elm1 = Uidoc.Document.GetElement(new ElementId(id1));
                    var elm2 = Uidoc.Document.GetElement(new ElementId(id2));

                    Elms.Add(elm1);
                    Elms.Add(elm2);
                }
            }

            DialogResult = DialogResult.OK;
            Close();
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}