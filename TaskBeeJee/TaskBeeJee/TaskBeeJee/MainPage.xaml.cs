using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;
using TaskBeeJee.ApiConnector;
using TaskBeeJee.Exceptions;
using TaskBeeJee.ApiConnector.JsonData;

namespace TaskBeeJee
{
    public partial class MainPage : ContentPage
    {
        #region Constants
        private const int RECORDS_PER_PAGE = 3;
        #endregion

        #region Vars
        private ActivityIndicator activityIndicator;
        private List<TaskData>[] allTasks;
        private int currPage;
        private Main.SortingField currSortField;
        private Main.SortingType currSortType;
        #endregion

        public MainPage()
        {
            InitializeComponent();

            allTasks = null;
            currPage = 0;

            activityIndicator = new ActivityIndicator
            {
                IsRunning = true,
                VerticalOptions = LayoutOptions.Center,
                HorizontalOptions = LayoutOptions.Center
            };

            Main.TaskSaved += () => LoadFirstPage();

            pickerSort.SelectedIndex = 0; //Runs PickerChanged => runs LoadFirstPage
        }

        #region MainMethods

        private void AddBtnPressed(object sender, EventArgs e) { Navigation.PushAsync(new TaskPage()); }
        private void AuthClicked(object sender, EventArgs e) { Navigation.PushAsync(new LoginPage()); }

        private Button CreatePageButton(int page, bool current = false)
        {
            Button pageBtn = new Button()
            {
                VerticalOptions = LayoutOptions.Center,
                CornerRadius = 30,
                BackgroundColor = current ? Color.LightBlue : Color.White,
                WidthRequest = 40,
                HeightRequest = 40,
                BorderColor = Color.Black,
                BorderWidth = 1,
                Text = (page + 1).ToString(),
                FontSize = 10
            };
            pageBtn.Clicked += (object sender, EventArgs e) => GoToPage(page);
            return pageBtn;
        }

        private Label CreatePageDots() { return new Label() { VerticalOptions = LayoutOptions.Center, Text = ". . ." }; }

        private void FillPage(List<TaskData> tasks)
        {
            //Tasks
            mainLayout.Children.Clear();
            foreach(TaskData task in tasks)
            {
                StackLayout layout = new StackLayout() { Orientation = StackOrientation.Vertical };
                layout.Children.Add(new Label() { Text = task.User });
                layout.Children.Add(new Label() { Text = task.Email });
                layout.Children.Add(new Label() { Text = task.EnumStatus?.GetStringRepresentation() });
                layout.Children.Add(new Label() { Text = task.Text, LineBreakMode = LineBreakMode.TailTruncation });

                Grid grid = new Grid() { VerticalOptions = LayoutOptions.FillAndExpand };
                grid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(1, GridUnitType.Star) });
                grid.Children.Add(layout);

                if (task.IsEdited)
                    grid.Children.Add(new Label() { Text = "edited", VerticalOptions = LayoutOptions.Start, HorizontalOptions = LayoutOptions.End });

                Frame frame = new Frame()
                {
                    Margin = new Thickness(5, 5, 5, 0),
                    CornerRadius = 10,
                    BorderColor = Color.LightGray,
                    BackgroundColor = task.IsDone ? Color.FromHex("#f3ffed") : Color.White
                };
                TapGestureRecognizer gesture = new TapGestureRecognizer();
                gesture.Tapped += (object sender, EventArgs e) => OpenTask(task);
                frame.GestureRecognizers.Add(gesture);
                frame.Content = grid;
                mainLayout.Children.Add(frame);
            }

            //Page buttons
            pagesLayout.Children.Clear();
            if (allTasks.Length <= 5)
            {
                for(int i = 0; i < allTasks.Length; i++)                    
                    pagesLayout.Children.Add(CreatePageButton(i, i == currPage));
            } else
            {
                if(currPage == 0)
                { 
                    pagesLayout.Children.Add(CreatePageButton(0, true));
                    pagesLayout.Children.Add(CreatePageButton(1));
                    pagesLayout.Children.Add(CreatePageDots());
                    pagesLayout.Children.Add(CreatePageButton(allTasks.Length - 1));
                } else if(currPage == 1)
                {
                    pagesLayout.Children.Add(CreatePageButton(0));
                    pagesLayout.Children.Add(CreatePageButton(1, true));
                    pagesLayout.Children.Add(CreatePageButton(2));
                    pagesLayout.Children.Add(CreatePageDots());
                    pagesLayout.Children.Add(CreatePageButton(allTasks.Length - 1));
                } else if(currPage == allTasks.Length - 2)
                {
                    pagesLayout.Children.Add(CreatePageButton(0));
                    pagesLayout.Children.Add(CreatePageDots());
                    pagesLayout.Children.Add(CreatePageButton(allTasks.Length - 3));
                    pagesLayout.Children.Add(CreatePageButton(allTasks.Length - 2, true));
                    pagesLayout.Children.Add(CreatePageButton(allTasks.Length - 1));
                } else if(currPage == allTasks.Length - 1)
                {
                    pagesLayout.Children.Add(CreatePageButton(0));
                    pagesLayout.Children.Add(CreatePageDots());
                    pagesLayout.Children.Add(CreatePageButton(allTasks.Length - 2));
                    pagesLayout.Children.Add(CreatePageButton(allTasks.Length - 1, true));
                } else
                {
                    pagesLayout.Children.Add(CreatePageButton(0));
                    pagesLayout.Children.Add(CreatePageDots());
                    pagesLayout.Children.Add(CreatePageButton(currPage - 1));
                    pagesLayout.Children.Add(CreatePageButton(currPage, true));
                    pagesLayout.Children.Add(CreatePageButton(currPage + 1));
                    pagesLayout.Children.Add(CreatePageDots());
                    pagesLayout.Children.Add(CreatePageButton(allTasks.Length - 1));
                }
            }
        }

        private async void GoToPage(int page)
        {
            mainLayout.Children.Clear();
            mainLayout.Children.Add(activityIndicator);

            if(allTasks[page] == null)
            {
                try
                {
                    TasksData tasks = await Main.GetTasksAsync(page + 1, currSortType, currSortField);
                    if (tasks == null)
                    {
                        PrintError("No tasks");
                        return;
                    }
                    allTasks[page] = tasks.Tasks;
                }
                catch
                {
                    PrintError("Something went wrong");
                    return;
                }
            }
            currPage = page;
            FillPage(allTasks[page]);
        }

        private async void LoadFirstPage()
        {
            mainLayout.Children.Clear();
            pagesLayout.Children.Clear();
            mainLayout.Children.Add(activityIndicator);

            allTasks = null;
            currPage = 0;
            TasksData tasks = null;
            currSortField = (Main.SortingField)(pickerSort.SelectedIndex / 2);
            currSortType = (Main.SortingType)(pickerSort.SelectedIndex % 2);
            try
            {
                tasks = await Main.GetTasksAsync(1, currSortType, currSortField);
            }
            catch
            {
                PrintError("Something went wrong");
                return;
            }

            if (tasks == null)
            {
                PrintError("No tasks");
                return;
            }

            int pages = (tasks.TotalCount + RECORDS_PER_PAGE - 1) / RECORDS_PER_PAGE;
            allTasks = new List<TaskData>[pages];
            allTasks[0] = tasks.Tasks;

            FillPage(tasks.Tasks);
        }

        private void LoginBtnClicked(object sender, EventArgs e)
        {
            if (Main.IsLogined)
            {
                Main.LogOut();
                loginBtn.Text = "Log in";
            }
            else
                Navigation.PushAsync(new LoginPage());
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            if (Main.IsLogined)
                loginBtn.Text = "Log out";
            else
                loginBtn.Text = "Log in";
        }

        private void OpenTask(TaskData task) { Navigation.PushAsync(new TaskPage(task)); }

        private void PickerChanged(object sender, EventArgs e) { LoadFirstPage(); }

        private void PrintError(string error)
        {
            mainLayout.Children.Clear();
            pagesLayout.Children.Clear();
            allTasks = null;
            currPage = 0;
            mainLayout.Children.Add(new Label() { Text = error, HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(10) });
            Button refreshBtn = new Button() { Text = "Refresh", HorizontalOptions = LayoutOptions.Center, Margin = new Thickness(10) };
            refreshBtn.Clicked += (object sender, EventArgs e) => LoadFirstPage();
            mainLayout.Children.Add(refreshBtn);
        }
        #endregion
    }
}
