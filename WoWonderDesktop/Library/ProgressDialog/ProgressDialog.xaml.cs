//
// Parago Media GmbH & Co. KG, Jürgen Bäurle (jbaurle@parago.de)
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
//

using System;
using System.ComponentModel;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace WoWonderDesktop.Library.ProgressDialog
{
	public partial class ProgressDialog : Window
	{
		volatile bool IsBusy;
		BackgroundWorker Worker;

		public string Label
		{
			get => TextLabel.Text;
            set => TextLabel.Text = value;
        }

		public string SubLabel
		{
			get => SubTextLabel.Text;
            set => SubTextLabel.Text = value;
        }

		internal ProgressDialogResult Result { get; private set; }

		public ProgressDialog(ProgressDialogSettings settings)
		{
			InitializeComponent();

			//disable window moving
			SourceInitialized += OnSourceInitialized;

			double top;
			double right;

			if (settings == null)
				settings = ProgressDialogSettings.WithLabelOnly;

			if (settings.ShowSubLabel)
			{
				top = 38;
				Height = 110;
				SubTextLabel.Visibility = Visibility.Visible;
			}
			else
			{
				top = 22;
				Height = 100;
				SubTextLabel.Visibility = Visibility.Collapsed;
			}

			if (settings.ShowCancelButton)
			{
				right = 74;
				CancelButton.Visibility = Visibility.Visible;
			}
			else
			{
				right = 0;
				CancelButton.Visibility = Visibility.Collapsed;
			}

			ProgressBar.Margin = new Thickness(0, top, right, 0);
			ProgressBar.IsIndeterminate = settings.ShowProgressBarIndeterminate;
		}

		#region disable window moving

        private void OnSourceInitialized(object sender, EventArgs e)
        {
            WindowInteropHelper helper = new WindowInteropHelper(this);
            HwndSource source = HwndSource.FromHwnd(helper.Handle);
            source.AddHook(WndProc);
        }

        const int WmSyscommand = 0x0112;
        const int ScMove = 0xF010;

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {

            switch (msg)
            {
                case WmSyscommand:
                    int command = wParam.ToInt32() & 0xfff0;
                    if (command == ScMove)
                    {
                        handled = true;
                    }
                    break;
                default:
                    break;
            }
            return IntPtr.Zero;
        }
		 
		#endregion


		internal ProgressDialogResult Execute(object operation , int timeout)
		{
			if (operation == null)
				throw new ArgumentNullException("operation");

			ProgressDialogResult result = null;

			IsBusy = true;

			Worker = new BackgroundWorker();
			Worker.WorkerReportsProgress = true;
			Worker.WorkerSupportsCancellation = true;

			Worker.DoWork += (s, e) => {
				if (operation is Action) ((Action)operation)();
                else if (operation is Action<BackgroundWorker>) ((Action<BackgroundWorker>)operation)(s as BackgroundWorker);
                else if (operation is Action<BackgroundWorker, DoWorkEventArgs>) ((Action<BackgroundWorker, DoWorkEventArgs>)operation)(s as BackgroundWorker, e);
                else if (operation is Func<object>) e.Result = ((Func<object>)operation)();
                else if (operation is Func<BackgroundWorker, object>) e.Result = ((Func<BackgroundWorker, object>)operation)(s as BackgroundWorker);
                else if (operation is Func<BackgroundWorker, DoWorkEventArgs, object>) e.Result = ((Func<BackgroundWorker, DoWorkEventArgs, object>)operation)(s as BackgroundWorker, e);
                else
                    throw new InvalidOperationException("Operation type is not supoorted");
            };

			Worker.RunWorkerCompleted += (s, e) => {
                if (timeout > -1)
				{
					result = new ProgressDialogResult(e);

					Thread.Sleep(timeout);

                    Dispatcher.BeginInvoke(DispatcherPriority.Send, (SendOrPostCallback)delegate {
                        IsBusy = false;
                        Close();
                    }, null);
				} 
			};

			Worker.ProgressChanged += (s, e) => {
				if (!Worker.CancellationPending)
                {
                    SubLabel = (e.UserState as string) ?? string.Empty;
                    ProgressBar.Value = e.ProgressPercentage;
                }
            };

			Worker.RunWorkerAsync();

			ShowDialog();

			return result;
		}

        internal void Dismiss()
        {
			if (Worker != null && Worker.WorkerSupportsCancellation)
            {
                Worker.CancelAsync();
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Send, (SendOrPostCallback)delegate {
                IsBusy = false;
                Close();
            }, null);
        }

		void OnCancelButtonClick(object sender, RoutedEventArgs e)
		{
			if (Worker != null && Worker.WorkerSupportsCancellation)
			{
				SubLabel = "Please wait while process will be cancelled...";
				CancelButton.IsEnabled = false;
				Worker.CancelAsync();
			}
		}

		void OnClosing(object sender, CancelEventArgs e)
		{
			e.Cancel = IsBusy;
		}

		internal static ProgressDialogResult Execute(Window owner, string label, int timeout, Action operation)
		{
			return ExecuteInternal(owner, label, timeout,(object)operation, null);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, int timeout, Action operation, ProgressDialogSettings settings)
		{
			return ExecuteInternal(owner, label, timeout, (object)operation, settings);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, int timeout, Action<BackgroundWorker> operation)
		{
			return ExecuteInternal(owner, label, timeout, (object)operation, null);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, int timeout, Action<BackgroundWorker> operation, ProgressDialogSettings settings)
		{
			return ExecuteInternal(owner, label, timeout, (object)operation, settings);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, int timeout, Action<BackgroundWorker, DoWorkEventArgs> operation)
		{
			return ExecuteInternal(owner, label, timeout, (object)operation, null);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, int timeout, Action<BackgroundWorker, DoWorkEventArgs> operation, ProgressDialogSettings settings)
		{
			return ExecuteInternal(owner, label, timeout, (object)operation, settings);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, int timeout, Func<object> operationWithResult)
		{
			return ExecuteInternal(owner, label, timeout, (object)operationWithResult, null);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, int timeout, Func<object> operationWithResult, ProgressDialogSettings settings)
		{
			return ExecuteInternal(owner, label, timeout, (object)operationWithResult, settings);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, int timeout, Func<BackgroundWorker, object> operationWithResult)
		{
			return ExecuteInternal(owner, label, timeout, (object)operationWithResult, null);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, int timeout, Func<BackgroundWorker, object> operationWithResult, ProgressDialogSettings settings)
		{
			return ExecuteInternal(owner, label, timeout, (object)operationWithResult, settings);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, int timeout, Func<BackgroundWorker, DoWorkEventArgs, object> operationWithResult)
		{
			return ExecuteInternal(owner, label, timeout, (object)operationWithResult, null);
		}

		internal static ProgressDialogResult Execute(Window owner, string label, int timeout, Func<BackgroundWorker, DoWorkEventArgs, object> operationWithResult, ProgressDialogSettings settings)
		{
			return ExecuteInternal(owner, label, timeout, (object)operationWithResult, settings);
		}

		internal static void Execute(Window owner, string label, int timeout, Action operation, Action<ProgressDialogResult> successOperation, Action<ProgressDialogResult> failureOperation = null, Action<ProgressDialogResult> cancelledOperation = null)
		{
			ProgressDialogResult result = ExecuteInternal(owner, label, timeout, operation, null);

			if (result.Cancelled && cancelledOperation != null)
				cancelledOperation(result);
			else if (result.OperationFailed && failureOperation != null)
				failureOperation(result);
			else if (successOperation != null)
				successOperation(result);
		}

        internal static ProgressDialog Dialog;
		internal static ProgressDialogResult ExecuteInternal(Window owner, string label, int timeout, object operation, ProgressDialogSettings settings)
		{
			Dialog = new ProgressDialog(settings);
			Dialog.Owner = owner;

			if (!string.IsNullOrEmpty(label))
				Dialog.Label = label;

			return Dialog.Execute(operation, timeout);
		}

        internal static void ExecuteDismiss()
        {
            Dialog?.Dismiss();
        }

		internal static bool CheckForPendingCancellation(BackgroundWorker worker, DoWorkEventArgs e)
		{
			if (worker.WorkerSupportsCancellation && worker.CancellationPending)
				e.Cancel = true;

			return e.Cancel;
		}
		//This operation has already had OperationCompleted called on it and further calls are illegal
		internal static void Report(BackgroundWorker worker, string message)
		{
           if (worker.WorkerReportsProgress)
                worker.ReportProgress(0, message);
		}

		internal static void Report(BackgroundWorker worker, string format, params object[] arg)
		{
			if (worker.WorkerReportsProgress)
				worker.ReportProgress(0, string.Format(format, arg));
		}

		internal static void Report(BackgroundWorker worker, int percentProgress, string message)
		{
			if (worker.WorkerReportsProgress)
				worker.ReportProgress(percentProgress, message);
		}

		internal static void Report(BackgroundWorker worker, int percentProgress, string format, params object[] arg)
		{
			if (worker.WorkerReportsProgress)
				worker.ReportProgress(percentProgress, string.Format(format, arg));
		}

		internal static bool ReportWithCancellationCheck(BackgroundWorker worker, DoWorkEventArgs e, string message)
		{
			if (CheckForPendingCancellation(worker, e))
				return true;

			if (worker.WorkerReportsProgress)
				worker.ReportProgress(0, message);

			return false;
		}

		internal static bool ReportWithCancellationCheck(BackgroundWorker worker, DoWorkEventArgs e, string format, params object[] arg)
		{
			if (CheckForPendingCancellation(worker, e))
				return true;

			if (worker.WorkerReportsProgress)
				worker.ReportProgress(0, string.Format(format, arg));

			return false;
		}

		internal static bool ReportWithCancellationCheck(BackgroundWorker worker, DoWorkEventArgs e, int percentProgress, string message)
		{
			if (CheckForPendingCancellation(worker, e))
				return true;

			if (worker.WorkerReportsProgress)
				worker.ReportProgress(percentProgress, message);

			return false;
		}

		internal static bool ReportWithCancellationCheck(BackgroundWorker worker, DoWorkEventArgs e, int percentProgress, string format, params object[] arg)
		{
			if (CheckForPendingCancellation(worker, e))
				return true;

			if (worker.WorkerReportsProgress)
				worker.ReportProgress(percentProgress, string.Format(format, arg));

			return false;
		}
	}
}
