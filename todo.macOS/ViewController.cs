using System;

using AppKit;
using Foundation;
using Stitch.Auth;
using Stitch.macOS;

namespace todo.macOS
{
    public partial class ViewController : NSViewController
    {
		private ClientManager _clientManager;

		public ViewController(IntPtr handle) : base(handle)
        {
        }

        private void InitTodoView()
        {
            
        }

        public async override void ViewDidLoad()
        {
            base.ViewDidLoad();

			_clientManager = new ClientManager(new StitchClient(
				"test-uybga"
			));

			var providersResult =
				await _clientManager.StitchClient.GetAuthProviders();

			if (!providersResult.IsSuccessful)
			{
				Console.Error.WriteLine("Error getting auth info: {0}",
						                providersResult.Error);
				return;
			}

			if (_clientManager.StitchClient.IsAuthenticated())
			{
				InitTodoView();
				return;
			}

			var availableProviders = providersResult.Value;

			if (availableProviders.HasAnonymousProviderInfo)
			{
                AnonymousLoginButton.Activated += async delegate
				{
					var authResult = await this._clientManager
											   .StitchClient
											   .Login(new AnonymousAuthProvider());

					if (authResult.IsSuccessful)
					{
						InitTodoView();
					}
				};

                AnonymousLoginButton.Transparent = false;
			}
            // Do any additional setup after loading the view.
        }

        public override NSObject RepresentedObject
        {
            get
            {
                return base.RepresentedObject;
            }
            set
            {
                base.RepresentedObject = value;
                // Update the view, if already loaded.
            }
        }
    }
}
