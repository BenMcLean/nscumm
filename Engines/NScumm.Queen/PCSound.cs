//
//  PCSound.cs
//
//  Author:
//       scemino <scemino74@gmail.com>
//
//  Copyright (c) 2016 scemino
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

using System.IO;
using NScumm.Core.Audio;

namespace NScumm.Queen
{

    abstract class PCSound : Sound
    {
        SoundHandle _sfxHandle;
        SoundHandle _speechHandle;
        //MidiMusic* _music;

        public override bool IsSpeechActive
        {
            get
            {
                return _mixer.IsSoundHandleActive(_speechHandle);
            }
        }

        public PCSound(IMixer mixer, QueenEngine vm)
            : base(mixer, vm)
        {
            // TODO: _music = new MidiMusic(vm);
        }

        public override void PlaySpeech(string @base)
        {
            if (SpeechOn)
            {
                PlaySound(@base, true);
            }
        }

        public override void StopSpeech() { _mixer.StopHandle(_speechHandle); }

        protected void PlaySound(string @base, bool isSpeech)
        {
            // alter filename to add zeros and append ".SB"
            string name = @base.Replace(' ', '0') + ".SB";
            if (isSpeech)
            {
                while (_mixer.IsSoundHandleActive(_speechHandle))
                {
                    _vm.Input.Delay(10);
                }
            }
            else
            {
                _mixer.StopHandle(_sfxHandle);
            }
            uint size;
            var f = _vm.Resource.FindSound(name, out size);
            if (f != null)
            {
                if (isSpeech)
                    PlaySoundData(f, size, ref _speechHandle);
                else
                    PlaySoundData(f, size, ref _sfxHandle);
                _speechSfxExists = isSpeech;
            }
            else
            {
                _speechSfxExists = false;
            }
        }

        protected abstract void PlaySoundData(Stream f, uint size, ref SoundHandle soundHandle);

    }

}
