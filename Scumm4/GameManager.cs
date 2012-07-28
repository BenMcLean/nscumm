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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;
using System.Globalization;

namespace Scumm4
{
    public class GameInfo
    {
        public string Path { get; set; }
        public string Id { get; set; }
        public string Variant { get; set; }
        public string Description { get; set; }
        public int Version { get; set; }
        public CultureInfo Culture { get; set; }
    }

    public static class GameManager
    {
        private static XDocument _doc;
        private static readonly XNamespace Namespace = "http://schemas.scemino.com/nscumm/2012/";

        static GameManager()
        {
            using (var stream = typeof(GameManager).Assembly.GetManifestResourceStream("Scumm4.Nscumm.xml"))
            {
                _doc = System.Xml.Linq.XDocument.Load(stream);
            }
        }

        public static GameInfo GetInfo(string path)
        {
            GameInfo info = null;
            var signature = GetSignature(path);
            var gameMd5 = (from md5 in _doc.Element(Namespace + "NScumm").Elements(Namespace + "MD5")
                           where (string)md5.Attribute("signature") == signature
                           select md5).FirstOrDefault();
            if (gameMd5 != null)
            {
                var game = (from g in _doc.Element(Namespace + "NScumm").Elements(Namespace + "Game")
                            where (string)g.Attribute("id") == (string)gameMd5.Attribute("gameId")
                            where (string)g.Attribute("variant") == (string)gameMd5.Attribute("variant")
                            select g).FirstOrDefault();
                var desc = (from d in _doc.Element(Namespace + "NScumm").Elements(Namespace + "Description")
                            where (string)d.Attribute("gameId") == (string)gameMd5.Attribute("gameId")
                            select (string)d.Attribute("text")).FirstOrDefault();
                info = new GameInfo
                {
                    Path = path,
                    Id = (string)game.Attribute("id"),
                    Variant = (string)game.Attribute("variant"),
                    Description = desc,
                    Version = (int)game.Attribute("version"),
                    Culture = CultureInfo.GetCultureInfo((string)gameMd5.Attribute("language"))
                };
            }
            return info;
        }

        private static string GetSignature(string path)
        {
            string signature;
            using (var md5 = System.Security.Cryptography.MD5.Create())
            {
                using (var file = File.OpenRead(path))
                using (var br = new BinaryReader(file))
                {
                    var data = br.ReadBytes(1024 * 1024);
                    var md5Key = md5.ComputeHash(data, 0, data.Length);
                    var md5Text = new StringBuilder();
                    for (int i = 0; i < 16; i++)
                    {
                        md5Text.AppendFormat("{0:x2}", md5Key[i]);
                    }
                    signature = md5Text.ToString();
                }
            }
            return signature;
        }
    }
}
