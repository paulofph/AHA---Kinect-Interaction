    using MongoDB.Bson;
    using MongoDB.Driver;

namespace Microsoft.Samples.Kinect.DiscreteGestureBasics
{
    using System;
    using System.ComponentModel;
    using System.Runtime.CompilerServices;
    using System.Windows.Media;
    using System.Windows.Media.Imaging;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    class MongoDBRecorder
    {
        private static IMongoClient _client;
        private static IMongoDatabase _database;
        private static IMongoCollection<BsonDocument> _collection;

        /// <summary> The body index (0-5) associated with the current gesture detector </summary>
        private int _bodyIndex = 0;

        /// <summary> Current confidence value reported by the discrete gesture </summary>
        private float _confidence = 0.0f;

        /// <summary> True, if the discrete gesture is currently being detected </summary>
        private bool _detected = false;

        /// <summary> True, if the body is currently being tracked </summary>
        private bool _isTracked = false;

        /// <summary>
        /// Initializes a new instance of the GestureResultView class and sets initial property values
        /// </summary>
        /// <param name="bodyIndex">Body Index associated with the current gesture detector</param>
        /// <param name="isTracked">True, if the body is currently tracked</param>
        /// <param name="detected">True, if the gesture is currently detected for the associated body</param>
        /// <param name="confidence">Confidence value for detection of the 'Seated' gesture</param>
        public MongoDBRecorder(int bodyIndex, bool isTracked, bool detected, float confidence)
        {
            var collName = "GestureBuildEvents";
            _client = new MongoClient();
            _database = _client.GetDatabase("AHA");

            CreateCappedCollectionAsync(collName);
            _collection = _database.GetCollection<BsonDocument>(collName);

            this._bodyIndex = bodyIndex;
            this._isTracked = isTracked;
            this._detected = detected;
            this._confidence = confidence;
        }

        /// <summary>
        /// Updates the values associated with the discrete gesture detection result
        /// </summary>
        /// <param name="isBodyTrackingIdValid">True, if the body associated with the GestureResultView object is still being tracked</param>
        /// <param name="isGestureDetected">True, if the discrete gesture is currently detected for the associated body</param>
        /// <param name="detectionConfidence">Confidence value for detection of the discrete gesture</param>
        public void UpdateMongoDB (bool isBodyTrackingIdValid, bool isGestureDetected, float detectionConfidence, string gestureName)
        {
            this._isTracked = isBodyTrackingIdValid;
            this._confidence = 0.0f;

            if (!this._isTracked)
            {
                this._detected = false;
            }
            else
            {
                this._detected = isGestureDetected;

                if (this._detected)
                {
                    this._confidence = detectionConfidence;

                    // add an element to the collection
                    var document = new BsonDocument
			        {
				        { "detected", this._detected },
                        { "confidence", this._confidence },
                        { "gesture", gestureName },
				        { "timeStamp", DateTime.UtcNow },
			        };
                    _collection.InsertOneAsync(document).Wait();
                }
                else
                {
                }
            }
        }

		private static void CreateCappedCollectionAsync(string collname)
		{
			_database.CreateCollectionAsync(collname, new CreateCollectionOptions
			{
				Capped = true,
				MaxSize = 2048,
				MaxDocuments = 100,
				});
		}

    }
}
