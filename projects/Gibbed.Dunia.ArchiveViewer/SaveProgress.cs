/* Copyright (c) 2021 Rick (rick 'at' gibbed 'dot' us)
 *
 * This software is provided 'as-is', without any express or implied
 * warranty. In no event will the authors be held liable for any damages
 * arising from the use of this software.
 *
 * Permission is granted to anyone to use this software for any purpose,
 * including commercial applications, and to alter it and redistribute it
 * freely, subject to the following restrictions:
 *
 * 1. The origin of this software must not be misrepresented; you must not
 *    claim that you wrote the original software. If you use this software
 *    in a product, an acknowledgment in the product documentation would
 *    be appreciated but is not required.
 *
 * 2. Altered source versions must be plainly marked as such, and must not
 *    be misrepresented as being the original software.
 *
 * 3. This notice may not be removed or altered from any source
 *    distribution.
 */

using System;
using System.Windows.Forms;

namespace Gibbed.Dunia.ArchiveViewer
{
	public partial class SaveProgress : Form
	{
		public SaveProgress()
		{
			this.InitializeComponent();
		}

        delegate void SetStatusDelegate(string status, int progress, int total);
		public void SetStatus(string status, int progress, int total)
		{
			if (this.progressBar.InvokeRequired == true ||
                this.statusLabel.InvokeRequired == true)
			{
				var callback = new SetStatusDelegate(SetStatus);
				this.Invoke(callback, new object[] { status, progress, total });
				return;
			}

            if (this.statusLabel.Text != status)
            {
                this.statusLabel.Text = status;
            }

            if (this.progressBar.Value != progress)
            {
                this.progressBar.Value = progress;
            }

            if (this.progressBar.Maximum != total)
            {
                this.progressBar.Maximum = total;
            }
		}

		delegate void WorkDoneDelegate();
		public void WorkDone()
		{
			if (this.InvokeRequired == true)
			{
				var callback = new WorkDoneDelegate(WorkDone);
				this.Invoke(callback);
				return;
			}

			this.Close();
		}

        public event EventHandler Cancel;

        private void OnCancel(object sender, EventArgs e)
        {
            if (this.Cancel != null)
            {
                this.Cancel.Invoke(this, null);
            }
        }
	}
}
