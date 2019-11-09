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

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using KeLi.Common.Revit.Builder;
using KeLi.Common.Revit.Widget;

namespace KeLi.RevitDev.App.Command
{
    [Transaction(TransactionMode.Manual)]
    [Regeneration(RegenerationOption.Manual)]
    public class CurvePerformanceCommand : IExternalCommand
    {
        public Result Execute(ExternalCommandData commandData, ref string message, ElementSet elements)
        {
            var uidoc = commandData.Application.ActiveUIDocument;
            var doc = uidoc.Document;
            var arc = Arc.Create(uidoc.ActiveView.SketchPlane.GetPlane(), 100, 0, Math.PI / 2);
            var points = arc.Tessellate().ToList();
            var watch = new Stopwatch();
            var sb = new StringBuilder();

            watch.Start();
            points = TestOneTransPerf(doc, points);
            watch.Stop();
            sb.AppendLine("One Transaction: " + watch.Elapsed.TotalMilliseconds);

            watch.Start();
            points = TestFourTransPerf(doc, points);
            watch.Stop();
            sb.AppendLine("Four Transactions: " + watch.Elapsed.TotalMilliseconds);

            watch.Start();
            TestSomeTransPerf(doc, points);
            watch.Stop();
            sb.AppendLine("Some Transactions: " + watch.Elapsed.TotalMilliseconds);

            MessageBox.Show(sb.ToString(), "Transaction Pref Test");

            return Result.Succeeded;
        }

        public static List<XYZ> TestOneTransPerf(Document doc, List<XYZ> points)
        {
            points = points.Select(s => new XYZ(s.X + 20, s.Y, s.Z)).ToList();

            doc.AutoTransaction(() =>
            {
                for (int i = 0; i < points.Count - 1; i++)
                {
                    var endIndex = i + 1;

                    if (endIndex >= points.Count)
                        break;

                    var line = Line.CreateBound(points[i], points[endIndex]);

                    doc.AddWall(line, doc.GetTypeElements<Level>()?[0].Id);
                    i = endIndex - 1;
                }
            });

            points = points.Select(s => new XYZ(s.X + 40, s.Y, s.Z)).ToList();

            doc.AutoTransaction(() =>
            {
                for (var i = 0; i < points.Count - 1; i++)
                {
                    var endIndex = i + 2;

                    if (endIndex >= points.Count)
                        break;

                    var line = Line.CreateBound(points[i], points[endIndex]);

                    doc.AddWall(line, doc.GetTypeElements<Level>()?[0].Id);
                    i = endIndex - 1;
                }
            });

            points = points.Select(s => new XYZ(s.X + 60, s.Y, s.Z)).ToList();

            doc.AutoTransaction(() =>
            {
                for (var i = 0; i < points.Count - 1; i++)
                {
                    var endIndex = i + 3;

                    if (endIndex >= points.Count)
                        break;

                    var line = Line.CreateBound(points[i], points[endIndex]);

                    doc.AddWall(line, doc.GetTypeElements<Level>()?[0].Id);
                    i = endIndex - 1;
                }
            });

            points = points.Select(s => new XYZ(s.X + 80, s.Y, s.Z)).ToList();

            doc.AutoTransaction(() =>
            {
                for (var i = 0; i < points.Count - 1; i++)
                {
                    var endIndex = i + 4;

                    if (endIndex >= points.Count)
                        break;

                    var line = Line.CreateBound(points[i], points[endIndex]);

                    doc.AddWall(line, doc.GetTypeElements<Level>()?[0].Id);
                    i = endIndex - 1;
                }
            });

            return points;
        }

        public static List<XYZ> TestFourTransPerf(Document doc, List<XYZ> points)
        {
            points = points.Select(s => new XYZ(s.X + 60, s.Y, s.Z)).ToList();

            doc.AutoTransaction(() =>
            {
                for (var i = 0; i < points.Count - 1; i++)
                {
                    var endIndex = i + 1;

                    if (endIndex >= points.Count)
                        break;

                    var line = Line.CreateBound(points[i], points[endIndex]);

                    doc.AddWall(line, doc.GetTypeElements<Level>()?[0].Id);
                    i = endIndex - 1;
                }
            });

            points = points.Select(s => new XYZ(s.X + 80, s.Y, s.Z)).ToList();

            doc.AutoTransaction(() =>
            {
                for (var i = 0; i < points.Count - 1; i++)
                {
                    var endIndex = i + 2;

                    if (endIndex >= points.Count)
                        break;

                    var line = Line.CreateBound(points[i], points[endIndex]);

                    doc.AddWall(line, doc.GetTypeElements<Level>()?[0].Id);
                    i = endIndex - 1;
                }
            });

            points = points.Select(s => new XYZ(s.X + 100, s.Y, s.Z)).ToList();

            doc.AutoTransaction(() =>
            {
                for (var i = 0; i < points.Count - 1; i++)
                {
                    var endIndex = i + 3;

                    if (endIndex >= points.Count)
                        break;

                    var line = Line.CreateBound(points[i], points[endIndex]);

                    doc.AddWall(line, doc.GetTypeElements<Level>()?[0].Id);
                    i = endIndex - 1;
                }
            });

            points = points.Select(s => new XYZ(s.X + 120, s.Y, s.Z)).ToList();

            doc.AutoTransaction(() =>
            {
                for (var i = 0; i < points.Count - 1; i++)
                {
                    var endIndex = i + 4;

                    if (endIndex >= points.Count)
                        break;

                    var line = Line.CreateBound(points[i], points[endIndex]);

                    doc.AddWall(line, doc.GetTypeElements<Level>()?[0].Id);
                    i = endIndex - 1;
                }
            });

            return points;
        }

        public static List<XYZ> TestSomeTransPerf(Document doc, List<XYZ> points)
        {
            points = points.Select(s => new XYZ(s.X + 100, s.Y, s.Z)).ToList();

            for (var i = 0; i < points.Count - 1; i++)
            {
                var endIndex = i + 1;

                if (endIndex >= points.Count)
                    break;

                var line = Line.CreateBound(points[i], points[endIndex]);

                doc.AddWall(line, doc.GetTypeElements<Level>()?[0].Id);
                i = endIndex - 1;
            }

            points = points.Select(s => new XYZ(s.X + 120, s.Y, s.Z)).ToList();

            for (var i = 0; i < points.Count - 1; i++)
            {
                var endIndex = i + 2;

                if (endIndex >= points.Count)
                    break;

                var line = Line.CreateBound(points[i], points[endIndex]);

                doc.AddWall(line, doc.GetTypeElements<Level>()?[0].Id);
                i = endIndex - 1;
            }

            points = points.Select(s => new XYZ(s.X + 140, s.Y, s.Z)).ToList();

            for (var i = 0; i < points.Count - 1; i++)
            {
                var endIndex = i + 3;

                if (endIndex >= points.Count)
                    break;

                var line = Line.CreateBound(points[i], points[endIndex]);

                doc.AddWall(line, doc.GetTypeElements<Level>()?[0].Id);
                i = endIndex - 1;
            }

            points = points.Select(s => new XYZ(s.X + 160, s.Y, s.Z)).ToList();

            for (var i = 0; i < points.Count - 1; i++)
            {
                var endIndex = i + 4;

                if (endIndex >= points.Count)
                    break;

                var line = Line.CreateBound(points[i], points[endIndex]);

                doc.AddWall(line, doc.GetTypeElements<Level>()?[0].Id);
                i = endIndex - 1;
            }

            return points;
        }
    }
}