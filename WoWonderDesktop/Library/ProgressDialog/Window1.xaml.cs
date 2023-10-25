using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace WoWonderDesktop.Library.ProgressDialog
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class Window1 : Window
    {
        public Window1()
        {
            InitializeComponent();
        }

		void OnProgressDialogButtonClick(object sender, RoutedEventArgs e)
		{
			switch ((sender as Button).Tag.ToString())
			{
				case "Default":
					ProgressDialogTestDefault();
					break;
				case "WithSubLabel":
					ProgressDialogTestWithSubLabel();
					break;
				case "WithCancelButton":
					ProgressDialogTestWithCancelButton();
					break;
				case "WithCancelButtonAndProgressDisplay":
					ProgressDialogTestWithCancelButtonAndProgressDisplay();
					break;
			}
		}

		void ProgressDialogTestDefault()
		{ 
			ProgressDialogResult result = ProgressDialog.Execute(this, "Loading data...",4000, (bw, we) => {

				Thread.Sleep(4000); 

			});

			if (result.OperationFailed)
				MessageBox.Show("ProgressDialog failed.");
			else
				MessageBox.Show("ProgressDialog successfully executed.");
		}

		void ProgressDialogTestWithSubLabel()
		{
			ProgressDialogResult result = ProgressDialog.Execute(this, "Loading data", 1500,(bw) => {

				for (int i = 1; i <= 5; i++)
				{
					ProgressDialog.Report(bw, "Executing step {0}/5...", i);

					Thread.Sleep(1500);
				}

			}, ProgressDialogSettings.WithSubLabel);

			if (result.OperationFailed)
				MessageBox.Show("ProgressDialog failed.");
			else
				MessageBox.Show("ProgressDialog successfully executed.");
		}

		void ProgressDialogTestWithCancelButton()
		{
			// Easy way to pass data to the async method
			int millisecondsTimeout = 1500;

			ProgressDialogResult result = ProgressDialog.Execute(this, "Loading data", millisecondsTimeout, (bw, we) => {

				for (int i = 1; i <= 5; i++)
				{
					if (ProgressDialog.ReportWithCancellationCheck(bw, we, "Executing step {0}/5...", i))
						return;

					Thread.Sleep(millisecondsTimeout);
				}

				// So this check in order to avoid default processing after the Cancel button has been pressed.
				// This call will set the Cancelled flag on the result structure.
				ProgressDialog.CheckForPendingCancellation(bw, we);

			}, ProgressDialogSettings.WithSubLabelAndCancel);

			if (result.Cancelled)
				MessageBox.Show("ProgressDialog cancelled.");
			else if (result.OperationFailed)
				MessageBox.Show("ProgressDialog failed.");
			else
				MessageBox.Show("ProgressDialog successfully executed.");
		}

		void ProgressDialogTestWithCancelButtonAndProgressDisplay()
		{
			// Easy way to pass data to the async method
			int millisecondsTimeout = 250;

			ProgressDialogResult result = ProgressDialog.Execute(this, "Loading data", millisecondsTimeout, (bw, we) => {

				for (int i = 1; i <= 20; i++)
				{
					if (ProgressDialog.ReportWithCancellationCheck(bw, we, i * 5, "Executing step {0}/20...", i))
						return;

					Thread.Sleep(millisecondsTimeout);
				}

				// So this check in order to avoid default processing after the Cancel button has been pressed.
				// This call will set the Cancelled flag on the result structure.
				ProgressDialog.CheckForPendingCancellation(bw, we);

			}, new ProgressDialogSettings(true, true, false));

			if (result.Cancelled)
				MessageBox.Show("ProgressDialog cancelled.");
			else if (result.OperationFailed)
				MessageBox.Show("ProgressDialog failed.");
			else
				MessageBox.Show("ProgressDialog successfully executed.");
		}
	}
}
