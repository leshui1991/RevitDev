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
using Autodesk.Revit.DB.Architecture;
using Autodesk.Revit.UI.Selection;
using KeLi.Common.Revit.Relation;
using KeLi.Common.Revit.Widget;
using KeLi.RevitDev.App.Entity;
using KeLi.RevitDev.App.Properties;
using Line = Autodesk.Revit.DB.Line;
using MessageBox = System.Windows.Forms.MessageBox;

namespace KeLi.RevitDev.App.Common
{
    public static class SeatDesignAssist
    {
        private static DesignStatus Status { get; } = new DesignStatus();

        private static PositionRequest Request { get; set; }

        private static Document Doc { get; set; }

        private static BoundingBoxXYZ PickBox { get; set; } = new BoundingBoxXYZ();

        private static List<Line> PickEdges { get; set; } = new List<Line>();

        private static FillPatternElement FillPattern { get; set; }

        private static FamilyInstance RefSeat { get; set; }

        public static void AutoPutSeat(this Document doc, PickedBox box, PositionRequest request, List<SeatBatch> batches)
        {
            Doc = doc;
            Request = request;
            PickBox = box.ToBoundingBoxXYZ();
            PickEdges = PickBox.GetPlaneEdges();
            FillPattern = doc.GetFirstFillPattern();
            RefSeat = doc.GetFloorSeats().FirstOrDefault();

            if (RefSeat == null)
            {
                MessageBox.Show("The document has's any seat, please put a seat!");
                return;
            }

            var floorRooms = GetRoomListOnFloor();
            var seatInfos = GetSeatInfosOnFloor(floorRooms, batches);

            PutSeatList(doc, seatInfos);

            if (!ConformPasswayWidth())
                MessageBox.Show("The passsway width is qualified!");
        }

        private static List<SeatInfo> GetSeatInfosOnFloor(List<Room> rooms, List<SeatBatch> batches)
        {
            var results = new List<SeatInfo>();

            foreach (var room in rooms)
            {
                InitRoomStatus(room);

                // Copies a new seat batch list, it's for each room.
                var currentBatches = batches.Select(s => s.Clone()).Cast<SeatBatch>().ToList();

                var alignBottom = !room.Name.Contains(Resources.RoomName_North);
                var nextPt = GetStartPoint(alignBottom);

                for (var i = 0; i < currentBatches.Count; i++)
                {
                    // If puts seat to last row, skips the current task.
                    if (Request.AlignLeft ? nextPt.X > Status.InsBox.Max.X : nextPt.X < Status.InsBox.Min.X)
                        continue;

                    // If the results count is 0, calls Max method throw new null exception
                    if (i > 0)
                    {
                        var f1 = Request.AlignLeft && nextPt.X > results.Max(m => m.Location.X);
                        var f2 = !Request.AlignLeft && nextPt.X < results.Min(m => m.Location.X);

                        // Goes to the next row, must revise y value.
                        if (f1 || f2)
                            nextPt = ReviseYValue(nextPt, alignBottom);

                        if (!alignBottom)
                        {
                            if (f1 || f2)
                                nextPt = new XYZ(nextPt.X, nextPt.Y - currentBatches[i].Length, nextPt.Z);
                            else
                                nextPt = new XYZ(nextPt.X, nextPt.Y + currentBatches[i - 1].Length - currentBatches[i].Length, nextPt.Z);
                        }
                    }
                    else if (!alignBottom)
                        nextPt = new XYZ(nextPt.X, nextPt.Y - currentBatches[i].Length, nextPt.Z);

                    // When the next seat location isn't in the room polygon, breaks the loop.
                    if (!CanPut(nextPt))
                        break;

                    // The next room should skips the batch, because the batch seats run out.
                    if (currentBatches[i].UsableNumber == 0)
                        continue;

                    var seatInfos = GetSeatInfosOnBatch(currentBatches[i], ref nextPt, alignBottom);

                    results.AddRange(seatInfos);
                }
            }

            return results;
        }

        private static List<SeatInfo> GetSeatInfosOnBatch(SeatBatch batch, ref XYZ nextPt, bool alignBottom)
        {
            var results = new List<SeatInfo>();
            var canPut = true;
            var sum = 0;
            var f1 = Request.AlignLeft;

            while (batch.UsableNumber > 0 && canPut)
            {
                sum++;

                // In order to not be an infinite loop.
                if (sum > 1000)
                {
                    MessageBox.Show("The loop is endless, please contact the program's developer!");
                    break;
                }

                var f2 = Status.RowNum % 2 == 0;

                // The seat aligns left and double num or aligns right and single num, should be retated.
                var seat = new SeatInfo(nextPt, batch, Status.RowNum, f1 && f2 || !f1 && !f2);
                var pts = GetSeatVectors(batch, nextPt, seat);

                if (pts.All(CanPut))
                {
                    results.Add(seat);
                    batch.UsableNumber -= 1;
                }

                nextPt = GetNextPt(batch, nextPt, ref canPut, alignBottom);
            }

            return results;
        }

        private static XYZ GetNextPt(SeatBatch batch, XYZ nextPt, ref bool canPut, bool alignBottom)
        {
            // The next seat location should set the location's y axis plus seat length.
            var pt1 = new XYZ(nextPt.X, nextPt.Y + batch.Length, nextPt.Z);
            var pt2 = new XYZ(nextPt.X, nextPt.Y - batch.Length, nextPt.Z);

            nextPt = alignBottom ? pt1 : pt2;

            var f1 = nextPt.Y > Status.InsBox.Max.Y || Status.InsBox.Max.Y - nextPt.Y < batch.Length;

            // If seat aligns bottom, then goes to next row util the next point y is less than ins box min y.
            var f2 = nextPt.Y < Status.InsBox.Min.Y && nextPt.Y - Status.InsBox.Min.Y < batch.Length;

            if (!Status.IsLastRow)
            {
                if (alignBottom ? f1 : f2)
                {
                    Status.RowNum++;
                    nextPt = CalcNewRowPosition(nextPt, batch, alignBottom);

                    // To not multi calc.
                    canPut = Request.AlignLeft ? !(nextPt.X > Status.InsBox.Max.X) :
                        !(nextPt.X < Status.InsBox.Min.X);

                    // It's an invalid point, must return.
                    // Because boxInsPts count is 0, throw new exception.
                    if (!canPut)
                        return nextPt;

                    if (!alignBottom && Status.RowNum % 2 == 1)
                    {
                        nextPt = ReviseYValue(nextPt, false);
                        nextPt = new XYZ(nextPt.X, nextPt.Y - batch.Length, nextPt.Z);
                    }
                }

                // Must compares again.
                var f3 = Status.InsBox.Max.X - nextPt.X < batch.Width;
                var f4 = nextPt.Y < Status.InsBox.Min.Y;

                // Not calc muilt times.
                Status.IsLastRow = Request.AlignLeft ? f3 : f4;
            }

            f1 = nextPt.Y > Status.InsBox.Max.Y || Status.InsBox.Max.Y - nextPt.Y < batch.Length;
            f2 = nextPt.Y < Status.InsBox.Min.Y && nextPt.Y - Status.InsBox.Min.Y > batch.Length;

            // If true, means that put last row and last column, should break.
            if (alignBottom ? f1 : f2)
                canPut = false;

            return nextPt;
        }

        private static void PutSeatList(Document doc, List<SeatInfo> seatInfos)
        {
            Doc.AutoTransaction(() =>
            {
                foreach (var seatInfo in seatInfos)
                {
                    var offset = seatInfo.Location - (RefSeat.Location as LocationPoint)?.Point;
                    var cloneSeatIds = ElementTransformUtils.CopyElement(Doc, RefSeat.Id, offset).ToList();

                    if (cloneSeatIds.Count == 0)
                    {
                        MessageBox.Show("Copys the seat, but gets not a seat.");
                        return;
                    }

                    var seat = Doc.GetElement(cloneSeatIds[0]);

                    // Sets the seat some parameters.
                    seat.SetSeatParameters(FillPattern, doc, seatInfo);

                    // Sets the seat fill color.
                    seat.SetColorFill(FillPattern, doc, seatInfo.FillColor);

                    if (!seatInfo.IsRotation)
                        continue;

                    var location = seatInfo.Location;
                    var startPt = new XYZ(location.X, location.Y + seatInfo.Length / 2, 0);
                    var endPt = new XYZ(location.X, location.Y + seatInfo.Length / 2, 1);
                    var line = Line.CreateBound(startPt, endPt);

                    // No use mirror, mirror element is very slow.
                    ElementTransformUtils.RotateElement(Doc, seat.Id, line, Math.PI);
                }
            });
        }

        private static XYZ GetStartPoint(bool alignBottom)
        {
            var box = Status.InsBox;
            var pt1 = new XYZ(box.Min.X + 0.01, box.Min.Y + 0.05, box.Min.Z);
            var pt2 = new XYZ(box.Min.X + 0.01, box.Max.Y - 0.05, box.Min.Z);
            var pt3 = new XYZ(box.Max.X - 0.01, box.Min.Y + 0.05, box.Min.Z);
            var pt4 = new XYZ(box.Max.X - 0.01, box.Max.Y - 0.05, box.Min.Z);

            return Request.AlignLeft ? alignBottom ? pt1 : pt2 : alignBottom ? pt3 : pt4;
        }

        private static List<XYZ> GetSeatVectors(SeatBatch batch, XYZ nextPt, SeatInfo seat)
        {
            var leftBottomPt = nextPt;
            var rightBottomPt = new XYZ(nextPt.X + batch.Width, nextPt.Y, nextPt.Z);
            var leftTopPt = new XYZ(nextPt.X, nextPt.Y + batch.Length, nextPt.Z);
            var rightTopPt = new XYZ(nextPt.X + batch.Width, nextPt.Y + batch.Length, nextPt.Z);

            if (!seat.IsRotation)
                return new List<XYZ> { leftBottomPt, rightBottomPt, leftTopPt, rightTopPt };

            // If you don't understand it, you should draw picture.
            leftBottomPt = new XYZ(nextPt.X - batch.Width, nextPt.Y, nextPt.Z);
            rightBottomPt = nextPt;
            leftTopPt = new XYZ(nextPt.X - batch.Width, nextPt.Y + batch.Length, nextPt.Z);
            rightTopPt = new XYZ(nextPt.X, nextPt.Y + batch.Length, nextPt.Z);

            return new List<XYZ> { leftBottomPt, rightBottomPt, leftTopPt, rightTopPt };
        }

        private static XYZ CalcNewRowPosition(XYZ nextPt, SeatBatch batch, bool alignBottom)
        {
            var pt1 = new XYZ(nextPt.X + Request.RowWidth, Status.InsBox.Min.Y + 0.05, nextPt.Z);
            var pt2 = new XYZ(nextPt.X - Request.RowWidth, Status.InsBox.Min.Y + 0.05, nextPt.Z);
            var pt3 = new XYZ(nextPt.X, Status.InsBox.Min.Y + 0.05, nextPt.Z);

            // If seat don't align bottom, the seat y value must redure on new row
            var pt4 = new XYZ(nextPt.X + Request.RowWidth, Status.InsBox.Max.Y - batch.Length - 0.05, nextPt.Z);
            var pt5 = new XYZ(nextPt.X - Request.RowWidth, Status.InsBox.Max.Y - batch.Length - 0.05, nextPt.Z);
            var pt6 = new XYZ(nextPt.X, Status.InsBox.Max.Y - batch.Length - 0.05, nextPt.Z);

            var f1 = Status.RowNum % 2 == 0;
            var f2 = Request.AlignLeft;

            return alignBottom ? (f1 ? (f2 ? pt1 : pt2) : pt3) : (f1 ? (f2 ? pt4 : pt5) : pt6);
        }

        private static XYZ ReviseYValue(XYZ nextPt, bool alignBottom)
        {
            var line = Line.CreateBound(new XYZ(nextPt.X, -10000000, nextPt.Z), new XYZ(nextPt.X, 10000000, nextPt.Z));
            var roomInsPts = line.GetPlaneInsPointList(Status.RoomEdges);
            var boxInsPts = line.GetPlaneInsPointList(PickEdges);
            var roomInsMinY = roomInsPts.Min(m => m.Y);
            var boxInsMinY = boxInsPts.Min(m => m.Y);
            var roomInsMaxY = roomInsPts.Max(m => m.Y);
            var boxInsMaxY = boxInsPts.Max(m => m.Y);

            // Revises the next point y min value.
            var pt1 = new XYZ(nextPt.X, Math.Max(roomInsMinY, boxInsMinY) + 0.05, nextPt.Z);
            var pt2 = new XYZ(nextPt.X, Math.Min(roomInsMaxY, boxInsMaxY) - 0.05, nextPt.Z);

            return alignBottom ? pt1 : pt2;
        }

        private static void InitRoomStatus(Room room)
        {
            Status.RoomBox = room.GetBoundingBox(Doc);
            Status.RoomEdges = room.GetEdgeList();
            Status.RowNum = 1;
            Status.IsLastRow = false;
            Status.InsBox = Status.RoomBox.GetInsBox(PickBox);
            Status.InsEdges = Status.InsBox.GetPlaneEdges();
        }

        private static bool CanPut(XYZ pt)
        {
            return pt.InPlanePolygon(Status.InsEdges) && pt.InPlanePolygon(Status.RoomEdges);
        }

        private static List<Room> GetRoomListOnFloor()
        {
            var rooms = Doc.GetCategoryElements(BuiltInCategory.OST_Rooms, true).Cast<Room>();
            var results = new List<Room>();
            var boxVectors = PickBox.GetPlaneVectors();

            foreach (var room in rooms)
            {
                var roomEdges = room.GetEdgeList();
                var roomVectors = roomEdges.GetDistinctPointList();
                var boxPtInRoom = boxVectors.Any(a => a.InPlanePolygon(roomEdges));
                var roomPtInBox = roomVectors.Any(a => a.InPlanePolygon(PickEdges));

                if (!boxPtInRoom && !roomPtInBox)
                    continue;

                results.Add(room);
            }

            return results;
        }

        private static bool ConformPasswayWidth()
        {
            return true;
        }
    }
}