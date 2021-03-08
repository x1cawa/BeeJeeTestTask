using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using TaskBeeJee.ApiConnector.JsonData;
using TaskBeeJee.ApiConnector;

namespace TaskBeeJee
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class TaskPage : ContentPage
    {
        private TaskData data;
        private Editor textEditor;

        //New Task
        public TaskPage()
        {
            InitializeComponent();
            data = null;

            AddSaveButton();
            AddTextEditor("");
        }

        //Edit Task
        public TaskPage(TaskData task)
        {
            InitializeComponent();
            data = task;

            statusBar.IsVisible = true;

            statusCheckBox.IsChecked = task.IsDone;
            statusCheckBox.IsEnabled = Main.IsLogined;
            editedLabel.IsVisible = task.IsEdited;

            userEntry.Text = data.User;
            userEntry.IsEnabled = false;

            emailEntry.Text = data.Email;
            emailEntry.IsEnabled = false;

            if (Main.IsLogined)
            {
                AddSaveButton();
                AddTextEditor(data.Text);
            } else
            {
                Label label = new Label() { Text = data.Text, TextColor = Color.Gray };
                editorFrame.Content = label;
            }
        }

        private void AddSaveButton()
        {
            ToolbarItem toolbarItem = new ToolbarItem() { Text = "Save  " };
            toolbarItem.Clicked += (object sender, EventArgs e) => SaveClicked();
            this.ToolbarItems.Add(toolbarItem);
        }

        private void AddTextEditor(string text)
        {
            textEditor = new Editor() { Placeholder = "Text", Text = text, HeightRequest = 300 };
            editorFrame.Content = textEditor;
        }

        private async void SaveClicked()
        {
            //Validation
            if (userEntry.Text.Length == 0)
            {
                await DisplayAlert("Validation error", "Empty user", "Ok");
                return;
            }
            if (emailEntry.Text.Length == 0)
            { 
                await DisplayAlert("Validation error", "Empty email", "Ok");
                return;
            }
            if (textEditor.Text.Length == 0)
            { 
                await DisplayAlert("Validation error", "Empty text", "Ok");
                return;
            }

            try
            {
                new System.Net.Mail.MailAddress(emailEntry.Text);
            } catch
            {
                await DisplayAlert("Validation error", "Incorrect email", "Ok");
                return;
            }

            if (data == null)
            {
                //new task
                TaskData task = new TaskData()
                {
                    User = userEntry.Text,
                    Email = emailEntry.Text,
                    Text = textEditor.Text
                };
                try
                {
                    await Main.SaveTaskAsync(task);
                } catch
                {
                    await DisplayAlert("Error", "Something went wrong", "Ok");
                    return;
                }
                await DisplayAlert("Done", "Saved", "Ok");
                await Navigation.PopAsync();
            } else
            {
                //edit task
                data.Status = (statusCheckBox.IsChecked ? 1 : 0) * 10 + (data.IsEdited || data.Text != textEditor.Text ? 1 : 0);
                data.Text = textEditor.Text;
                try
                {
                    await Main.EditTaskAsync(data);
                } catch
                {
                    await DisplayAlert("Error", "Something went wrong", "Ok");
                    return;
                }
                await DisplayAlert("Done", "Saved", "Ok");
                await Navigation.PopAsync();
            }
        }
    }
}