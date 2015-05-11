﻿//
//  ScummEngine4.cs
//
//  Author:
//       Scemino <scemino74@gmail.com>
//
//  Copyright (c) 2014 
//
//  This program is free software: you can redistribute it and/or modify
//  it under the terms of the GNU General Public License as published by
//  the Free Software Foundation, either version 3 of the License, or
//  (at your option) any later version.
//
//  This program is distributed in the hope that it will be useful,
//  but WITHOUT ANY WARRANTY; without even the implied warranty of
//  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//  GNU General Public License for more details.
//
//  You should have received a copy of the GNU General Public License
//  along with this program.  If not, see <http://www.gnu.org/licenses/>.

using NScumm.Core.Graphics;
using NScumm.Core.Input;
using System.Collections.Generic;
using NScumm.Core.IO;
using NScumm.Core.Audio;

namespace NScumm.Core
{
    public class ScummEngine4: ScummEngine3
    {
        public ScummEngine4(GameSettings game, IGraphicsManager graphicsManager, IInputManager inputManager, IMixer mixer)
            : base(game, graphicsManager, inputManager, mixer)
        {
        }

        protected override void SetupVars()
        {
            base.SetupVars();

            VariableScrollScript = 27;
            VariableDebugMode = 39;
            VariableMainMenu = 50;
            VariableFixedDisk = 51;
            VariableCursorState = 52;
            VariableUserPut = 53;
            VariableTalkStringY = 54;

            if ((Game.GameId == GameId.Loom && Game.Version == 4) || Game.Version >= 5)
            {
                VariableNoSubtitles = 60;
            }
        }

        protected override void ResetScummVars()
        {
            base.ResetScummVars();

            if (Game.Version >= 4 && Game.Version <= 5)
                Variables[VariableTalkStringY.Value] = -0x50;
        }

        protected override void InitOpCodes()
        {
            base.InitOpCodes();

            _opCodes[0x30] = MatrixOperations;
            _opCodes[0xB0] = MatrixOperations;
            _opCodes[0x3B] = GetActorScale;
            _opCodes[0xBB] = GetActorScale;
            _opCodes[0x4C] = SoundKludge;
        }

        protected void GetActorScale()
        {
            GetResult();
            var act = GetVarOrDirectByte(OpCodeParameter.Param1);
            var actor = Actors[act];
            SetResult((int)actor.ScaleX);
        }

        protected void SoundKludge()
        {
            var items = GetWordVarArgs();
            Sound.SoundKludge(items);
        }

        void MatrixOperations()
        {
            int a, b;

            _opCode = ReadByte();
            switch (_opCode & 0x1F)
            {
                case 1:
                    a = GetVarOrDirectByte(OpCodeParameter.Param1);
                    b = GetVarOrDirectByte(OpCodeParameter.Param2);
                    SetBoxFlags(a, b);
                    break;

                case 2:
                    a = GetVarOrDirectByte(OpCodeParameter.Param1);
                    b = GetVarOrDirectByte(OpCodeParameter.Param2);
                    SetBoxScale(a, b);
                    break;

                case 3:
                    a = GetVarOrDirectByte(OpCodeParameter.Param1);
                    b = GetVarOrDirectByte(OpCodeParameter.Param2);
                    SetBoxScale(a, (b - 1) | 0x8000);
                    break;

                case 4:
                    CreateBoxMatrixCore();
                    break;
            }
        }

        void SetBoxScale(int box, int scale)
        {
            var b = GetBoxBase(box);
            b.Scale = (ushort)scale;
        }

        protected void CreateBoxMatrixCore()
        {
            // The total number of boxes
            var num = GetNumBoxes();

            // calculate shortest paths
            var itineraryMatrix = CalcItineraryMatrix(num);

            // "Compress" the distance matrix into the box matrix format used
            // by the engine. The format is like this:
            // For each box (from 0 to num) there is first a byte with value 0xFF,
            // followed by an arbitrary number of byte triples; the end is marked
            // again by the lead 0xFF for the next "row". The meaning of the
            // byte triples is as follows: the first two bytes define a range
            // of box numbers (e.g. 7-11), while the third byte defines an
            // itineray box. Assuming we are in the 5th "row" and encounter
            // the triplet 7,11,15: this means to get from box 5 to any of
            // the boxes 7,8,9,10,11 the shortest way is to go via box 15.
            // See also getNextBox.

            var boxMatrix = new List<byte>();

            for (byte i = 0; i < num; i++)
            {
                boxMatrix.Add(0xFF);
                for (byte j = 0; j < num; j++)
                {
                    var itinerary = itineraryMatrix[i, j];
                    if (itinerary != InvalidBox)
                    {
                        boxMatrix.Add(j);
                        while (j < num - 1 && itinerary == itineraryMatrix[i, (j + 1)])
                            j++;
                        boxMatrix.Add(j);
                        boxMatrix.Add(itinerary);
                    }
                }
            }
            boxMatrix.Add(0xFF);

            _boxMatrix.Clear();
            _boxMatrix.AddRange(boxMatrix);
        }


        byte[,] CalcItineraryMatrix(int num)
        {
            const byte boxSize = 64;

            // Allocate the adjacent & itinerary matrices
            var itineraryMatrix = new byte[boxSize, boxSize];
            var adjacentMatrix = new byte[boxSize, boxSize];

            // Initialize the adjacent matrix: each box has distance 0 to itself,
            // and distance 1 to its direct neighbors. Initially, it has distance
            // 255 (= infinity) to all other boxes.
            for (byte i = 0; i < num; i++)
            {
                for (byte j = 0; j < num; j++)
                {
                    if (i == j)
                    {
                        adjacentMatrix[i, j] = 0;
                        itineraryMatrix[i, j] = j;
                    }
                    else if (AreBoxesNeighbors(i, j))
                    {
                        adjacentMatrix[i, j] = 1;
                        itineraryMatrix[i, j] = j;
                    }
                    else
                    {
                        adjacentMatrix[i, j] = 255;
                        itineraryMatrix[i, j] = InvalidBox;
                    }
                }
            }

            // Compute the shortest routes between boxes via Kleene's algorithm.
            // The original code used some kind of mangled Dijkstra's algorithm;
            // while that might in theory be slightly faster, it was
            // a) extremly obfuscated
            // b) incorrect: it didn't always find the shortest paths
            // c) not any faster in reality for our sparse & small adjacent matrices
            for (byte k = 0; k < num; k++)
            {
                for (byte i = 0; i < num; i++)
                {
                    for (byte j = 0; j < num; j++)
                    {
                        if (i == j)
                            continue;
                        byte distIK = adjacentMatrix[i, k];
                        byte distKJ = adjacentMatrix[k, j];
                        if (adjacentMatrix[i, j] > distIK + distKJ)
                        {
                            adjacentMatrix[i, j] = (byte)(distIK + distKJ);
                            itineraryMatrix[i, j] = itineraryMatrix[i, k];
                        }
                    }
                }
            }

            return itineraryMatrix;
        }
    }
}

