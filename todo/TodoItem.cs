using System;
using MongoDB.Bson;

namespace todo
{
    public struct TodoItem
    {
        public readonly ObjectId Id;
        public readonly string Text;
        public readonly bool IsChecked;

        public TodoItem(BsonDocument document)
        {
            if (document["_id"].IsObjectId)
            {
				Id = document["_id"].AsObjectId;
            }
            else
            {
                Id = new ObjectId(document["_id"].AsString);
            }

            if (document.Contains("text"))
            {
				Text = document["text"]?.AsString ?? "<invalid>";
            }
            else
            {
                Text = "<invalid>";
            }

            if (document.Contains("checked"))
            {
				IsChecked = document["checked"].AsBoolean;
            }
            else
            {
                IsChecked = false;
            }
        }

        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return obj is TodoItem && this.Id.Equals(((TodoItem)obj).Id);
        }
    }
}
