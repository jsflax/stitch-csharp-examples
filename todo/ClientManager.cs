using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using MongoDB.Bson;
using Stitch;
using Stitch.Services.MongoDB;

namespace todo
{
    public sealed class ClientManager
    {
        public readonly StitchClientBase StitchClient;
        public readonly MongoClient MongoClient;

        public ClientManager(StitchClientBase stitchClient)
        {
            this.StitchClient = stitchClient;
            this.MongoClient = new MongoClient(this.StitchClient,
                                               "mongodb-atlas");
        }

        public MongoClient.Collection GetItemsCollection()
        {
            return this.MongoClient.GetDatabase("todo").GetCollection("items");
        }

        public async Task<StitchResult<bool>> AddItem(string text)
        {
            var doc = new BsonDocument
            {
                { "owner_id", StitchClient.Auth.UserId },
                { "text", text },
                { "checked", false }
            };

            return await this.GetItemsCollection().InsertOne(doc);
        }

        public async Task<StitchResult<bool>> ClearChecked()
        {
            var query = new BsonDocument
            {
                { "owner_id", StitchClient.Auth.UserId },
                { "checked", true }
            };

            return await this.GetItemsCollection().DeleteMany(query);
        }

		public async Task<StitchResult<List<TodoItem>>> RefreshList()
		{
            var results = await GetItemsCollection().Find(
                new BsonDocument
                {
                    {"owner_id", StitchClient.Auth.UserId } 
                });

            if (!results.IsSuccessful)
            {
                return new StitchResult<List<TodoItem>>(false,
                                                        null,
                                                        results.Error);
            }

            return new StitchResult<List<TodoItem>>(true,
                                                    ConvertDocsToTodo(results.Value));
        }

        public List<TodoItem> ConvertDocsToTodo(List<BsonDocument> documents)
        {
            return documents.ConvertAll(doc => new TodoItem(doc));
        }
    }
}