﻿using System.Drawing;
using System;
using System.Windows.Forms;

public static class CInputText
{
	public static DialogResult Show( string strTitle, string strPromptText, ref string strValue )
	{
		Form form = new Form();
		Label label = new Label();
		TextBox textBox = new TextBox();
		Button buttonOk = new Button();
		Button buttonCancel = new Button();

		form.Text = strTitle;
		label.Text = strPromptText;
		textBox.Text = strValue;

		buttonOk.Text = "OK";
		buttonCancel.Text = "Cancel";
		buttonOk.DialogResult = DialogResult.OK;
		buttonCancel.DialogResult = DialogResult.Cancel;

		label.SetBounds( 9, 20, 372, 13 );
		textBox.SetBounds( 12, 36, 372, 20 );
		buttonOk.SetBounds( 228, 72, 75, 23 );
		buttonCancel.SetBounds( 309, 72, 75, 23 );

		label.AutoSize = true;
		textBox.Anchor = textBox.Anchor | AnchorStyles.Right;
		buttonOk.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
		buttonCancel.Anchor = AnchorStyles.Bottom | AnchorStyles.Right;

		form.ClientSize = new Size( 396, 107 );
		form.Controls.AddRange( new Control[] { label, textBox, buttonOk, buttonCancel } );
		form.ClientSize = new Size( Math.Max( 300, label.Right + 10 ), form.ClientSize.Height );
		form.FormBorderStyle = FormBorderStyle.FixedDialog;
		form.StartPosition = FormStartPosition.CenterScreen;
		form.MinimizeBox = false;
		form.MaximizeBox = false;
		form.AcceptButton = buttonOk;
		form.CancelButton = buttonCancel;

		DialogResult dialogResult = form.ShowDialog();
		strValue = textBox.Text;
		return dialogResult;
	}
}