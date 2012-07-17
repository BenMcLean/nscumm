﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Scumm4;

namespace CostumeViewer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Costume _cost;
        private CostumeAnimation _anim;
        private WriteableBitmap _bmp;
        private ScummIndex _index;

        public MainWindow()
        {
            InitializeComponent();

            _index = new ScummIndex();
            _index.LoadIndex(@"E:\Program Files (x86)\ScummVM\Games\monkey1vga\000.lfl");
            sliderCost.Minimum = 0;
            sliderCost.Maximum = 0x7D;
            sliderCost.Value = 1;
        }

        private void slider1_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _anim = _cost.Animations[(int)slider1.Value];
            CostumeAnimationLimb frame = null;
            if (_anim != null)
            {
                frame = _anim.Limbs.FirstOrDefault(f => f != null);
            }

            if (frame != null)
            {
                slider2.IsEnabled = true;
                slider2.Minimum = 0;
                slider2.Maximum = _anim.Limbs.Max(f => (f != null) ? f.End - f.Start : 0);
                slider2.Value = slider2.Minimum;
            }
            else
            {
                slider2.IsEnabled = false;
                slider2.Value = 0;
                slider2.Minimum = 0;
                slider2.Maximum = 0;
            }
            UpdatePicture();
        }

        private void slider2_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            UpdatePicture();
        }

        private void UpdatePicture()
        {
            var data = new byte[320 * 200];
            _bmp.WritePixels(new Int32Rect(0, 0, 320, 200), data, 320, 0);
            for (int i = 0; i < 16; i++)
            {
                var limb = _anim != null ? _anim.Limbs[i] : null;
                var num = (int)slider2.Value;
                if (limb != null && limb.Start != 0xFFFF && (limb.Start + num) <= limb.End && limb.Pictures.Count > num)
                {
                    var pict = limb.Pictures[num];
                    _bmp.WritePixels(new Int32Rect(160 + pict.RelX, 100 + pict.RelY, pict.Width, pict.Height), pict.Data, pict.Width, 0);
                    image1.Source = _bmp;
                }
            }
        }

        private void sliderCost_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            try
            {
                _cost = _index.GetCostume((byte)sliderCost.Value);
                if (_cost != null)
                {
                    var room = _index.GetRoom(_cost.Room);
                    _bmp = new WriteableBitmap(320, 200, 96, 96, PixelFormats.Indexed8, new BitmapPalette(room.Palette.Colors));
                    slider1.IsEnabled = _cost.Animations.Length > 0;
                    slider1.Minimum = 0;
                    slider1.Maximum = _cost.Animations.Length - 1;
                    slider1.Value = 0;
                    _anim = (from anim in _cost.Animations
                            where anim != null
                            select anim).FirstOrDefault();
                    slider2.IsEnabled = _anim != null && _anim.Limbs.Count > 0;
                    slider2.Minimum = 0;
                    slider2.Maximum = _anim != null ? _anim.Limbs.Count - 1 : 0;
                    slider2.Value = 0;
                }
            }
            catch (Exception) { }

            if (_cost == null)
            {
                slider1.IsEnabled = slider2.IsEnabled = false;
            }
        }
    }
}

