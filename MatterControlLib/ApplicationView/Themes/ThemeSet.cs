﻿/*
Copyright (c) 2018, John Lewin
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

1. Redistributions of source code must retain the above copyright notice, this
   list of conditions and the following disclaimer.
2. Redistributions in binary form must reproduce the above copyright notice,
   this list of conditions and the following disclaimer in the documentation
   and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND
ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED
WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR CONTRIBUTORS BE LIABLE FOR
ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES
(INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES;
LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND
ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT
(INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS
SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

The views and conclusions contained in the software and documentation are those
of the authors and should not be interpreted as representing official policies,
either expressed or implied, of the FreeBSD Project.
*/

using System.Collections.Generic;
using MatterHackers.Agg;

namespace MatterHackers.MatterControl
{
	public class ThemeSet
	{
		public static int LatestSchemeVersion { get; } = 20181023;

		public string ThemeID { get; set; }

		public string Name { get; set; }

		public ThemeConfig Theme { get; set; }

		public ThemeConfig MenuTheme { get; set; }

		public List<Color> AccentColors { get; set; } = new List<Color>();

		public int DefaultColorIndex { get; set; }

		public void SetAccentColor(Color accentColor)
		{
			this.Theme.PrimaryAccentColor = accentColor;
			this.Theme.AccentMimimalOverlay = accentColor.WithAlpha(90);

			this.MenuTheme.PrimaryAccentColor = accentColor;
			this.MenuTheme.AccentMimimalOverlay = accentColor.WithAlpha(90);
		}

		/// <summary>
		/// The latest version of the theme file format. When changed, the clients state becomes invalid and will require a reset to the new theme format
		/// </summary>
		public int SchemeVersion { get; set; }
	}
}