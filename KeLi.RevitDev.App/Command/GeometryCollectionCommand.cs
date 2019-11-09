/*
 * MIT License
 *
 * Copyright(c) 2019 kelicto
 *
 * Permission is hereby granted, free of charge, to any person obtaining a copy
 * of this software and associated documentation files (the "Software"), to deal
 * in the Software without restriction, including without limitation the rights
 * to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
 * copies of the Software, and to permit persons to whom the Software is
 * furnished to do so, subject to the following conditions:
 *
 * The above copyright notice and this permission notice shall be included in all
 * copies or substantial portions of the Software.
 *
 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
 * IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
 * FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
 * AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
 * LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
 * OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
 * SOFTWARE.
 */

/*
             ,---------------------------------------------------,              ,---------,
        ,----------------------------------------------------------,          ,"        ,"|
      ,"                                                         ,"|        ,"        ,"  |
     +----------------------------------------------------------+  |      ,"        ,"    |
     |  .----------------------------------------------------.  |  |     +---------+      |
     |  | C:\>FILE -INFO                                     |  |  |     | -==----'|      |
     |  |                                                    |  |  |     |         |      |
     |  |                                                    |  |  |/----|`---=    |      |
     |  |              Author: kelicto                       |  |  |     |         |      |
     |  |              Email: kelistudy@163.com              |  |  |     |         |      |
     |  |              Creation Time: 10/31/2019 04:16:45 PM |  |  |     |         |      |
     |  | C:\>_                                              |  |  |     | -==----'|      |
     |  |                                                    |  |  |   ,/|==== ooo |      ;
     |  |                                                    |  |  |  // |(((( [66]|    ,"
     |  `----------------------------------------------------'  |," .;'| |((((     |  ,"
     +----------------------------------------------------------+  ;;  | |         |,"
        /_)_________________________________________________(_/  //'   | +---------+
           ___________________________/___  `,
          /  oooooooooooooooo  .o.  oooo /,   \,"-----------
         / ==ooooooooooooooo==.o.  ooo= //   ,`\--{)B     ,"
        /_==__==========__==_ooo__ooo=_/'   /___________,"
*/

using System.IO;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using KeLi.Common.Revit.Widget;

namespace KeLi.RevitDev.App.Command
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class GeometryCollectionCommand : IExternalCommand
    {
        private const string MESH_FILE_PATH = @"C:\Users\KeLi\Desktop\Element Mesh Data.txt";

        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            var elmIds = uidoc.Selection.GetElementIds();

            foreach (var elmId in elmIds)
            {
                var elm = doc.GetElement(elmId);

                if (elm == null)
                    continue;

                var sbPnts = new StringBuilder();
                var meshes = elm.GetMeshes();
                var triSum = 0;

                meshes.ForEach(f => triSum += f.NumTriangles);
                meshes.ForEach(m =>
                {
                    sbPnts.AppendLine("Current triangle number: " + m.NumTriangles.ToString());
                    m.GetMeshTriangles().ForEach(f =>
                    {
                        sbPnts.AppendLine(f.get_Index(0).ToString() + "\t" + f.get_Vertex(0).ToString());
                        sbPnts.AppendLine(f.get_Index(1).ToString() + "\t" + f.get_Vertex(1).ToString());
                        sbPnts.AppendLine(f.get_Index(2).ToString() + "\t" + f.get_Vertex(2).ToString());
                        sbPnts.AppendLine();
                    });
                });

                File.WriteAllLines(MESH_FILE_PATH, sbPnts.ToString().Replace("\r\n", "\r").Split("\r".ToCharArray()[0]));
                MessageBox.Show(sbPnts.ToString(), "Triangle total Number: " + triSum);
            }

            return Result.Succeeded;
        }
    }
}