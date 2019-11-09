﻿/*
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
     |  |              Creation Time: 10/31/2019 04:51:51 PM |  |  |     |         |      |
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

using System.Collections.Generic;
using Autodesk.Revit.DB;
using KeLi.Common.Revit.Widget;

namespace KeLi.RevitDev.App.Common
{
    public static class TestGeometry
    {
        public static void TestPolygonAlgorithm(this XYZ pt, Document doc)
        {
            doc.AutoTransaction(() =>
            {
                var line = Line.CreateBound(pt, pt + new XYZ(10, 0, 0));

                doc.Create.NewModelCurve(line, doc.ActiveView.SketchPlane);
            });
        }

        public static void TestPolygonAlgorithm(this List<XYZ> pts, Document doc)
        {
            doc.AutoTransaction(() =>
            {
                foreach (var pt in pts)
                {
                    var line = Line.CreateBound(pt, pt + new XYZ(10, 0, 0));

                    doc.Create.NewModelCurve(line, doc.ActiveView.SketchPlane);
                }
            });
        }

        public static void TestPolygonAlgorithm(this Line line, Document doc)
        {
            doc.AutoTransaction(() =>
            {
                doc.Create.NewModelCurve(line, doc.ActiveView.SketchPlane);
            });
        }

        public static void TestPolygonAlgorithm(this List<Line> lines, Document doc)
        {
            doc.AutoTransaction(() =>
            {
                lines.ForEach(f => doc.Create.NewModelCurve(f, doc.ActiveView.SketchPlane));
            });
        }
    }
}