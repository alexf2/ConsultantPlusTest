using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Threading;

using Ifx.FoundationHelpers.RuntimeModel;

namespace Ifx.FoundationHelpers.WinServices
{
	partial class ServiceDebugForm : Form
	{
		public event EventHandler Stop;
		public event EventHandler Suspend;
		SynchronizationContext _ctx;
		RuntimeServiceState _currState = RuntimeServiceState.Stopped;

		public ServiceDebugForm ()
		{
			_ctx = WindowsFormsSynchronizationContext.Current;
			InitializeComponent();
			adjustButton();
		}

		private void button1_Click (object sender, EventArgs e)
		{
			Application.DoEvents();
			Close();
		}

		public bool SupportsResume
		{
			get;
			set;
		}

		public void ShowState (RuntimeServiceState state)
		{
			_currState = state;
			
			Action act = () => {adjustButton(); textBox1.Text = string.Format("Service is in {0} state", state); textBox1.Select(0,0); Application.DoEvents();};
			if (InvokeRequired)
				//Invoke( (MethodInvoker)delegate  {act();} );
				_ctx.Post( (st) => act(), null);
			else
				act();
		}

		public void ShowStateFailure (RuntimeServiceState from, RuntimeServiceState to, Exception ex)
		{
			_currState = from;
			
			Action act = () => {adjustButton(); string.Format("Error at transition {0} --> {1}:\r\n", from, to, ex.ToString()); textBox1.Select(0,0); Application.DoEvents();};
			if (InvokeRequired)
				_ctx.Post( (st) => act(), null);
			else
				act();
		}
		
		void ServiceDebugForm_FormClosing (object sender, FormClosingEventArgs e)
		{
			EventHandler h = Stop;
			if (h != null)				
				h(this, null);
		}

		void adjustButton ()
		{
			string val = null;
			string val2 = null;
			switch (_currState )
			{
				case RuntimeServiceState.Stopped:
					val = "Start";					
					break;
				case RuntimeServiceState.Started:
					val = "Stop";
					val2 = "Suspend";
					break;
				case RuntimeServiceState.Suspended:
					val2 = "Resume";
					break;
			}
			if (val == null)
				button1.Enabled = false;
			else {
				button1.Enabled = true;
				button1.Text = val;
			}

			if (SupportsResume)
			{
				if (val2 == null)
				{
					button2.Enabled = false;
					hide2(true);
				}
				else {
					button2.Enabled = true;
					button2.Text = val2;
					hide2(false);
				}
			}
		}

		private void button2_Click(object sender, EventArgs e)
		{
			EventHandler h = Suspend;
			if (h != null)
				h(this, null);
		}

		void hide2 (bool hide)
		{
			button2.Visible = !hide;
			if (hide)
			{				
				button1.Left = (ClientSize.Width - button1.Width) / 2;				
			}
			else {
				int w = button1.Width + button2.Width + 10;
				button1.Left = (ClientSize.Width - w) / 2;				
				button2.Left = (ClientSize.Width - w) / 2 + button1.Width + 10;				
			}
		}
	}

}
