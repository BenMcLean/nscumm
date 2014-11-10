﻿//
//  ScummEngine3_Camera.cs
//
//  Author:
//       scemino <scemino74@gmail.com>
//
//  Copyright (c) 2014 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU Lesser General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU Lesser General Public License for more details.
//
//  You should have received a copy of the GNU Lesser General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using NScumm.Core.Graphics;
using System;

namespace NScumm.Core
{
    partial class ScummEngine3
    {
        void PanCameraTo()
        {
            PanCameraToCore(GetVarOrDirectWord(OpCodeParameter.Param1));
        }

        void SetCameraAt()
        {
            short at = (short)GetVarOrDirectWord(OpCodeParameter.Param1);
            Camera.Mode = CameraMode.Normal;
            Camera.CurrentPosition.X = at;
            SetCameraAt(new Point(at, 0));
            Camera.MovingToActor = false;
        }

        void ActorFollowCamera()
        {
            var actor = GetVarOrDirectByte(OpCodeParameter.Param1);
            var old = Camera.ActorToFollow;
            SetCameraFollows(_actors[actor], false);

            if (Camera.ActorToFollow != old)
                RunInventoryScript(0);

            Camera.MovingToActor = false;
        }
    }
}
