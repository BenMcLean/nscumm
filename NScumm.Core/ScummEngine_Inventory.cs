﻿//
//  ScummEngine_Inventory.cs
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

using System;
using System.Linq;

namespace NScumm.Core
{
    partial class ScummEngine
    {
        protected ushort[] _inventory = new ushort[NumInventory];
        protected ObjectData[] _invData = new ObjectData[NumInventory];

        int GetInventorySlot()
        {
            for (var i = 0; i < NumInventory; i++)
            {
                if (_inventory[i] == 0)
                    return i;
            }
            return -1;
        }

        protected void AddObjectToInventory(int obj, byte room)
        {
            var slot = GetInventorySlot();
            if (GetWhereIsObject(obj) == WhereIsObject.FLObject)
            {
                GetObjectIndex(obj);
                throw new NotImplementedException();
            }
            else
            {
                var objs = _resManager.GetRoom(room).Objects;
                var objFound = (from o in objs
                                            where o.Number == obj
                                            select o).FirstOrDefault();
                _invData[slot] = objFound;
            }
            _inventory[slot] = (ushort)obj;
        }

        protected int GetInventoryCountCore(int owner)
        {
            var count = 0;
            for (var i = 0; i < NumInventory; i++)
            {
                var obj = _inventory[i];
                if (obj != 0 && GetOwnerCore(obj) == owner)
                    count++;
            }
            return count;
        }

        protected int FindInventoryCore(int owner, int idx)
        {
            int count = 1, i, obj;
            for (i = 0; i < NumInventory; i++)
            {
                obj = _inventory[i];
                if (obj != 0 && GetOwnerCore(obj) == owner && count++ == idx)
                    return obj;
            }
            return 0;
        }
    }
}

