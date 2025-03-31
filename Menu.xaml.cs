using AdonisUI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ChatAppRealTime
{
	/// <summary>
	/// Interaction logic for NavBar.xaml
	/// </summary>
	public partial class Menu : UserControl
	{

		public Menu()
		{
			InitializeComponent();
		}

		private void Home_Click(object sender, RoutedEventArgs e)
		{
			ChatRoom chatRoom = new ChatRoom();
			chatRoom.Show();
			Window.GetWindow(this).Close();
		}

		private void AIChat_Click(object sender, RoutedEventArgs e)
		{
			ChatAi chatAi= new ChatAi();
			chatAi.Show();
			Window.GetWindow(this).Close();
		}

		private void Settings_Click(object sender, RoutedEventArgs e)
		{
		
		}

		private void DarkModeToggle_Checked(object sender, RoutedEventArgs e)
		{
			AdonisUI.ResourceLocator.SetColorScheme(Application.Current.Resources, ResourceLocator.DarkColorScheme);

		}

		private void DarkModeToggle_Unchecked(object sender, RoutedEventArgs e)
		{
			AdonisUI.ResourceLocator.SetColorScheme(Application.Current.Resources, ResourceLocator.LightColorScheme);

		}

		private void UserList_Click(object sender, RoutedEventArgs e)
		{
			ListUsers listUsers = new ListUsers();
			listUsers.Show();
		}

		private void Login_Click(object sender, RoutedEventArgs e)
		{
			MainWindow mainWindow = new MainWindow();
			mainWindow.Show();
			Window.GetWindow(this).Close();
		}
	}
}
