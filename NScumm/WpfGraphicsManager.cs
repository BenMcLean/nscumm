﻿/*
 * This file is part of NScumm.
 * 
 * NScumm is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 * 
 * NScumm is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 * 
 * You should have received a copy of the GNU General Public License
 * along with NScumm.  If not, see <http://www.gnu.org/licenses/>.
 */

using Scumm4.Graphics;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace NScumm
{
    public class WpfGraphicsManager : DispatcherObject, IGraphicsManager
    {
        private Image _elt;
        private WriteableBitmap _bmp;

        public WpfGraphicsManager(Image elt)
        {
            var colors = new Color[256];
            _bmp = new WriteableBitmap(320, 200, 96, 96, PixelFormats.Indexed8, new BitmapPalette(colors));
            _elt = elt;
            this.Width = _elt.ActualWidth;
            this.Height = _elt.ActualHeight;
            _elt.SizeChanged += OnSizeChanged;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.Width = _elt.ActualWidth;
            this.Height = _elt.ActualHeight;
        }

        public double Width
        {
            get;
            private set;
        }

        public double Height
        {
            get;
            private set;
        }

        public void SetPalette(Color[] colors)
        {
            this.Dispatcher.Invoke(new Action(() =>
            {
                _bmp = new WriteableBitmap(320, 200, 96, 96, PixelFormats.Indexed8, new BitmapPalette(colors));
                _elt.Source = _bmp;
            }));
        }

        public Scumm4.Point GetMousePosition()
        {
            return (Scumm4.Point)this.Dispatcher.Invoke(new Func<Scumm4.Point>(() =>
            {
                var pos = Mouse.GetPosition(_elt);
                return new Scumm4.Point((short)pos.X, (short)pos.Y);
            }));
        }

        public void UpdateScreen()
        {
            
        }

        public void CopyRectToScreen(Array buf, int sourceStride, int x, int y, int width, int height)
        {
            if (height == 0) return;
            if (this.Dispatcher.CheckAccess())
            {
                CopyRectToScreenCore(buf, sourceStride, x, y, width, height);
            }
            else
            {
                this.Dispatcher.Invoke(new Action<Array, int, int, int, int, int>(CopyRectToScreenCore),
                    buf, sourceStride, x, y, width, height);
            }
        }

        private void CopyRectToScreenCore(Array buf, int sourceStride, int x, int y, int width, int height)
        {
            _bmp.WritePixels(new Int32Rect(x, y, width, height), buf, sourceStride, x, y);
        }
    }
}
