using System;
using System.Windows.Threading;
using System.Threading;
using System.IO.IsolatedStorage;
using System.IO;
using CodeBox.Core.Services;

namespace CodeBox.Core.Services
{
	public class Recovery : IService
	{
		#region singleton instantiation
		private static Recovery _instance;
		public static Recovery Instance
		{
			get
			{
				if (_instance == null)
					_instance = new Recovery();

				return _instance;
			}
		}
		
		private Recovery()
		{
			
		}
		#endregion
		
		private const int SAVE_EVERY = 5;
		private const string FILE_NAME = "algorithmatic.txt";

		private DispatcherTimer timer;
		private string lastCopy = "";
		private bool isBusy;

		private void SaveCopy(object sender, EventArgs e)
		{
			if (isBusy)
				return;

			isBusy = true;

			string text = Controller.Instance.Text;

			new Thread(() =>
			           	{
			           		if (text == lastCopy)
			           		{
			           			isBusy = false;
			           			return;
			           		}

			           		lastCopy = text;

			           		try
			           		{
			           			// InitializeServices
			           			IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForSite();
			           			IsolatedStorageFileStream stream = file.CreateFile(FILE_NAME);
			           			StreamWriter writer = new StreamWriter(stream);

			           			// Save
			           			writer.WriteLine(text);
			           			writer.Close();
			           		}
			           		catch
			           		{
			           			System.Diagnostics.Debug.WriteLine("Error saving backup.");
			           		}

			           		isBusy = false;
			           	}).Start();
		}

		public void LoadBackupIfAvailable()
		{
			try
			{
				string text;
				IsolatedStorageFile file = IsolatedStorageFile.GetUserStoreForSite();
				IsolatedStorageFileStream stream = file.OpenFile(FILE_NAME, FileMode.Open);
				StreamReader sr = new StreamReader(stream);

				text = sr.ReadToEnd().Trim();
				stream.Close();

				Controller.Instance.Text = text;
				lastCopy = text;
			}
			catch
			{
				
			}
		}

		public void Initialize()
		{
			timer = new DispatcherTimer();
			timer.Interval = TimeSpan.FromSeconds(SAVE_EVERY);
			timer.Tick += SaveCopy;
			timer.Start();
		}
	}
}