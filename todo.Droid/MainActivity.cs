using Android.App;
using Android.OS;
using Android.Util;
using Stitch.Auth;
using Xamarin.Facebook;
using Android.Views;
using Stitch.Android;
using System.Collections.Generic;
using Android.Widget;
using System.Threading.Tasks;
using Android.Content;

namespace todo.Droid
{
    [Activity(Name = "com.mongodb.stitch.examples.todo.MainActivity",
              Label = "todo",
              MainLauncher = true,
              Icon = "@mipmap/icon")]
    public class MainActivity : Android.Support.V7.App.AppCompatActivity
    {
        private const string Tag = "TodoApp";

        private ClientManager _clientManager;
        private TodoListAdapter _itemAdapter;

        private async Task RefreshList()
        {
			var result = await _clientManager.RefreshList();

			if (result.IsSuccessful)
			{
				_itemAdapter.Clear();
				_itemAdapter.AddAll(result.Value);
				_itemAdapter.NotifyDataSetChanged();
			}
			else
			{
                Log.Error(Tag, result.Error.Message);
                Toast.MakeText(this, 
                               Resource.String.error_refreshing_list, 
                               ToastLength.Long).Show();
			}
        }

        private async void InitTodoView()
        {
            SetContentView(Resource.Layout.ActivityMainTodoList);

            // Set up items
            _itemAdapter = new TodoListAdapter(
                    this,
                    Resource.Layout.TodoItem,
                    (await _clientManager.RefreshList()).Value ?? new List<TodoItem>(),
                    _clientManager,
                    _clientManager.GetItemsCollection());
            ((ListView)FindViewById(Resource.Id.todoList)).Adapter = _itemAdapter;

            FindViewById(Resource.Id.refresh).Click += async delegate
            {
                await RefreshList();
            };

            FindViewById(Resource.Id.clear).Click += async delegate
            {
                var result = await _clientManager.ClearChecked();

                if (result.IsSuccessful)
                {
                    await RefreshList();
                }
                else
                {
					Toast.MakeText(this,
                                   Resource.String.error_clearing_checked,
							       ToastLength.Long).Show();
                }
            };

            FindViewById(Resource.Id.logout).Click += async delegate
            {
                await _clientManager.StitchClient.Logout();
                SetContentView(Resource.Layout.ActivityMain);
            };

            FindViewById(Resource.Id.addItem).Click += delegate
            {
                var diagBuilder = new AlertDialog.Builder(this);

                LayoutInflater inflater = this.LayoutInflater;
                View view = inflater.Inflate(Resource.Layout.AddItem, null);
                EditText text = (EditText)view.FindViewById(Resource.Id.addItemText);

                diagBuilder.SetView(view);
                diagBuilder.SetPositiveButton(Resource.String.addOk,
                                              async delegate (object sender,
                                                        DialogClickEventArgs e)
                {
                    await _clientManager.AddItem(text.Text);
                    await RefreshList();
                });

                diagBuilder.SetNegativeButton(Resource.String.addCancel,
                                              (sender, e) => {
                  ((AlertDialog)sender).Cancel();
                });

                diagBuilder.SetCancelable(false);
                diagBuilder.Create().Show();
            };
        }

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

			_clientManager = new ClientManager(new StitchClient(
			    this,
			    "test-uybga"
		    ));

            var providersResult =
                await _clientManager.StitchClient.GetAuthProviders();

            if (!providersResult.IsSuccessful)
            {
                Log.Error(Tag,
                          "Error getting auth info: {0}",
                          providersResult.Error);
                return;
            }

            if (_clientManager.StitchClient.IsAuthenticated())
            {
                InitTodoView();
                return;
            }

            var availableProviders = providersResult.Value;

            FacebookSdk.ApplicationId =
                    availableProviders.FacebookProviderInfo?.AppId ?? "INVALID";

#pragma warning disable 0618
            FacebookSdk.SdkInitialize(this);
#pragma warning restore 0618

			SetContentView(Resource.Layout.ActivityMain);

			if (availableProviders.HasAnonymousProviderInfo)
            {
                var loginButton = FindViewById(Resource.Id.anonymous_login_button);

                loginButton.Click += async delegate
                {
                    var authResult = await this._clientManager
                                               .StitchClient
                                               .Login(new AnonymousAuthProvider());

                    if (authResult.IsSuccessful)
                    {
                        InitTodoView();
                    }
                };

                loginButton.Visibility = ViewStates.Visible;
            }
        }
    }
}
