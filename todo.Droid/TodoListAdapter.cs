using System;
using System.Collections.Generic;
using Android.Content;
using Android.Views;
using Android.Widget;
using MongoDB.Bson;
using Stitch.Services.MongoDB;

namespace todo.Droid
{
    public class TodoListAdapter : ArrayAdapter<TodoItem>
    {
		private readonly MongoClient.Collection _itemSource;

		// Store the expected state of the items based off the users intentions. This is to handle this
		// series of events:
		// Check Item Request Begin - Item in state X
		// Refresh List - Item in state Y, View is refreshed
		// Check Item Request End - Item in State X
		// Refresh List - Item in state X, View is refreshed
		//
		// In this example app, these updates happen on the UI thread,
		// so no synchronization is necessary.
		private readonly Dictionary<ObjectId, Boolean> _itemState =
            new Dictionary<ObjectId, bool>();

        private readonly ClientManager _clientManager;

        public TodoListAdapter(Context context,
                               int resource,
                               List<TodoItem> items,
                               ClientManager clientManager,
                               MongoClient.Collection itemSource) :
        base(context, resource, items)
        {
            this._itemSource = itemSource;
            this._clientManager = clientManager;
        }

		public override View GetView(
			int position,
			View convertView,
			ViewGroup parent)
		{

			View row;
			if (convertView == null)
			{
				LayoutInflater inflater = 
                    (LayoutInflater)Context.GetSystemService(Context.LayoutInflaterService);
				row = inflater.Inflate(Resource.Layout.TodoItem, null);
			}
			else
			{
				// Reuse past view
				row = convertView;
			}

			var item = this.GetItem(position);

			// Hydrate data/event handlers
            ((TextView)row.FindViewById(Resource.Id.text)).Text = item.Text;

			var checkBox = (CheckBox)row.FindViewById(Resource.Id.checkBox);
			checkBox.SetOnCheckedChangeListener(null);

            if (_itemState.ContainsKey(item.Id))
			{
				checkBox.Checked = _itemState[item.Id];
			}
			else
			{
				checkBox.Checked = item.IsChecked;
			}

            checkBox.CheckedChange += async delegate (
                object sender,
                CompoundButton.CheckedChangeEventArgs e)
            {
                var query = new BsonDocument
                {
                    { "_id", new BsonDocument { { "$oid" , item.Id.ToString() } } }
                };

                var update = new BsonDocument
                    {
                        { "$set", new BsonDocument
                        {
                            {"checked", e.IsChecked }
                        }}
                    };

                _itemState[item.Id] = e.IsChecked;
                await _itemSource.UpdateOne(query, update, false);
                _itemState.Remove(item.Id);
            };

            return row;
        }
    }
}
