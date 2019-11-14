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

using System;
using System.Collections.Generic;
using System.Linq;
using Autodesk.Revit.DB;
using KeLi.Common.Revit.Widget;
using KeLi.RevitDev.App.Entity;
using KeLi.RevitDev.App.Properties;

namespace KeLi.RevitDev.App.Common
{
    public static class SeatManager
    {
        public static List<FamilyInstance> GetFloorSeats(this Document doc)
        {
            return doc.GetTypeElements<FamilyInstance>(BuiltInCategory.OST_Furniture)
                .Where(w => w.LevelId == doc.ActiveView.GenLevel.Id).ToList();
        }

        public static FamilyInstance Get90Seat(this Document doc)
        {
            return doc.GetTypeElements<FamilyInstance>(BuiltInCategory.OST_Furniture)
                .FirstOrDefault(w => Math.Abs((w.Location as LocationPoint).Rotation - Math.PI / 2) < 10e-3);
        }

        public static FillPatternElement GetFirstFillPattern(this Document doc)
        {
            return doc.GetTypeElements<FillPatternElement>()
                .FirstOrDefault(f => f.GetFillPattern().IsSolidFill);
        }

        public static void SetColorFill(this Element seat, Element fillPattern, Document doc, Color color)
        {
            var graSetting = doc.ActiveView.GetElementOverrides(seat.Id);

            if (fillPattern != null)
                graSetting.SetProjectionFillPatternId(fillPattern.Id);

            graSetting.SetProjectionFillColor(color);
            doc.ActiveView.SetElementOverrides(seat.Id, graSetting);
        }

        public static void SetSeatParameters(this Element seat, FillPatternElement fillPattern, Document doc, SeatInfo info)
        {
            seat.GetParameters(Resources.ParameterName_Width).FirstOrDefault()?.Set(info.Width);
            seat.GetParameters(Resources.ParameterName_Length).FirstOrDefault()?.Set(info.Length);
            seat.SetColorFill(fillPattern, doc, info.FillColor);
        }

        public static List<Line> GetEdgeList(this SpatialElement room)
        {
            var result = new List<Line>();
            var option = new SpatialElementBoundaryOptions
            {
                StoreFreeBoundaryFaces = true,
                SpatialElementBoundaryLocation = SpatialElementBoundaryLocation.CoreBoundary
            };
            var segments = room.GetBoundarySegments(option).SelectMany(s => s);

            foreach (var seg in segments)
            {
                var sp = seg.GetCurve().GetEndPoint(0);
                var ep = seg.GetCurve().GetEndPoint(1);

                result.Add(Line.CreateBound(sp, ep));
            }

            return result;
        }
    }
}