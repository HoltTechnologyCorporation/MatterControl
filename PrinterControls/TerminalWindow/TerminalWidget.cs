﻿/*
Copyright (c) 2014, Lars Brubaker
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
using System.Collections.Generic;
using System.Diagnostics;
using MatterHackers.Agg;
using MatterHackers.Agg.Platform;
using MatterHackers.Agg.UI;
using MatterHackers.Localizations;
using MatterHackers.MatterControl.CustomWidgets;
using MatterHackers.MatterControl.PrinterCommunication;

namespace MatterHackers.MatterControl
{
	public class TerminalWidget : GuiWidget
	{
		private CheckBox filterOutput;
		private CheckBox autoUppercase;
		private MHTextEditWidget manualCommandTextEdit;
		private TextScrollWidget textScrollWidget;

		private static readonly string TerminalFilterOutputKey = "TerminalFilterOutput";
		private static readonly string TerminalAutoUppercaseKey = "TerminalAutoUppercase";

		public TerminalWidget()
		{
			this.Name = "TerminalWidget";
			this.BackgroundColor = ActiveTheme.Instance.PrimaryBackgroundColor;
			this.Padding = new BorderDouble(5, 0);
			FlowLayoutWidget topLeftToRightLayout = new FlowLayoutWidget();
			topLeftToRightLayout.AnchorAll();

			{
				FlowLayoutWidget manualEntryTopToBottomLayout = new FlowLayoutWidget(FlowDirection.TopToBottom);
				manualEntryTopToBottomLayout.VAnchor |= Agg.UI.VAnchor.Top;
				manualEntryTopToBottomLayout.Padding = new BorderDouble(top: 8);

				{
					FlowLayoutWidget topBarControls = new FlowLayoutWidget(FlowDirection.LeftToRight);
					topBarControls.HAnchor |= HAnchor.Left;

					{
						filterOutput = new CheckBox("Filter Output".Localize())
						{
							Margin = new BorderDouble(5, 5, 5, 2),
							TextColor = ActiveTheme.Instance.PrimaryTextColor,
							VAnchor = Agg.UI.VAnchor.Bottom,
						};
						filterOutput.CheckedStateChanged += (object sender, EventArgs e) =>
						{
							if (filterOutput.Checked)
							{
								textScrollWidget.SetLineStartFilter(new string[] { "<-wait", "<-ok", "<-T" });
							}
							else
							{
								textScrollWidget.SetLineStartFilter(null);
							}

							UserSettings.Instance.Fields.SetBool(TerminalFilterOutputKey, filterOutput.Checked);
						};

						topBarControls.AddChild(filterOutput);
					}

					{
						autoUppercase = new CheckBox("Auto Uppercase".Localize());
						autoUppercase.Margin = new BorderDouble(5, 5, 5, 2);
						autoUppercase.Checked = UserSettings.Instance.Fields.GetBool(TerminalAutoUppercaseKey, true);
						autoUppercase.TextColor = ActiveTheme.Instance.PrimaryTextColor; ;
						autoUppercase.VAnchor = Agg.UI.VAnchor.Bottom;
						topBarControls.AddChild(autoUppercase);
						autoUppercase.CheckedStateChanged += (sender, e) =>
						{
							UserSettings.Instance.Fields.SetBool(TerminalAutoUppercaseKey, autoUppercase.Checked);
						};
						manualEntryTopToBottomLayout.AddChild(topBarControls);
					}
				}

				{
					FlowLayoutWidget leftToRight = new FlowLayoutWidget();
					leftToRight.AnchorAll();

					textScrollWidget = new TextScrollWidget(PrinterConnection.Instance.TerminalLog.PrinterLines);
					//outputScrollWidget.Height = 100;
					Debug.WriteLine(PrinterConnection.Instance.TerminalLog.PrinterLines);
					textScrollWidget.BackgroundColor = ActiveTheme.Instance.SecondaryBackgroundColor;
					textScrollWidget.TextColor = ActiveTheme.Instance.PrimaryTextColor;
					textScrollWidget.HAnchor = HAnchor.Stretch;
					textScrollWidget.VAnchor = VAnchor.Stretch;
					textScrollWidget.Margin = new BorderDouble(0, 5);
					textScrollWidget.Padding = new BorderDouble(3, 0);

					leftToRight.AddChild(textScrollWidget);

					TextScrollBar textScrollBar = new TextScrollBar(textScrollWidget, 15);
					leftToRight.AddChild(textScrollBar);

					manualEntryTopToBottomLayout.AddChild(leftToRight);
				}

				FlowLayoutWidget manualEntryLayout = new FlowLayoutWidget(FlowDirection.LeftToRight);
				manualEntryLayout.BackgroundColor = this.BackgroundColor;
				manualEntryLayout.HAnchor = HAnchor.Stretch;
				{
					manualCommandTextEdit = new MHTextEditWidget("", typeFace: ApplicationController.MonoSpacedTypeFace);
					//manualCommandTextEdit.BackgroundColor = RGBA_Bytes.White;
					manualCommandTextEdit.Margin = new BorderDouble(right: 3);
					manualCommandTextEdit.HAnchor = HAnchor.Stretch;
					manualCommandTextEdit.VAnchor = VAnchor.Bottom;
					manualCommandTextEdit.ActualTextEditWidget.EnterPressed += manualCommandTextEdit_EnterPressed;
					manualCommandTextEdit.ActualTextEditWidget.KeyDown += manualCommandTextEdit_KeyDown;
					manualEntryLayout.AddChild(manualCommandTextEdit);
				}

				var controlButtonFactory = ApplicationController.Instance.Theme.ButtonFactory;

				manualEntryTopToBottomLayout.AddChild(manualEntryLayout);

				Button clearConsoleButton = controlButtonFactory.Generate("Clear".Localize());
				clearConsoleButton.Margin = new BorderDouble(0);
				clearConsoleButton.Click += (sender, e) =>
				{
					PrinterConnection.Instance.TerminalLog.Clear();
				};

				//Output Console text to screen
				Button exportConsoleTextButton = controlButtonFactory.Generate("Export".Localize() + "...");
				exportConsoleTextButton.Click += (sender, mouseEvent) =>
				{
					UiThread.RunOnIdle(() =>
					{
						AggContext.FileDialogs.SaveFileDialog(
							new SaveFileDialogParams("Save as Text|*.txt")
							{
								Title = "MatterControl: Terminal Log",
								ActionButtonLabel = "Export",
								FileName = "print_log.txt"
							},
							(saveParams) =>
							{
								if (!string.IsNullOrEmpty(saveParams.FileName))
								{
									string filePathToSave = saveParams.FileName;
									if (filePathToSave != null && filePathToSave != "")
									{
										try
										{
											textScrollWidget.WriteToFile(filePathToSave);
										}
										catch (UnauthorizedAccessException e)
										{
											Debug.Print(e.Message);

											PrinterConnection.Instance.TerminalLog.PrinterLines.Add("");
											PrinterConnection.Instance.TerminalLog.PrinterLines.Add(writeFaildeWaring);
											PrinterConnection.Instance.TerminalLog.PrinterLines.Add(cantAccessPath.FormatWith(filePathToSave));
											PrinterConnection.Instance.TerminalLog.PrinterLines.Add("");

											UiThread.RunOnIdle(() =>
											{
												StyledMessageBox.ShowMessageBox(null, e.Message, "Couldn't save file".Localize());
											});
										}
									}
								}
							});
					});
				};

				var sendCommand = controlButtonFactory.Generate("Send".Localize());
				sendCommand.Click += sendManualCommandToPrinter_Click;

				FlowLayoutWidget bottomRowContainer = new FlowLayoutWidget();
				bottomRowContainer.HAnchor = Agg.UI.HAnchor.Stretch;
				bottomRowContainer.Margin = new BorderDouble(0, 3);

				bottomRowContainer.AddChild(sendCommand);
				bottomRowContainer.AddChild(clearConsoleButton);
				bottomRowContainer.AddChild(exportConsoleTextButton);
				bottomRowContainer.AddChild(new HorizontalSpacer());

				manualEntryTopToBottomLayout.AddChild(bottomRowContainer);
				manualEntryTopToBottomLayout.AnchorAll();

				topLeftToRightLayout.AddChild(manualEntryTopToBottomLayout);
			}

			AddChild(topLeftToRightLayout);
			this.AnchorAll();
		}

#if !__ANDROID__
		public override void OnLoad(EventArgs args)
		{
			filterOutput.Checked = UserSettings.Instance.Fields.GetBool(TerminalFilterOutputKey, false);
			UiThread.RunOnIdle(manualCommandTextEdit.Focus);
			base.OnLoad(args);
		}
#endif

		string writeFaildeWaring = "WARNING: Write Failed!".Localize();
		string cantAccessPath = "Can't access '{0}'.".Localize();

		private List<string> commandHistory = new List<string>();
		private int commandHistoryIndex = 0;

		private void manualCommandTextEdit_KeyDown(object sender, KeyEventArgs keyEvent)
		{
			bool changeToHistory = false;
			if (keyEvent.KeyCode == Keys.Up)
			{
				commandHistoryIndex--;
				if (commandHistoryIndex < 0)
				{
					commandHistoryIndex = 0;
				}
				changeToHistory = true;
			}
			else if (keyEvent.KeyCode == Keys.Down)
			{
				commandHistoryIndex++;
				if (commandHistoryIndex > commandHistory.Count - 1)
				{
					commandHistoryIndex = commandHistory.Count - 1;
				}
				else
				{
					changeToHistory = true;
				}
			}
			else if (keyEvent.KeyCode == Keys.Escape)
			{
				manualCommandTextEdit.Text = "";
			}

			if (changeToHistory && commandHistory.Count > 0)
			{
				manualCommandTextEdit.Text = commandHistory[commandHistoryIndex];
			}
		}

		private void manualCommandTextEdit_EnterPressed(object sender, KeyEventArgs keyEvent)
		{
			sendManualCommandToPrinter_Click(null, null);
		}

		private void sendManualCommandToPrinter_Click(object sender, EventArgs mouseEvent)
		{
			string textToSend = manualCommandTextEdit.Text.Trim();
			if (autoUppercase.Checked)
			{
				textToSend = textToSend.ToUpper();
			}
			commandHistory.Add(textToSend);
			commandHistoryIndex = commandHistory.Count;
			PrinterConnection.Instance.SendLineToPrinterNow(textToSend);
			manualCommandTextEdit.Text = "";
		}
	}
}
