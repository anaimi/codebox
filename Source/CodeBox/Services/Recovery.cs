using System;
using System.Windows.Threading;
using System.Threading;
using System.IO.IsolatedStorage;
using System.IO;

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
		
		private const string FILE_NAME = "algorithmatic.txt";
		private bool isBusy;

		private void SaveCopy()
		{
			if (isBusy)
				return;

			isBusy = true;

			var text = Controller.Instance.Text;

			new Thread(() => {
           		try
           		{
           			// InitializeServices
           			var file = IsolatedStorageFile.GetUserStoreForSite();
           			var stream = file.CreateFile(FILE_NAME);
           			var writer = new StreamWriter(stream);

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

		public void LoadBackup()
		{
			try
			{
				string text;
				var file = IsolatedStorageFile.GetUserStoreForSite();
				var stream = file.OpenFile(FILE_NAME, FileMode.Open);
				var sr = new StreamReader(stream);

				text = sr.ReadToEnd().Trim();
				stream.Close();

				Controller.Instance.Text = text;
			}
			catch
			{
				
			}
		}
		
		public bool HaveCopy()
		{
			var file = IsolatedStorageFile.GetUserStoreForSite();
			return file.FileExists(FILE_NAME);
		}

		public void Initialize()
		{
			Controller.Instance.AddTextChangeObserver(7, SaveCopy);
		}
	}
}