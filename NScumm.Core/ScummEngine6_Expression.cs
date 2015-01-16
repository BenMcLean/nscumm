﻿//
//  ScummEngine6_Expression.cs
//
//  Author:
//       scemino <scemino74@gmail.com>
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
using System;
using System.Linq;

namespace NScumm.Core
{
    partial class ScummEngine6
    {
        [OpCode(0x0c)]
        protected void Dup(int value)
        {
            Push(value);
            Push(value);
        }

        [OpCode(0x0d)]
        protected void Not(int value)
        {
            Push(value == 0);
        }

        [OpCode(0x0e)]
        protected void Eq(int a, int b)
        {
            Push(a == b);
        }

        [OpCode(0x0f)]
        protected void NEq(int a, int b)
        {
            Push(a != b);
        }

        [OpCode(0x14)]
        protected void Add(int a, int b)
        {
            Push(a + b);
        }

        [OpCode(0x15)]
        protected void Sub(int a, int b)
        {
            Push(a - b);
        }

        [OpCode(0x16)]
        protected void Mul(int a, int b)
        {
            Push(a * b);
        }

        [OpCode(0x17)]
        protected void Div(int a, int b)
        {
            Push(a / b);
        }

        [OpCode(0x18)]
        protected void Land(int a, int b)
        {
            Push((a != 0) && (b != 0));
        }

        [OpCode(0x19)]
        protected void Lor(int a, int b)
        {
            Push((a != 0) || (b != 0));
        }

        [OpCode(0x1a)]
        protected void Pop(int a)
        {
        }

        [OpCode(0x5c)]
        protected void If(int condition)
        {
            if (condition != 0)
                Jump();
            else
                ReadWordSigned();
        }

        [OpCode(0x5d)]
        protected void IfNot(int condition)
        {
            if (condition == 0)
                Jump();
            else
                ReadWordSigned();
        }

        [OpCode(0x10)]
        protected void Gt(int a, int b)
        {
            Push(a > b);
        }

        [OpCode(0x11)]
        protected void Lt(int a, int b)
        {
            Push(a < b);
        }

        [OpCode(0x12)]
        protected void Le(int a, int b)
        {
            Push(a <= b);
        }

        [OpCode(0x13)]
        protected void Ge(int a, int b)
        {
            Push(a >= b);
        }

        [OpCode(0x73)]
        protected void Jump()
        {
            var offset = ReadWordSigned();

            // WORKAROUND bug #2826144: Talking to the guard at the bigfoot party, after
            // he's let you inside, will cause the game to hang, if you end the conversation.
            // This is a script bug, due to a missing jump in one segment of the script.
            if (Game.Id == "samnmax" && Slots[CurrentScript].Number == 101 && ReadVariable(0x8000 + 97) == 1 && offset == 1)
            {
                offset = -18;
            }

            CurrentPos += offset;
        }

        [OpCode(0xad)]
        protected void IsAnyOf(int value, int[] args)
        {
            Push(args.Any(v => v == value));
        }

        [OpCode(0xc4)]
        protected void Abs(int value)
        {
            Push(Math.Abs(value));
        }

        [OpCode(0xd6)]
        protected void BAnd(int a, int b)
        {
            Push(a & b);
        }

        [OpCode(0xd7)]
        protected void Bor(int a, int b)
        {
            Push(a | b);
        }
    }
}

