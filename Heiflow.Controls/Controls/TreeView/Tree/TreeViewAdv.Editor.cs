﻿//
// The Visual HEIFLOW License
//
// Copyright (c) 2015-2018 Yong Tian, SUSTech, Shenzhen, China. All rights reserved.
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights to
// use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of
// the Software, and to permit persons to whom the Software is furnished to do
// so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in all
// copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
//
// Note: only part of the files distributed in the software belong to the Visual HEIFLOW. 
// The software also contains contributed files, which may have their own copyright notices.
//  If not, the GNU General Public License holds for them, too, but so that the author(s) 
// of the file have the Copyright.

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Heiflow.Controls.Tree.NodeControls;
using System.Drawing;

namespace Heiflow.Controls.Tree
{
	partial class TreeViewAdv
	{
		private TreeNodeAdv _editingNode;

		public EditableControl CurrentEditorOwner { get; private set; }
		public Control CurrentEditor { get; private set; }

		public void HideEditor()
		{
			if (CurrentEditorOwner != null)
				CurrentEditorOwner.EndEdit(false);
		}

		internal void DisplayEditor(Control editor, EditableControl owner)
		{
			if (editor == null || owner == null || CurrentNode == null)
				throw new ArgumentNullException();

			HideEditor(false);

			CurrentEditor = editor;
			CurrentEditorOwner = owner;
			_editingNode = CurrentNode;

			editor.Validating += EditorValidating;
			editor.Leave += EditorLeave;
			editor.LostFocus += EditorLeave;
			UpdateEditorBounds();
			UpdateView();
			editor.Parent = this;
			editor.Focus();
			owner.UpdateEditor(editor);
		}

		void EditorLeave(object sender, EventArgs e)
		{
			HideEditor(true);
		}

		internal bool HideEditor(bool applyChanges)
		{
			if (CurrentEditor != null)
			{
				if (applyChanges)
				{
					if (!ApplyChanges())
						return false;
				}

				//Check once more if editor was closed in ApplyChanges
				if (CurrentEditor != null)
				{
					CurrentEditor.Validating -= EditorValidating;
					CurrentEditor.Leave -= EditorLeave;
					CurrentEditor.LostFocus -= EditorLeave;
					CurrentEditorOwner.DoDisposeEditor(CurrentEditor);

					CurrentEditor.Parent = null;
					CurrentEditor.Dispose();

					CurrentEditor = null;
					CurrentEditorOwner = null;
					_editingNode = null;
				}
			}
			return true;
		}

		private bool ApplyChanges()
		{
			try
			{
				CurrentEditorOwner.ApplyChanges(_editingNode, CurrentEditor);
				_errorProvider.Clear();
				return true;
			}
			catch (ArgumentException ex)
			{
				_errorProvider.SetError(CurrentEditor, ex.Message);
				/*CurrentEditor.Validating -= EditorValidating;
				MessageBox.Show(this, ex.Message, "Value is not valid", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				CurrentEditor.Focus();
				CurrentEditor.Validating += EditorValidating;*/
				return false;
			}
		}

		void EditorValidating(object sender, System.ComponentModel.CancelEventArgs e)
		{
			e.Cancel = !ApplyChanges();
		}

		public void UpdateEditorBounds()
		{
			if (CurrentEditor != null)
			{
				EditorContext context = new EditorContext();
				context.Owner = CurrentEditorOwner;
				context.CurrentNode = CurrentNode;
				context.Editor = CurrentEditor;
				context.DrawContext = _measureContext;
				SetEditorBounds(context);
			}
		}

		private void SetEditorBounds(EditorContext context)
		{
			foreach (NodeControlInfo info in GetNodeControls(context.CurrentNode))
			{
				if (context.Owner == info.Control && info.Control is EditableControl)
				{
					Point p = info.Bounds.Location;
					p.X += info.Control.LeftMargin;
					p.X -= OffsetX;
					p.Y -= (_rowLayout.GetRowBounds(FirstVisibleRow).Y - ActualColumnHeaderHeight);
					int width = DisplayRectangle.Width - p.X;
					if (UseColumns && info.Control.ParentColumn != null && Columns.Contains(info.Control.ParentColumn))
					{
						Rectangle rect = GetColumnBounds(info.Control.ParentColumn.Index);
						width = rect.Right - OffsetX - p.X;
					}
					context.Bounds = new Rectangle(p.X, p.Y, width, info.Bounds.Height);
					((EditableControl)info.Control).SetEditorBounds(context);
					return;
				}
			}
		}

		private Rectangle GetColumnBounds(int column)
		{
			int x = 0;
			for (int i = 0; i < Columns.Count; i++)
			{
				if (Columns[i].IsVisible)
				{
					if (i < column)
						x += Columns[i].Width;
					else
						return new Rectangle(x, 0, Columns[i].Width, 0);
				}
			}
			return Rectangle.Empty;
		}
	}
}
