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
        private static DesignStatus CurrentStatus { get; } = new DesignStatus();

        public static void AutoPutSeat(this Document doc, PickedBox box, PositionRequest request, List<SeatBatch> batches)
        {
            CurrentStatus.Doc = doc;
            CurrentStatus.BoxEdges = box.GetPlaneEdges();
            CurrentStatus.BoxVectors = box.GetPlaneVectors();
            CurrentStatus.Request = request;
            CurrentStatus.ReferenceSeat = doc.GetFloorSeats().FirstOrDefault();
            CurrentStatus.FillPattern = doc.GetFirstFillPattern();

            if (CurrentStatus.ReferenceSeat == null)
            {
                MessageBox.Show("The document has's any seat, please put a seat!");
                return;
            }

            var floorRooms = GetRoomListOnFloor();
            var seatInfos = GetSeatInfosOnFloor(floorRooms, batches);

            CurrentStatus.Doc.AutoTransaction(() =>
            {
                foreach (var seatInfo in seatInfos)
                {
                    var translation = seatInfo.Position - (CurrentStatus.ReferenceSeat.Location as LocationPoint)?.Point;
                    var cloneSeatIds = ElementTransformUtils.CopyElement(CurrentStatus.Doc, CurrentStatus.ReferenceSeat.Id, translation).ToList();

                    if (cloneSeatIds.Count == 0)
                    {
                        MessageBox.Show("Copys the seat, but gets not a seat.");
                        return;
                    }

                    var seat = CurrentStatus.Doc.GetElement(cloneSeatIds[0]);

                    // Sets the seat some parameters.
                    seat.SetSeatParameters(doc, CurrentStatus, seatInfo);

                    // Sets the seat fill color.
                    seat.SetColorFill(CurrentStatus.FillPattern, doc, seatInfo.FillColor);

                    if (!seatInfo.IsRotation)
                        continue;

                    var location = seatInfo.Position;
                    var startPt = new XYZ(location.X, location.Y + seatInfo.SeatLength / 2, 0);
                    var endPt = new XYZ(location.X, location.Y + seatInfo.SeatLength / 2, 1);
                    var line = Line.CreateBound(startPt, endPt);

                    // No use mirror, mirror element is very slow.
                    ElementTransformUtils.RotateElement(CurrentStatus.Doc, seat.Id, line, Math.PI);
                }
            });

            if (!ConformPasswayWidth())
                MessageBox.Show("The passsway width is qualified!");
        }

        private static List<SeatInfo> GetSeatInfosOnFloor(List<Room> rooms, List<SeatBatch> batches)
        {
            var seatInfos = new List<SeatInfo>();

            foreach (var room in rooms)
            {
                CurrentStatus.CurrentRoomBox = room.GetBoundingBox(CurrentStatus.Doc);
                CurrentStatus.CurrentRowNum = 1;
                CurrentStatus.RoomMinX = CurrentStatus.CurrentRoomBox.Min.X;
                CurrentStatus.RoomMaxX = CurrentStatus.CurrentRoomBox.Max.X;
                CurrentStatus.RoomMaxY = CurrentStatus.CurrentRoomBox.Max.Y;
                CurrentStatus.IsLastRow = false;

                var nextPt = GetStartSeatPosition();
                var roomEdges = room.GetRoomEdgeList();

                foreach (var batch in batches)
                {
                    // When the next seat location isn't in the room polygon, breaks the loop.
                    if (!nextPt.InPlanePolygon(roomEdges))
                        break;

                    // The next room should skips the batch, because the batch seats run out.
                    if (batch.UsableNumber == 0)
                        continue;

                    seatInfos.AddRange(GetSeatInfosInRoom(roomEdges, batch, ref nextPt));
                }
            }

            return seatInfos;
        }

        private static XYZ GetStartSeatPosition()
        {
            var roomMin = CurrentStatus.CurrentRoomBox.Min;
            var roomMax = CurrentStatus.CurrentRoomBox.Max;
            var minX = Math.Max(roomMin.X, CurrentStatus.BoxVectors.Min(o => o.X));
            var maxX = Math.Min(roomMax.X, CurrentStatus.BoxVectors.Max(o => o.X));
            var minY = Math.Max(roomMin.Y, CurrentStatus.BoxVectors.Min(o => o.Y));

            // TODO: I should solve the precision problem.
            // The seat location is in left bottom postition.
            return CurrentStatus.Request.AlignLeft ? new XYZ(minX + 0.01, minY + 0.2, roomMin.Z) : new XYZ(maxX - 0.01, minY + 0.2, roomMin.Z);
        }

        private static List<SeatInfo> GetSeatInfosInRoom(List<Line> roomEdges, SeatBatch batch, ref XYZ nextPt)
        {
            var results = new List<SeatInfo>();
            var flag = true;
            var sum = 0;

            CurrentStatus.CurrentY = nextPt.Y;

            while (batch.UsableNumber > 0 && flag)
            {
                sum++;

                // In order to not be an infinite loop.
                if (sum > 1000)
                {
                    MessageBox.Show("The loop is endless, please contact the program's developer!");
                    break;
                }

                var seat = new SeatInfo(nextPt, batch, CurrentStatus.CurrentRowNum)
                {
                    // The seat aligns left and double num or aligns right and single num, should be retated.
                    IsRotation = CurrentStatus.Request.AlignLeft && CurrentStatus.CurrentRowNum % 2 == 0
                        || !CurrentStatus.Request.AlignLeft && CurrentStatus.CurrentRowNum % 2 == 1
                };
                var pts = GetSeatVectors(batch, nextPt, seat);

                if (CanPut(pts, roomEdges))
                {
                    results.Add(seat);
                    batch.UsableNumber -= 1;
                }

                nextPt = GetNextPt(roomEdges, batch, nextPt, ref flag);
            }

            return results;
        }

        private static List<XYZ> GetSeatVectors(SeatBatch batch, XYZ nextPt, SeatInfo seat)
        {
            var leftBottomPt = nextPt;
            var rightBottomPt = new XYZ(nextPt.X + batch.SeatWidth, nextPt.Y, nextPt.Z);
            var leftTopPt = new XYZ(nextPt.X, nextPt.Y + batch.SeatLength, nextPt.Z);
            var rightTopPt = new XYZ(nextPt.X + batch.SeatWidth, nextPt.Y + batch.SeatLength, nextPt.Z);

            if (!seat.IsRotation)
                return new List<XYZ> { leftBottomPt, rightBottomPt, leftTopPt, rightTopPt };

            // If you don't understand it, you should draw picture.
            leftBottomPt = new XYZ(nextPt.X - batch.SeatWidth, nextPt.Y, nextPt.Z);
            rightBottomPt = nextPt;
            leftTopPt = new XYZ(nextPt.X - batch.SeatWidth, nextPt.Y + batch.SeatLength, nextPt.Z);
            rightTopPt = new XYZ(nextPt.X, nextPt.Y + batch.SeatLength, nextPt.Z);

            return new List<XYZ> { leftBottomPt, rightBottomPt, leftTopPt, rightTopPt };
        }

        private static XYZ GetNextPt(List<Line> roomEdges, SeatBatch batch, XYZ nextPt, ref bool flag)
        {
            // The next seat location should set the location's y axis plus seat length.
            nextPt = new XYZ(nextPt.X, nextPt.Y + batch.SeatLength, nextPt.Z);

            if (!CurrentStatus.IsLastRow && CurrentStatus.RoomMaxY - nextPt.Y < batch.SeatLength)
            {
                CurrentStatus.CurrentRowNum++;

                var pt1 = new XYZ(nextPt.X + CurrentStatus.Request.RowWidth, CurrentStatus.CurrentY, nextPt.Z);
                var pt2 = new XYZ(nextPt.X - CurrentStatus.Request.RowWidth, CurrentStatus.CurrentY, nextPt.Z);
                var pt3 = new XYZ(nextPt.X, CurrentStatus.CurrentY, nextPt.Z);

                nextPt = CurrentStatus.CurrentRowNum % 2 == 0 ? CurrentStatus.Request.AlignLeft ? pt1 : pt2 : pt3;

                var line = Line.CreateBound(new XYZ(nextPt.X, -10000000, nextPt.Z), new XYZ(nextPt.X, 10000000, nextPt.Z));

                var roomInsPts = line.GetPlaneInsPointList(roomEdges);
                var boxInsPts = line.GetPlaneInsPointList(CurrentStatus.BoxEdges);

                if (roomInsPts.Count > 0)
                {
                    var roomInsMinY = roomInsPts.Min(m => m.Y);
                    var boxInsMinY = boxInsPts.Min(m => m.Y);

                    // Revises the next point y value.
                    CurrentStatus.CurrentY = Math.Max(roomInsMinY, boxInsMinY) + 0.2;

                    nextPt = new XYZ(nextPt.X, CurrentStatus.CurrentY, nextPt.Z);
                }
            }

            // Not calc muilt times.
            if (!CurrentStatus.IsLastRow)
                CurrentStatus.IsLastRow = CurrentStatus.Request.AlignLeft ? CurrentStatus.RoomMaxX - nextPt.X <= batch.SeatWidth
                    : nextPt.X - CurrentStatus.RoomMinX <= batch.SeatWidth;

            // If puts last row and last column, should break.
            if (CurrentStatus.RoomMaxY - nextPt.Y < batch.SeatLength)
                flag = false;

            return nextPt;
        }

        private static bool CanPut(List<XYZ> pts, List<Line> roomEdges)
        {
            return pts.All(a => CanPut(a, roomEdges));
        }

        private static bool CanPut(XYZ pt, List<Line> roomEdges)
        {
            return pt.InPlanePolygon(roomEdges) && pt.InPlanePolygon(CurrentStatus.BoxEdges);
        }

        private static List<Room> GetRoomListOnFloor()
        {
            var rooms = CurrentStatus.Doc.GetCategoryElements(BuiltInCategory.OST_Rooms, true).Cast<Room>();
            var results = new List<Room>();

            foreach (var room in rooms)
            {
                var roomEdges = room.GetRoomEdgeList();
                var roomVectors = roomEdges.GetDistinctPointList();
                var boxPtInRoom = CurrentStatus.BoxVectors.Any(a => a.InPlanePolygon(roomEdges));
                var roomPtInBox = roomVectors.Any(a => a.InPlanePolygon(CurrentStatus.BoxEdges));

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