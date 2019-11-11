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

        public static void AutoPutSeat(this Document doc, PickedBox box, PositionRequest request, List<SeatBatch> batches)
        {
            Doc = doc;
            Request = request;
            PickBox = box.ToBoundingBoxXYZ();
            PickEdges = PickBox.GetPlaneEdges();
            FillPattern = doc.GetFirstFillPattern();
            Status.RefSeat = doc.GetFloorSeats().FirstOrDefault();

            if (Status.RefSeat == null)
            {
                MessageBox.Show("The document has's any seat, please put a seat!");
                return;
            }

            // It's very crucial and integral, else maybe produce some vacant space that can put seat.
            batches = batches.OrderBy(o => o.Length).ToList();

            var floorRooms = GetRoomListOnFloor();
            var seatInfos = GetSeatInfosOnFloor(floorRooms, batches);

            PutSeatList(doc, seatInfos);

            if (!ConformPasswayWidth())
                MessageBox.Show("The passsway width is qualified!");
        }

        private static void PutSeatList(Document doc, List<SeatInfo> seatInfos)
        {
            Doc.AutoTransaction(() =>
            {
                foreach (var seatInfo in seatInfos)
                {
                    var offset = seatInfo.Location - (Status.RefSeat.Location as LocationPoint)?.Point;
                    var cloneSeatIds = ElementTransformUtils.CopyElement(Doc, Status.RefSeat.Id, offset).ToList();

                    if (cloneSeatIds.Count == 0)
                    {
                        MessageBox.Show("Copys the seat, but gets not a seat.");
                        return;
                    }

                    var seat = Doc.GetElement(cloneSeatIds[0]);

                    // Sets the seat some parameters.
                    seat.SetSeatParameters(doc, Status, seatInfo);

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

        private static XYZ GetStartPoint()
        {
            var box = Status.InsBox;
            var pt1 = new XYZ(box.Min.X + 0.01, box.Min.Y + 0.2, box.Min.Z);
            var pt2 = new XYZ(box.Max.X - 0.01, box.Max.Y + 0.2, box.Min.Z);

            // TODO: I should solve the precision problem.
            return Request.AlignLeft ? pt1 : pt2;
        }

        private static List<SeatInfo> GetSeatInfosOnBatch(List<Line> roomEdges, SeatBatch batch, ref XYZ nextPt)
        {
            var results = new List<SeatInfo>();
            var canPut = true;
            var sum = 0;

            while (batch.UsableNumber > 0 && canPut)
            {
                sum++;

                // In order to not be an infinite loop.
                if (sum > 1000)
                {
                    MessageBox.Show("The loop is endless, please contact the program's developer!");
                    break;
                }

                var seat = new SeatInfo(nextPt, batch, Status.RowNum)
                {
                    // The seat aligns left and double num or aligns right and single num, should be retated.
                    IsRotation = Request.AlignLeft && Status.RowNum % 2 == 0
                        || !Request.AlignLeft && Status.RowNum % 2 == 1
                };
                var pts = GetSeatVectors(batch, nextPt, seat);

                if (CanPut(pts, roomEdges))
                {
                    results.Add(seat);
                    batch.UsableNumber -= 1;
                }

                nextPt = GetNextPt(batch, nextPt, ref canPut);
            }

            return results;
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

        private static XYZ GetNextPt(SeatBatch batch, XYZ nextPt, ref bool canPut)
        {
            // The next seat location should set the location's y axis plus seat length.
            nextPt = new XYZ(nextPt.X, nextPt.Y + batch.Length, nextPt.Z);

            if (!Status.IsLastRow)
            {
                if (nextPt.Y > Status.InsBox.Max.Y || Status.InsBox.Max.Y - nextPt.Y < batch.Length)
                {
                    Status.RowNum++;
                    nextPt = CalcNewLinePosition(nextPt);

                    // To not multi calc.
                    canPut = Request.AlignLeft ? !(nextPt.X > Status.InsBox.Max.X) :
                        !(nextPt.X < Status.InsBox.Min.X);

                    // It's an invalid point, must return.
                    // Because boxInsPts count is 0, throw new exception.
                    if (!canPut)
                        return nextPt;

                    nextPt = ReviseYValue(nextPt);
                }

                var f1 = Status.InsBox.Max.X - nextPt.X <= batch.Width;
                var f2 = nextPt.X - Status.InsBox.Min.X <= batch.Width;

                // Not calc muilt times.
                Status.IsLastRow = Request.AlignLeft ? f1 : f2;
            }

            // If true, means that put last row and last column, should break.
            if (Status.InsBox.Max.Y - nextPt.Y < batch.Length)
                canPut = false;

            return nextPt;
        }

        private static XYZ CalcNewLinePosition(XYZ nextPt)
        {
            var pt1 = new XYZ(nextPt.X + Request.RowWidth, Status.InsBox.Min.Y, nextPt.Z);
            var pt2 = new XYZ(nextPt.X - Request.RowWidth, Status.InsBox.Min.Y, nextPt.Z);
            var pt3 = new XYZ(nextPt.X, Status.InsBox.Min.Y, nextPt.Z);

            nextPt = Status.RowNum % 2 == 0 ? Request.AlignLeft ? pt1 : pt2 : pt3;
            return nextPt;
        }

        private static XYZ ReviseYValue(XYZ nextPt)
        {
            var line = Line.CreateBound(new XYZ(nextPt.X, -10000000, nextPt.Z), new XYZ(nextPt.X, 10000000, nextPt.Z));
            var roomInsPts = line.GetPlaneInsPointList(Status.RoomEdges);
            var boxInsPts = line.GetPlaneInsPointList(PickEdges);
            var roomInsMinY = roomInsPts.Min(m => m.Y);
            var boxInsMinY = boxInsPts.Min(m => m.Y);

            // Revises the next point y min value.
            nextPt = new XYZ(nextPt.X, Math.Max(roomInsMinY, boxInsMinY) + 0.2, nextPt.Z);

            return nextPt;
        }

        private static List<SeatInfo> GetSeatInfosOnFloor(List<Room> rooms, List<SeatBatch> batches)
        {
            var seatInfos = new List<SeatInfo>();

            foreach (var room in rooms)
            {
                InitRoomStatus(room);

                var nextPt = GetStartPoint();
                var roomEdges = room.GetEdgeList();

                foreach (var batch in batches)
                {
                    // When the next seat location isn't in the room polygon, breaks the loop.
                    if (!CanPut(nextPt, roomEdges))
                        break;

                    // The next room should skips the batch, because the batch seats run out.
                    if (batch.UsableNumber == 0)
                        continue;

                    seatInfos.AddRange(GetSeatInfosOnBatch(roomEdges, batch, ref nextPt));
                }
            }

            return seatInfos;
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

        private static bool CanPut(List<XYZ> pts, List<Line> roomEdges)
        {
            return pts.All(a => CanPut(a, Status.InsEdges)) && pts.All(a => CanPut(a, roomEdges));
        }

        private static bool CanPut(XYZ pt, List<Line> roomEdges)
        {
            return pt.InPlanePolygon(Status.InsEdges) && pt.InPlanePolygon(roomEdges);
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