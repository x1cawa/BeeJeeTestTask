using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using Xamarin.Essentials;
using System.Threading.Tasks;
using TaskBeeJee.Exceptions;
using TaskBeeJee.ApiConnector.JsonData;
using Newtonsoft.Json;
using System.Net;
using System.ComponentModel;

namespace TaskBeeJee.ApiConnector
{
    public static class Main
    {
        #region Types
        public delegate void Event();

        public enum SortingType
        {
            asc,
            desc
        }

        public enum SortingField
        {
            id,
            username,
            email,
            status
        }

        public enum TaskStatus
        {
            [Description("In progress")]
            InProgress = 0,
            [Description("In progress and edited")]
            InProgressEdited = 1,
            [Description("Done")]
            Done = 10,
            [Description("Done and edited")]
            DoneEdited = 11
        }
        #endregion

        #region Constants
        const string PREF_KEY_CUSTOMER_TOKEN = "tkn_cal";
        const string PREF_KEY_CUSTOMER_TOKEN_CREATED = "tkn_created";
        const string PREF_KEY_CUSTOMER_LOGIN = "tkn_lgn";
        const string PREF_KEY_CUSTOMER_PASSWORD = "tkn_psswd";

        const string API_URL = "https://uxcandy.com/~shapoval/test-task-backend/v2";
        const string PARAM_DEV = "developer=alexander_khodin_test";
        #endregion

        #region Vars
        private static HttpClient client;
        private static AuthToken authToken;
        #endregion

        #region Events
        public static event Event TaskSaved;
        #endregion

        #region Properties
        public static bool IsLogined;
        #endregion

        static Main()
        {
            client = new HttpClient();
            authToken = null;
            IsLogined = false;
            if (IsAuthTokenSaved())
            {
                LoadAuthToken();
                _ = ReAuthIfNeeded();
            }
        }

        #region Main methods

        /// <summary>
        /// Log in as admin. Return true if login succeed
        /// </summary>
        /// <param name="login">Admin login</param>
        /// <param name="password">Admin password</param>
        public static async Task<bool> AuthAsync(string login, string password)
        {
            var payload = new MultipartFormDataContent();
            payload.Add(new StringContent(login), "username");
            payload.Add(new StringContent(password), "password");
            TokenData res = await RequestPostAsync<TokenData>(new Uri(API_URL + "/login?" + PARAM_DEV), payload);
            if (res.Value == null)
                return false;
            authToken = new AuthToken(res.Value, DateTime.Now, login, password);
            SaveAuthToken();
            IsLogined = true;
            return true;
        }
        
        /// <summary>
        /// Edit task with admin token
        /// </summary>
        /// <param name="task">Task data to edit</param>
        /// <returns></returns>
        public static async Task<bool> EditTaskAsync(TaskData task)
        {
            await ReAuthIfNeeded();

            var payload = new MultipartFormDataContent();
            payload.Add(new StringContent(task.Text), "text");
            payload.Add(new StringContent(task.Status.ToString()), "status");
            payload.Add(new StringContent(authToken.Value), "token");

            await RequestPostAsync<string>(new Uri(API_URL + $"/edit/{task.ID}?" + PARAM_DEV), payload);

            TaskSaved?.Invoke();

            return true;
        }

        /// <summary>
        /// Get page of tasks
        /// </summary>
        /// <param name="page">Page number (from 1)</param>
        /// <param name="sortType">Sorting type</param>
        /// <param name="sortField">Sort by field</param>
        /// <returns></returns>
        public static async Task<TasksData> GetTasksAsync(int page, SortingType sortType, SortingField sortField)
        {
            TasksData res = await RequestGetAsync<TasksData>(new Uri(API_URL + $"/?{PARAM_DEV}&page={page}&sort_direction={sortType}&sort_field={sortField}" ));
            return (res.TotalCount == 0 || res.Tasks.Count == 0) ? null : res;
        }

        /// <summary>
        /// Admin log out
        /// </summary>
        public static void LogOut()
        {
            authToken = null;
            RemoveSavedAuthToken();
            IsLogined = false;
        }

        /// <summary>
        /// Save new task
        /// </summary>
        /// <param name="task">Task data to save</param>
        public static async Task<bool> SaveTaskAsync(TaskData task)
        {
            var payload = new MultipartFormDataContent();
            payload.Add(new StringContent(task.User), "username");
            payload.Add(new StringContent(task.Email), "email");
            payload.Add(new StringContent(task.Text), "text");

            await RequestPostAsync<TaskData>(new Uri(API_URL + "/create?" + PARAM_DEV), payload);

            TaskSaved?.Invoke();

            return true;
        }

        #endregion

        #region Additional methods
        public static void CheckInternetConnection()
        {
            NetworkAccess network = Connectivity.NetworkAccess;
            if (network == NetworkAccess.Internet || network == NetworkAccess.ConstrainedInternet)
                return;

            throw new NoInternetConnectionException(network);
        }

        private static async Task<T> GetContentAsync<T>(Task<HttpResponseMessage> msgTask)
        {
            HttpResponseMessage msg = await msgTask;
            string content = await msg.Content.ReadAsStringAsync();
            if (msg.StatusCode != HttpStatusCode.OK)
                throw new StatusCodeNotOkException(msg.StatusCode, content);

            try
            {
                ResponseData<T> res = JsonConvert.DeserializeObject<ResponseData<T>>(content);
                if (res.IsStatusOk())
                    return res.Data;
                else
                    throw new StatusCodeNotOkException(HttpStatusCode.BadRequest, res.GetErrorMessage());
            }
            catch
            {
                ResponseData<string> res = JsonConvert.DeserializeObject<ResponseData<string>>(content);
                throw new StatusCodeNotOkException(HttpStatusCode.BadRequest, res.GetErrorMessage());
            }
        }

        private static bool IsAuthTokenSaved()
        {
            return Preferences.ContainsKey(PREF_KEY_CUSTOMER_TOKEN) &&
                    Preferences.ContainsKey(PREF_KEY_CUSTOMER_TOKEN_CREATED) &&
                    Preferences.ContainsKey(PREF_KEY_CUSTOMER_LOGIN) &&
                    Preferences.ContainsKey(PREF_KEY_CUSTOMER_PASSWORD);
        }

        private static void LoadAuthToken()
        {
            string value = Preferences.Get(PREF_KEY_CUSTOMER_TOKEN, string.Empty);
            DateTime created = Preferences.Get(PREF_KEY_CUSTOMER_TOKEN_CREATED, default(DateTime));
            string login = Preferences.Get(PREF_KEY_CUSTOMER_LOGIN, string.Empty);
            string password = Preferences.Get(PREF_KEY_CUSTOMER_PASSWORD, string.Empty);

            if (value == string.Empty || created == default || login == string.Empty || password == string.Empty)
                return;

            authToken = new AuthToken(value, created, login, password);
        }

        private static async Task<bool> ReAuthIfNeeded()
        {
            if (authToken == null)
                IsLogined = false;

            if (!authToken.IsTimeOut())
                IsLogined = true;
            else
                IsLogined = await AuthAsync(authToken.Login, authToken.Password);
            return IsLogined;
        }

        private static void RemoveSavedAuthToken()
        {
            Preferences.Remove(PREF_KEY_CUSTOMER_TOKEN);
            Preferences.Remove(PREF_KEY_CUSTOMER_TOKEN_CREATED);
            Preferences.Remove(PREF_KEY_CUSTOMER_LOGIN);
            Preferences.Remove(PREF_KEY_CUSTOMER_PASSWORD);
        }

        private static Task<T> RequestGetAsync<T>(Uri uri)
        {
            CheckInternetConnection();
            return GetContentAsync<T>(client.GetAsync(uri));
        }
        private static Task<T> RequestPostAsync<T>(Uri uri, MultipartFormDataContent payload)
        {
            CheckInternetConnection();
            return GetContentAsync<T>(client.PostAsync(uri, payload));
        }

        private static void SaveAuthToken()
        {
            Preferences.Set(PREF_KEY_CUSTOMER_TOKEN, authToken.Value);
            Preferences.Set(PREF_KEY_CUSTOMER_TOKEN_CREATED, authToken.Created);
            Preferences.Set(PREF_KEY_CUSTOMER_LOGIN, authToken.Login);
            Preferences.Set(PREF_KEY_CUSTOMER_PASSWORD, authToken.Password);
        }
        #endregion
    }
}
