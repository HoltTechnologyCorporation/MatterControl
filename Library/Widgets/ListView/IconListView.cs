﻿/*
Copyright (c) 2017, John Lewin
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

using System;
using MatterHackers.Agg;
using MatterHackers.Agg.UI;
using MatterHackers.MatterControl.Library;
using MatterHackers.VectorMath;

namespace MatterHackers.MatterControl.CustomWidgets
{
	public class IconListView : FlowLayoutWidget, IListContentView
	{
		public int ThumbWidth { get; set; } = 100;
		public int ThumbHeight { get; set; } = 100;

		private FlowLayoutWidget rowButtonContainer = null;

		private int cellIndex = 0;
		private int columnCount = 1;
		private int itemRightMargin;

		public IconListView()
			: base(FlowDirection.TopToBottom)
		{
		}

		public override void OnBoundsChanged(EventArgs e)
		{
			int padding = 4;
			int itemWidth = ThumbWidth + (padding * 2);

			columnCount = (int)Math.Floor(this.LocalBounds.Width / itemWidth);

			int remainingSpace = (int) this.LocalBounds.Width - columnCount * itemWidth;

			itemRightMargin = (columnCount <= 0) ? 0 : remainingSpace / (columnCount + 1);

			this.Padding = new BorderDouble(0, itemRightMargin);

			base.OnBoundsChanged(e);
		}

		public void AddItem(ListViewItem item)
		{
			var iconView = new IconViewItem(item, this.ThumbWidth, this.ThumbHeight);
			iconView.Margin = new BorderDouble(0, 0, itemRightMargin, 0);
			item.ViewWidget = iconView;

			if (rowButtonContainer == null)
			{
				rowButtonContainer = new FlowLayoutWidget(FlowDirection.LeftToRight)
				{
					HAnchor = HAnchor.ParentLeftRight,
					Padding = new BorderDouble(itemRightMargin, 0, 0, 0)
				};
				rowButtonContainer.AddChild(iconView);
				this.AddChild(rowButtonContainer);
				cellIndex = 1;
			}
			else
			{
				rowButtonContainer.AddChild(iconView);

				if (cellIndex++ >= columnCount - 1)
				{
					rowButtonContainer = null;
				}
			}
		}

		public void ClearItems()
		{
			rowButtonContainer = null;
		}
	}

	public class IconViewItem : ListViewItemBase
	{
		public IconViewItem(ListViewItem item, int thumbWidth, int thumbHeight)
			: base(item, thumbWidth, thumbHeight)
		{
			this.VAnchor = VAnchor.FitToChildren;
			this.HAnchor = HAnchor.FitToChildren;
			this.Padding = 4;
			this.Margin = new BorderDouble(6, 0, 0, 6);

			var container = new FlowLayoutWidget(FlowDirection.TopToBottom);
			this.AddChild(container);

			imageWidget = new ImageWidget(thumbWidth, thumbHeight)
			{
				AutoResize = false,
				Name = "List Item Thumbnail",
				BackgroundColor = item.ListView.ThumbnailBackground,
				Margin = 0,
			};

			imageWidget.Click += (sender, e) =>
			{
				this.OnItemSelect();
			};

			container.AddChild(imageWidget);

			int maxWidth = thumbWidth - 4;

			var text = new TextWidget(item.Model.Name, 0, 0, 9, textColor: ActiveTheme.Instance.PrimaryTextColor)
			{
				AutoExpandBoundsToText = false,
				EllipsisIfClipped = true,
				HAnchor = HAnchor.ParentCenter,
				Margin = new BorderDouble(0, 0, 0, 3),
			};

			text.MaximumSize = new Vector2(maxWidth, 20);
			if (text.Printer.LocalBounds.Width > maxWidth)
			{
				text.Width = maxWidth;
				text.Text = item.Model.Name;
			}

			container.AddChild(text);
		}

		public override async void OnLoad(EventArgs args)
		{
			base.OnLoad(args);
			await this.LoadItemThumbnail();
		}

		public override RGBA_Bytes BackgroundColor
		{
			get
			{
				return this.IsSelected ? ActiveTheme.Instance.PrimaryAccentColor : RGBA_Bytes.Transparent;
			}
			set { }
		}
	}
}